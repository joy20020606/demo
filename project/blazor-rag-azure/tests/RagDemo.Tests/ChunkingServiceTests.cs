using RagDemo.Web.Services;

namespace RagDemo.Tests;

public class ChunkingServiceTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   \n\n  ")]
    public void Split_EmptyOrWhitespace_ReturnsNoChunks(string text)
    {
        var sut = new ChunkingService();

        Assert.Empty(sut.Split(text));
    }

    [Fact]
    public void Split_ShortText_ReturnsSingleChunk()
    {
        var sut = new ChunkingService(maxChars: 600, overlap: 100);

        var chunks = sut.Split("第一段。\n\n第二段。");

        var chunk = Assert.Single(chunks);
        Assert.Equal("第一段。\n\n第二段。", chunk);
    }

    [Fact]
    public void Split_ManyParagraphs_RespectsMaxCharsAndOverlap()
    {
        var sut = new ChunkingService(maxChars: 100, overlap: 20);
        var paragraphs = Enumerable.Range(1, 8).Select(i => new string((char)('a' + i), 30));
        var text = string.Join("\n\n", paragraphs);

        var chunks = sut.Split(text);

        Assert.True(chunks.Count > 1);
        Assert.All(chunks, c => Assert.True(c.Length <= 100, $"chunk too long: {c.Length}"));
        for (var i = 1; i < chunks.Count; i++)
        {
            var tail = chunks[i - 1][^20..];
            Assert.StartsWith(tail, chunks[i]);
        }
    }

    [Fact]
    public void Split_ParagraphLongerThanBody_IsHardSplit()
    {
        var sut = new ChunkingService(maxChars: 50, overlap: 10);
        var text = new string('x', 130);

        var chunks = sut.Split(text);

        Assert.Equal(4, chunks.Count);
        Assert.All(chunks, c => Assert.True(c.Length <= 50));
    }

    [Fact]
    public void Split_NormalizesWindowsNewlines()
    {
        var sut = new ChunkingService();

        var chunks = sut.Split("A\r\n\r\nB");

        Assert.Equal("A\n\nB", Assert.Single(chunks));
    }
}
