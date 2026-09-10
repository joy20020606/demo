using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.AI;
using RagDemo.Web.Data;

namespace RagDemo.Web.Services;

public sealed class DeterministicEmbeddingGenerator : IEmbeddingGenerator<string, Embedding<float>>
{
    public Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(
        IEnumerable<string> values, EmbeddingGenerationOptions? options = null, CancellationToken cancellationToken = default)
    {
        var result = new GeneratedEmbeddings<Embedding<float>>(values.Select(v => new Embedding<float>(Vectorize(v))));
        return Task.FromResult(result);
    }

    public object? GetService(Type serviceType, object? serviceKey = null)
        => serviceType.IsInstanceOfType(this) ? this : null;

    public void Dispose() { }

    private static float[] Vectorize(string text)
    {
        var vec = new float[DocumentChunk.EmbeddingDimensions];
        foreach (var token in text.ToLowerInvariant().Split([' ', '\n', ',', '.', '。', ',', '、'], StringSplitOptions.RemoveEmptyEntries))
        {
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
            var idx = BitConverter.ToUInt16(hash, 0) % vec.Length;
            vec[idx] += 1f;
        }

        var norm = MathF.Sqrt(vec.Sum(x => x * x));
        if (norm > 0) for (var i = 0; i < vec.Length; i++) vec[i] /= norm;
        else vec[0] = 1f;
        return vec;
    }
}
