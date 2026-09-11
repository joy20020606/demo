namespace RagDemo.Web.Services;

public sealed class ChunkingService(int maxChars = 600, int overlap = 100)
{
    public int MaxChars { get; } = maxChars;
    public int Overlap { get; } = overlap;

    private int Body => Overlap > 0 ? MaxChars - Overlap - 1 : MaxChars;

    public IReadOnlyList<string> Split(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return [];

        var normalized = text.Replace("\r\n", "\n").Replace('\r', '\n');
        var units = normalized
            .Split("\n\n", StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Trim())
            .Where(p => p.Length > 0)
            .SelectMany(HardSplit)
            .ToList();

        var body = Body;
        var chunks = new List<string>();
        var current = new List<string>();
        var currentLen = 0;

        foreach (var unit in units)
        {
            var extra = current.Count == 0 ? unit.Length : unit.Length + 2;
            if (current.Count > 0 && currentLen + extra > body)
            {
                chunks.Add(string.Join("\n\n", current));
                current.Clear();
                currentLen = 0;
                extra = unit.Length;
            }
            current.Add(unit);
            currentLen += extra;
        }
        if (current.Count > 0) chunks.Add(string.Join("\n\n", current));

        if (Overlap <= 0 || chunks.Count < 2) return chunks;

        var result = new List<string>(chunks.Count) { chunks[0] };
        for (var i = 1; i < chunks.Count; i++)
        {
            var prev = chunks[i - 1];
            var tail = prev.Length <= Overlap ? prev : prev[^Overlap..];
            result.Add(tail + "\n" + chunks[i]);
        }
        return result;
    }

    private IEnumerable<string> HardSplit(string paragraph)
    {
        var body = Body;
        if (paragraph.Length <= body)
        {
            yield return paragraph;
            yield break;
        }

        for (var start = 0; start < paragraph.Length; start += body)
        {
            var len = Math.Min(body, paragraph.Length - start);
            yield return paragraph.Substring(start, len);
        }
    }
}
