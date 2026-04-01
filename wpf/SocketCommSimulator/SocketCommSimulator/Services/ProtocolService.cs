using SocketCommSimulator.Models;

namespace SocketCommSimulator.Services;

public class ProtocolService
{
    private readonly List<byte> _buffer = [];

    public byte[] PackMessage(string text)
    {
        var msg = ProtocolMessage.FromText(text);
        return msg.ToBytes();
    }

    public byte[] PackMessage(byte[] payload)
    {
        var msg = ProtocolMessage.FromPayload(payload);
        return msg.ToBytes();
    }

    public IEnumerable<ProtocolMessage> FeedData(byte[] data)
    {
        _buffer.AddRange(data);
        var results = new List<ProtocolMessage>();

        while (true)
        {
            var arr = _buffer.ToArray();
            var (msg, consumed) = ProtocolMessage.TryParse(arr);

            if (consumed > 0)
                _buffer.RemoveRange(0, consumed);

            if (msg != null)
                results.Add(msg);
            else
                break;
        }

        return results;
    }

    public void Reset() => _buffer.Clear();
}
