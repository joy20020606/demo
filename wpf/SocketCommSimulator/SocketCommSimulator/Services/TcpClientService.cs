using System.Net.Sockets;
using SocketCommSimulator.Models;

namespace SocketCommSimulator.Services;

public class TcpClientService
{
    private TcpClient? _client;
    private CancellationTokenSource? _cts;
    private readonly ProtocolService _protocol = new();
    private string _ip = "127.0.0.1";
    private int _port = 9000;
    private bool _autoReconnect = true;
    private int _reconnectDelay = 3000;

    public event Action<LogEntry>? OnLog;
    public event Action<ProtocolMessage>? OnMessageReceived;
    public event Action<bool>? OnConnectionChanged;

    public bool IsConnected => _client?.Connected ?? false;

    public async Task ConnectAsync(string ip, int port, bool autoReconnect = true)
    {
        _ip = ip;
        _port = port;
        _autoReconnect = autoReconnect;
        _cts = new CancellationTokenSource();

        await ConnectWithRetryAsync(_cts.Token);
    }

    private async Task ConnectWithRetryAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                _client = new TcpClient();
                OnLog?.Invoke(LogEntry.CreateSystem($"嘗試連線至 {_ip}:{_port}..."));
                await _client.ConnectAsync(_ip, _port, ct);
                _protocol.Reset();

                OnLog?.Invoke(LogEntry.CreateSystem($"已連線至 {_ip}:{_port}"));
                OnConnectionChanged?.Invoke(true);

                await ReceiveLoopAsync(_client, ct);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                OnLog?.Invoke(LogEntry.CreateSystem($"連線失敗: {ex.Message}"));
                OnConnectionChanged?.Invoke(false);
                _client?.Close();
            }

            if (!_autoReconnect || ct.IsCancellationRequested) break;

            OnLog?.Invoke(LogEntry.CreateSystem($"{_reconnectDelay / 1000} 秒後重試..."));
            try { await Task.Delay(_reconnectDelay, ct); }
            catch (OperationCanceledException) { break; }
        }

        OnConnectionChanged?.Invoke(false);
    }

    private async Task ReceiveLoopAsync(TcpClient client, CancellationToken ct)
    {
        var stream = client.GetStream();
        var buffer = new byte[4096];

        try
        {
            while (!ct.IsCancellationRequested && client.Connected)
            {
                int read = await stream.ReadAsync(buffer, ct);
                if (read == 0) break;

                var data = buffer[..read];
                OnLog?.Invoke(LogEntry.CreateReceived(data));

                var messages = _protocol.FeedData(data);
                foreach (var msg in messages)
                    OnMessageReceived?.Invoke(msg);
            }
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            OnLog?.Invoke(LogEntry.CreateSystem($"接收錯誤: {ex.Message}"));
        }

        OnLog?.Invoke(LogEntry.CreateSystem("連線已斷開"));
        OnConnectionChanged?.Invoke(false);
    }

    public async Task SendAsync(byte[] data)
    {
        if (_client?.Connected != true)
        {
            OnLog?.Invoke(LogEntry.CreateSystem("未連線，無法傳送"));
            return;
        }

        try
        {
            var stream = _client.GetStream();
            await stream.WriteAsync(data);
            OnLog?.Invoke(LogEntry.CreateSent(data));
        }
        catch (Exception ex)
        {
            OnLog?.Invoke(LogEntry.CreateSystem($"傳送失敗: {ex.Message}"));
        }
    }

    public void Disconnect()
    {
        _autoReconnect = false;
        _cts?.Cancel();
        _client?.Close();
        OnLog?.Invoke(LogEntry.CreateSystem("已主動斷線"));
    }
}
