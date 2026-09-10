using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Data.SqlTypes;

namespace RagDemo.Web.Data;

public class DocumentChunk
{
    public const int EmbeddingDimensions = 1536;

    public int Id { get; set; }

    public required string SourceFile { get; set; }

    public int ChunkIndex { get; set; }

    public required string Content { get; set; }

    [Column(TypeName = "vector(1536)")]
    public SqlVector<float> Embedding { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
