using Microsoft.Data.SqlTypes;
using Microsoft.Extensions.AI;
using RagDemo.Web.Data;

namespace RagDemo.Web.Services;

public sealed class IngestService(
    ChunkingService chunking,
    IEmbeddingGenerator<string, Embedding<float>> embeddings,
    IChunkRepository repo,
    ILogger<IngestService> logger)
{
    public async Task<int> IngestAsync(string fileName, string text, CancellationToken ct = default)
    {
        var name = Path.GetFileName(fileName);
        var pieces = chunking.Split(text);
        if (pieces.Count == 0) return 0;

        var generated = await embeddings.GenerateAsync(pieces, cancellationToken: ct);
        if (generated.Count != pieces.Count)
            throw new InvalidOperationException($"Embedding count mismatch: {generated.Count} vs {pieces.Count}");

        var now = DateTime.UtcNow;
        var chunks = pieces.Select((content, i) => new DocumentChunk
        {
            SourceFile = name,
            ChunkIndex = i,
            Content = content,
            Embedding = new SqlVector<float>(generated[i].Vector),
            CreatedAt = now
        }).ToList();

        var removed = await repo.DeleteDocumentAsync(name, ct);
        await repo.AddRangeAsync(chunks, ct);

        logger.LogInformation("Ingested {File}: {Chunks} chunks (replaced {Removed})", name, chunks.Count, removed);
        return chunks.Count;
    }
}
