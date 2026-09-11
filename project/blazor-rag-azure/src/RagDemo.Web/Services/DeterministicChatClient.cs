using System.Runtime.CompilerServices;
using Microsoft.Extensions.AI;

namespace RagDemo.Web.Services;

public sealed class DeterministicChatClient : IChatClient
{
    private const string Prefix = "(無 OpenAI key,以下為檢索到的參考段落摘要)";

    public Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
        => Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, Compose(messages))));

    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages, ChatOptions? options = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        foreach (var piece in Compose(messages).Split(' '))
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Task.Delay(20, cancellationToken);
            yield return new ChatResponseUpdate(ChatRole.Assistant, piece + " ");
        }
    }

    public object? GetService(Type serviceType, object? serviceKey = null)
        => serviceType.IsInstanceOfType(this) ? this : null;

    public void Dispose() { }

    private static string Compose(IEnumerable<ChatMessage> messages)
    {
        var user = messages.LastOrDefault(m => m.Role == ChatRole.User)?.Text ?? "";
        var firstRef = user.Split("\n\n", StringSplitOptions.RemoveEmptyEntries).Skip(1).FirstOrDefault() ?? "";
        var excerpt = firstRef.Length > 200 ? firstRef[..200] + "…" : firstRef;
        return $"{Prefix} {excerpt} [1]";
    }
}
