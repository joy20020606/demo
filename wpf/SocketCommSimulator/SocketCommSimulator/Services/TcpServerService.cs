using System.Net;
using System.Net.Sockets;
using SocketCommSimulator.Models;

namespace SocketCommSimulator.Services;

public class ClientSession
{
    public string Id { get; init; } = Guid.NewGuid().ToString()[..8];
    public string EndPoint { get; init; } = string.Empty;
    public TcpClient Client { get; init; } = null!;
    public ProtocolService Protocol { get; } = new();
}

public class TcpServerService
{
    private TcpListener? _listener;
    private CancellationTokenSource? _cts;
    private readonly List<ClientSession> _sessions = [];
    private readonly object _lock = new();

    public event Action<LogEntry>? OnLog;
    public event Action<int>? OnClientCountChanged;
    public event Action<ClientSession, ProtocolMessage>? OnMessageReceived;

    public int ClientCount
    {
        get { lock (_lock) return _sessions.Count; }
    }

    public async Task StartAsync(string ip, int port)
    {
        _cts = new CancellationTokenSource();
        var endpoint = new IPEndPoint(IPAddress.Parse(ip), port);
        _listener = new TcpListener(endpoint);
        _listener.Start();

        OnLog?.Invoke(LogEntry.CreateSystem($"Server 啟動於 {ip}:{port}"));

        try
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                var tcpClient = await _listener.AcceptTcpClientAsync(_cts.Token);
                var ep = tcpClient.Client.RemoteEndPoint?.ToString() ?? "unknown";
                var session = new ClientSession { Client = tcpClient, EndPoint = ep };

                lock (_lock) _sessions.Add(session);
                OnClientCountChanged?.Invoke(ClientCount);
                OnLog?.Invoke(LogEntry.CreateSystem($"Client 連入: {ep} (ID: {session.Id})"));

                _ = HandleClientAsync(session, _cts.Token);
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            OnLog?.Invoke(LogEntry.CreateSystem($"Server 錯誤: {ex.Message}"));
        }
    }

    private async Task HandleClientAsync(ClientSession session, CancellationToken ct)
    {
        var stream = session.Client.GetStream();
        var buffer = new byte[4096];

        try
        {
            while (!ct.IsCancellationRequested && session.Client.Connected)
            {
                int read = await stream.ReadAsync(buffer, ct);
                if (read == 0) break;

                var data = buffer[..read];
                OnLog?.Invoke(LogEntry.CreateReceived(data, session.EndPoint));

                var messages = session.Protocol.FeedData(data);
                foreach (var msg in messages)
                    OnMessageReceived?.Invoke(session, msg);
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            OnLog?.Invoke(LogEntry.CreateSystem($"Client {session.EndPoint} 錯誤: {ex.Message}"));
        }
        finally
        {
            session.Client.Close();
            lock (_lock) _sessions.Remove(session);
            OnClientCountChanged?.Invoke(ClientCount);
            OnLog?.Invoke(LogEntry.CreateSystem($"Client 離線: {session.EndPoint}"));
        }
    }

    public async Task BroadcastAsync(byte[] data)
    {
        List<ClientSession> sessions;
        lock (_lock) sessions = [.._sessions];

        foreach (var session in sessions)
        {
            try
            {
                var stream = session.Client.GetStream();
                await stream.WriteAsync(data);
                OnLog?.Invoke(LogEntry.CreateSent(data, session.EndPoint));
            }
            catch (Exception ex)
            {
                OnLog?.Invoke(LogEntry.CreateSystem($"傳送至 {session.EndPoint} 失敗: {ex.Message}"));
            }
        }
    }

    public async Task SendToAsync(ClientSession session, byte[] data)
    {
        try
        {
            var stream = session.Client.GetStream();
            await stream.WriteAsync(data);
            OnLog?.Invoke(LogEntry.CreateSent(data, session.EndPoint));
        }
        catch (Exception ex)
        {
            OnLog?.Invoke(LogEntry.CreateSystem($"傳送失敗: {ex.Message}"));
        }
    }

    public void Stop()
    {
        _cts?.Cancel();
        _listener?.Stop();
        lock (_lock)
        {
            foreach (var s in _sessions) s.Client.Close();
            _sessions.Clear();
        }
        OnLog?.Invoke(LogEntry.CreateSystem("Server 已停止"));
    }
}
