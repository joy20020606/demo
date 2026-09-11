using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Data.SqlTypes;
using Microsoft.Extensions.AI;
using RagDemo.Web.Data;

namespace RagDemo.Web.Services;

public record Citation(int Index, string SourceFile, int ChunkIndex, double Distance, string Snippet);

public abstract record RagEvent;
public sealed record SourcesEvent(IReadOnlyList<Citation> Sources) : RagEvent;
public sealed record TokenEvent(string Text) : RagEvent;
public sealed record DoneEvent(bool Grounded) : RagEvent;

public sealed class RagService(
    IEmbeddingGenerator<string, Embedding<float>> embeddings,
    IChunkRepository repo,
    IChatClient chat,
    ILogger<RagService> logger)
{
    public const string NotFoundAnswer = "文件中找不到相關內容。";
    public const double MaxDistance = 0.65;

    public async IAsyncEnumerable<RagEvent> AskAsync(
        string question, int topK = 5, [EnumeratorCancellation] CancellationToken ct = default)
    {
        var queryVector = await embeddings.GenerateVectorAsync(question, cancellationToken: ct);
        var hits = await repo.SearchAsync(new SqlVector<float>(queryVector), topK, ct);

        var relevant = hits.Where(h => h.Distance <= MaxDistance).ToList();
        var citations = relevant
            .Select((h, i) => new Citation(i + 1, h.Chunk.SourceFile, h.Chunk.ChunkIndex, Math.Round(h.Distance, 3), Snippet(h.Chunk.Content)))
            .ToList();

        yield return new SourcesEvent(citations);

        if (citations.Count == 0)
        {
            logger.LogInformation("No relevant chunks for question (best distance {Distance})", hits.FirstOrDefault()?.Distance);
            yield return new TokenEvent(NotFoundAnswer);
            yield return new DoneEvent(Grounded: false);
            yield break;
        }

        var messages = BuildMessages(question, relevant);
        await foreach (var update in chat.GetStreamingResponseAsync(messages, cancellationToken: ct))
        {
            if (!string.IsNullOrEmpty(update.Text))
                yield return new TokenEvent(update.Text);
        }

        yield return new DoneEvent(Grounded: true);
    }

    internal static List<ChatMessage> BuildMessages(string question, IReadOnlyList<ScoredChunk> relevant)
    {
        var context = new StringBuilder();
        for (var i = 0; i < relevant.Count; i++)
        {
            var c = relevant[i].Chunk;
            context.AppendLine($"[{i + 1}] ({c.SourceFile} #{c.ChunkIndex})");
            context.AppendLine(c.Content);
            context.AppendLine();
        }

        const string system = """
            你是內部技術文件問答助理。只能根據下方「參考段落」回答,不可使用參考段落以外的知識。
            回答時在句尾用 [n] 標註引用的段落編號。
            如果參考段落無法回答問題,只回覆:「文件中找不到相關內容。」
            使用繁體中文,簡潔直接。
            """;

        return
        [
            new ChatMessage(ChatRole.System, system),
            new ChatMessage(ChatRole.User, $"參考段落:\n\n{context}\n問題:{question}")
        ];
    }

    private static string Snippet(string content)
    {
        var s = content.Replace('\n', ' ').Trim();
        return s.Length <= 120 ? s : s[..120] + "…";
    }
}
