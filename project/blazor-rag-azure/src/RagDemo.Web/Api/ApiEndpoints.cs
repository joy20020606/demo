using RagDemo.Web.Data;
namespace RagDemo.Web.Api;

public static class ApiEndpoints
{
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

        return app;
    }
}

public record PingResponse(string Status, DateTimeOffset ServerTime);
