using Microsoft.Data.SqlTypes;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using RagDemo.Web.Data;
using RagDemo.Web.Services;

namespace RagDemo.Tests;

public class RagServiceTests
{
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddings =
        Substitute.For<IEmbeddingGenerator<string, Embedding<float>>>();
    private readonly IChunkRepository _repo = Substitute.For<IChunkRepository>();
    private readonly IChatClient _chat = Substitute.For<IChatClient>();

    public RagServiceTests()
    {
        _embeddings
            .GenerateAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<EmbeddingGenerationOptions?>(), Arg.Any<CancellationToken>())
            .Returns(new GeneratedEmbeddings<Embedding<float>>([new Embedding<float>(new float[DocumentChunk.EmbeddingDimensions])]));
    }

    private RagService CreateSut() => new(_embeddings, _repo, _chat, NullLogger<RagService>.Instance);

    [Fact]
    public async Task AskAsync_RelevantChunks_StreamsSourcesTokensThenDone()
    {
        _repo.SearchAsync(Arg.Any<SqlVector<float>>(), 5, Arg.Any<CancellationToken>())
            .Returns([Scored("sop.md", 3, 0.25, "回滾條件是失敗率超過 2%"), Scored("sop.md", 1, 0.40, "PR 需要核准")]);
        _chat.GetStreamingResponseAsync(Arg.Any<IEnumerable<ChatMessage>>(), Arg.Any<ChatOptions?>(), Arg.Any<CancellationToken>())
            .Returns(Stream("超過 ", "2% [1]"));

        var events = await CreateSut().AskAsync("回滾條件?").ToListAsync();

        var sources = Assert.IsType<SourcesEvent>(events[0]);
        Assert.Equal(2, sources.Sources.Count);
        Assert.Equal((1, "sop.md", 3, 0.25), (sources.Sources[0].Index, sources.Sources[0].SourceFile, sources.Sources[0].ChunkIndex, sources.Sources[0].Distance));

        var tokens = events.OfType<TokenEvent>().Select(t => t.Text).ToList();
        Assert.Equal(["超過 ", "2% [1]"], tokens);

        var done = Assert.IsType<DoneEvent>(events[^1]);
        Assert.True(done.Grounded);
    }

    [Fact]
    public async Task AskAsync_NothingWithinDistanceGate_ReturnsNotFoundWithoutCallingLlm()
    {
        _repo.SearchAsync(Arg.Any<SqlVector<float>>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns([Scored("notes.md", 0, RagService.MaxDistance + 0.05, "無關內容")]);

        var events = await CreateSut().AskAsync("台北天氣?").ToListAsync();

        Assert.Empty(Assert.IsType<SourcesEvent>(events[0]).Sources);
        Assert.Equal(RagService.NotFoundAnswer, Assert.IsType<TokenEvent>(events[1]).Text);
        Assert.False(Assert.IsType<DoneEvent>(events[2]).Grounded);
        _chat.DidNotReceiveWithAnyArgs().GetStreamingResponseAsync(default!, default, default);
    }

    [Fact]
    public async Task AskAsync_SkipsEmptyStreamUpdates()
    {
        _repo.SearchAsync(Arg.Any<SqlVector<float>>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns([Scored("a.md", 0, 0.1, "x")]);
        _chat.GetStreamingResponseAsync(Arg.Any<IEnumerable<ChatMessage>>(), Arg.Any<ChatOptions?>(), Arg.Any<CancellationToken>())
            .Returns(Stream("", "ok", ""));

        var tokens = await CreateSut().AskAsync("q").OfType<TokenEvent>().ToListAsync();

        Assert.Equal(["ok"], tokens.Select(t => t.Text));
    }

    [Fact]
    public void BuildMessages_PutsSystemRulesFirstAndNumbersEveryChunk()
    {
        var relevant = new[] { Scored("sop.md", 3, 0.2, "段落甲"), Scored("notes.md", 0, 0.3, "段落乙") };

        var messages = RagService.BuildMessages("問題?", relevant);

        Assert.Equal(2, messages.Count);
        Assert.Equal(ChatRole.System, messages[0].Role);
        Assert.Contains("文件中找不到相關內容", messages[0].Text);
        Assert.Equal(ChatRole.User, messages[1].Role);
        Assert.Contains("[1] (sop.md #3)", messages[1].Text);
        Assert.Contains("段落甲", messages[1].Text);
        Assert.Contains("[2] (notes.md #0)", messages[1].Text);
        Assert.Contains("問題:問題?", messages[1].Text);
    }

    private static ScoredChunk Scored(string file, int index, double distance, string content) => new(
        new DocumentChunk
        {
            SourceFile = file,
            ChunkIndex = index,
            Content = content,
            Embedding = new SqlVector<float>(new float[DocumentChunk.EmbeddingDimensions])
        },
        distance);

    private static async IAsyncEnumerable<ChatResponseUpdate> Stream(params string[] pieces)
    {
        foreach (var p in pieces)
        {
            await Task.Yield();
            yield return new ChatResponseUpdate(ChatRole.Assistant, p);
        }
    }
}
