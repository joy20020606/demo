using Microsoft.Data.SqlTypes;

namespace RagDemo.Web.Data;

public record DocumentSummary(string SourceFile, int ChunkCount, DateTime CreatedAt);

public record ScoredChunk(DocumentChunk Chunk, double Distance);

public interface IChunkRepository
{
    Task AddRangeAsync(IEnumerable<DocumentChunk> chunks, CancellationToken ct = default);

    Task<IReadOnlyList<DocumentSummary>> ListDocumentsAsync(CancellationToken ct = default);

    Task<int> DeleteDocumentAsync(string sourceFile, CancellationToken ct = default);

    Task<IReadOnlyList<ScoredChunk>> SearchAsync(SqlVector<float> queryEmbedding, int topK, CancellationToken ct = default);
}
