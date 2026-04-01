namespace SocketCommSimulator.Models;

public class ProtocolMessage
{
    public const byte HeaderByte1 = 0xAA;
    public const byte HeaderByte2 = 0x55;

    public byte[] Header { get; set; } = [HeaderByte1, HeaderByte2];
    public ushort Length { get; set; }
    public byte[] Payload { get; set; } = [];
    public byte Checksum { get; set; }

    public static ProtocolMessage FromPayload(byte[] payload)
    {
        var msg = new ProtocolMessage
        {
            Payload = payload,
            Length = (ushort)payload.Length
        };
        msg.Checksum = CalculateChecksum(payload);
        return msg;
    }

    public static ProtocolMessage FromText(string text)
    {
        var payload = System.Text.Encoding.UTF8.GetBytes(text);
        return FromPayload(payload);
    }

    public byte[] ToBytes()
    {
        var buffer = new byte[2 + 2 + Payload.Length + 1];
        buffer[0] = Header[0];
        buffer[1] = Header[1];
        buffer[2] = (byte)(Length >> 8);
        buffer[3] = (byte)(Length & 0xFF);
        Array.Copy(Payload, 0, buffer, 4, Payload.Length);
        buffer[^1] = Checksum;
        return buffer;
    }

    public static (ProtocolMessage? message, int consumed) TryParse(byte[] data)
    {
        if (data.Length < 5) return (null, 0);

        int startIdx = -1;
        for (int i = 0; i < data.Length - 1; i++)
        {
            if (data[i] == HeaderByte1 && data[i + 1] == HeaderByte2)
            {
                startIdx = i;
                break;
            }
        }

        if (startIdx < 0) return (null, data.Length);
        if (data.Length - startIdx < 5) return (null, startIdx);

        ushort length = (ushort)((data[startIdx + 2] << 8) | data[startIdx + 3]);
        int totalSize = 2 + 2 + length + 1;

        if (data.Length - startIdx < totalSize) return (null, startIdx);

        var payload = new byte[length];
        Array.Copy(data, startIdx + 4, payload, 0, length);

        byte expectedChecksum = CalculateChecksum(payload);
        byte actualChecksum = data[startIdx + 4 + length];

        if (expectedChecksum != actualChecksum) return (null, startIdx + totalSize);

        var msg = new ProtocolMessage
        {
            Payload = payload,
            Length = length,
            Checksum = actualChecksum
        };

        return (msg, startIdx + totalSize);
    }

    public static byte CalculateChecksum(byte[] data)
    {
        byte checksum = 0;
        foreach (var b in data) checksum ^= b;
        return checksum;
    }

    public string GetPayloadAsText() => System.Text.Encoding.UTF8.GetString(Payload);

    public string GetPayloadAsHex() => BitConverter.ToString(Payload).Replace("-", " ");
}
