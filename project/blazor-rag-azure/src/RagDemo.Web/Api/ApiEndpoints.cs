using System.ComponentModel.DataAnnotations;
using System.Net.ServerSentEvents;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http.HttpResults;
using RagDemo.Web.Data;
using RagDemo.Web.Services;

namespace RagDemo.Web.Api;

public static class ApiEndpoints
{
    private const long MaxUploadBytes = 2 * 1024 * 1024;
    private static readonly string[] AllowedExtensions = [".md", ".txt"];

    public static IEndpointRouteBuilder MapRagApi(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api").WithTags("RAG");

        api.MapGet("/ping", () => TypedResults.Ok(new PingResponse("pong", DateTimeOffset.UtcNow)))
           .WithName("Ping")
           .WithSummary("Health check");

        api.MapGet("/documents", async (IChunkRepository repo, CancellationToken ct) =>
                TypedResults.Ok(await repo.ListDocumentsAsync(ct)))
           .WithName("ListDocuments")
           .WithSummary("List ingested documents with chunk counts");

        api.MapPost("/documents", async Task<Results<Ok<IngestResponse>, BadRequest<string>>> (
                IFormFile file, IngestService ingest, CancellationToken ct) =>
            {
                if (file.Length == 0) return TypedResults.BadRequest("Empty file.");
                if (file.Length > MaxUploadBytes) return TypedResults.BadRequest("File exceeds 2 MB.");

                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!AllowedExtensions.Contains(ext)) return TypedResults.BadRequest("Only .md and .txt are accepted.");

                using var reader = new StreamReader(file.OpenReadStream(), Encoding.UTF8);
                var text = await reader.ReadToEndAsync(ct);
                var count = await ingest.IngestAsync(file.FileName, text, ct);

                return TypedResults.Ok(new IngestResponse(Path.GetFileName(file.FileName), count));
            })
           .DisableAntiforgery()
           .WithName("UploadDocument")
           .WithSummary("Upload a .md/.txt file: chunk, embed and store (re-upload replaces)");

        api.MapDelete("/documents/{file}", async Task<Results<Ok<DeleteResponse>, NotFound>> (
                string file, IChunkRepository repo, CancellationToken ct) =>
            {
                var removed = await repo.DeleteDocumentAsync(file, ct);
                return removed == 0 ? TypedResults.NotFound() : TypedResults.Ok(new DeleteResponse(file, removed));
            })
           .WithName("DeleteDocument")
           .WithSummary("Delete all chunks of a document");

        api.MapPost("/ask", (AskRequest req, RagService rag, CancellationToken ct) =>
                TypedResults.ServerSentEvents(StreamAsk(req, rag, ct)))
           .WithName("Ask")
           .WithSummary("Ask a question over the ingested documents (SSE: sources, token*, done)");

        return app;
    }

    private static async IAsyncEnumerable<SseItem<string>> StreamAsk(
        AskRequest req, RagService rag, [EnumeratorCancellation] CancellationToken ct)
    {
        await foreach (var ev in rag.AskAsync(req.Question, req.TopK, ct))
        {
            yield return ev switch
            {
                SourcesEvent s => new SseItem<string>(JsonSerializer.Serialize(s.Sources, JsonOptions), "sources"),
                TokenEvent t => new SseItem<string>(t.Text, "token"),
                DoneEvent d => new SseItem<string>(JsonSerializer.Serialize(new { d.Grounded }, JsonOptions), "done"),
                _ => throw new InvalidOperationException($"Unknown event {ev.GetType().Name}")
            };
        }
    }

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };
}

public record PingResponse(string Status, DateTimeOffset ServerTime);
public record IngestResponse(string SourceFile, int ChunkCount);
public record DeleteResponse(string SourceFile, int RemovedChunks);

public record AskRequest(
    [property: Required, MinLength(2), MaxLength(1000)] string Question,
    [property: Range(1, 20)] int TopK = 5);
