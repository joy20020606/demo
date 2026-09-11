using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using RagDemo.Web.Data;
using RagDemo.Web.Services;

namespace RagDemo.Tests;

public class IngestServiceTests
{
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddings =
        Substitute.For<IEmbeddingGenerator<string, Embedding<float>>>();
    private readonly IChunkRepository _repo = Substitute.For<IChunkRepository>();

    private IngestService CreateSut(int maxChars = 50, int overlap = 10) =>
        new(new ChunkingService(maxChars, overlap), _embeddings, _repo, NullLogger<IngestService>.Instance);

    [Fact]
    public async Task IngestAsync_ChunksEmbedsReplacesAndStores()
    {
        _embeddings
            .GenerateAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<EmbeddingGenerationOptions?>(), Arg.Any<CancellationToken>())
            .Returns(ci => FakeEmbeddings(ci.Arg<IEnumerable<string>>().Count()));
        _repo.DeleteDocumentAsync("guide.md", Arg.Any<CancellationToken>()).Returns(2);

        var text = string.Join("\n\n", Enumerable.Range(1, 5).Select(i => new string((char)('a' + i), 30)));
        var count = await CreateSut().IngestAsync("C:\\somewhere\\guide.md", text);

        Assert.True(count > 1);
        await _repo.Received(1).DeleteDocumentAsync("guide.md", Arg.Any<CancellationToken>());
        await _repo.Received(1).AddRangeAsync(
            Arg.Is<IEnumerable<DocumentChunk>>(chunks => AllWellFormed(chunks.ToList(), count)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task IngestAsync_EmptyText_StoresNothing()
    {
        var count = await CreateSut().IngestAsync("empty.md", "   ");

        Assert.Equal(0, count);
        await _embeddings.DidNotReceiveWithAnyArgs().GenerateAsync(default!, default, default);
        await _repo.DidNotReceiveWithAnyArgs().AddRangeAsync(default!, default);
    }

    [Fact]
    public async Task IngestAsync_EmbeddingCountMismatch_Throws()
    {
        _embeddings
            .GenerateAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<EmbeddingGenerationOptions?>(), Arg.Any<CancellationToken>())
            .Returns(FakeEmbeddings(1));

        var text = string.Join("\n\n", Enumerable.Range(1, 5).Select(i => new string('x', 30)));

        await Assert.ThrowsAsync<InvalidOperationException>(() => CreateSut().IngestAsync("bad.md", text));
        await _repo.DidNotReceiveWithAnyArgs().AddRangeAsync(default!, default);
    }

    private static bool AllWellFormed(List<DocumentChunk> chunks, int expectedCount)
        => chunks.Count == expectedCount
           && chunks.Select(c => c.ChunkIndex).SequenceEqual(Enumerable.Range(0, expectedCount))
           && chunks.All(c => c.SourceFile == "guide.md")
           && chunks.All(c => c.Embedding.Length == DocumentChunk.EmbeddingDimensions);

    private static GeneratedEmbeddings<Embedding<float>> FakeEmbeddings(int n)
        => new(Enumerable.Range(0, n).Select(_ => new Embedding<float>(new float[DocumentChunk.EmbeddingDimensions])));
}
