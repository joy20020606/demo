using Microsoft.EntityFrameworkCore;

namespace RagDemo.Web.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<DocumentChunk> Chunks => Set<DocumentChunk>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DocumentChunk>(e =>
        {
            e.ToTable("DocumentChunks");
            e.Property(c => c.SourceFile).HasMaxLength(260);
            e.HasIndex(c => new { c.SourceFile, c.ChunkIndex }).IsUnique();
        });
    }
}
