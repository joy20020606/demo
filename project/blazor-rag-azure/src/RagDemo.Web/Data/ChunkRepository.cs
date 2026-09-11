using Microsoft.Data.SqlTypes;
using Microsoft.EntityFrameworkCore;

namespace RagDemo.Web.Data;

public class ChunkRepository(IDbContextFactory<AppDbContext> factory) : IChunkRepository
{
    public async Task AddRangeAsync(IEnumerable<DocumentChunk> chunks, CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        db.Chunks.AddRange(chunks);
        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<DocumentSummary>> ListDocumentsAsync(CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        var rows = await db.Chunks
            .GroupBy(c => c.SourceFile)
            .Select(g => new { SourceFile = g.Key, Count = g.Count(), CreatedAt = g.Min(c => c.CreatedAt) })
            .OrderBy(r => r.SourceFile)
            .ToListAsync(ct);

        return rows.Select(r => new DocumentSummary(r.SourceFile, r.Count, r.CreatedAt)).ToList();
    }

    public async Task<int> DeleteDocumentAsync(string sourceFile, CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        return await db.Chunks.Where(c => c.SourceFile == sourceFile).ExecuteDeleteAsync(ct);
    }

    public async Task<IReadOnlyList<ScoredChunk>> SearchAsync(SqlVector<float> queryEmbedding, int topK, CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        var rows = await db.Chunks
            .Select(c => new { Chunk = c, Distance = EF.Functions.VectorDistance("cosine", c.Embedding, queryEmbedding) })
            .OrderBy(x => x.Distance)
            .Take(topK)
            .ToListAsync(ct);

        return rows.Select(r => new ScoredChunk(r.Chunk, r.Distance)).ToList();
    }
}
