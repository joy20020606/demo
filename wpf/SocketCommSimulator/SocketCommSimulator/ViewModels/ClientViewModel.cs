using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SocketCommSimulator.Models;
using SocketCommSimulator.Services;

namespace SocketCommSimulator.ViewModels;

public partial class ClientViewModel : ObservableObject
{
    private readonly TcpClientService _client = new();
    private readonly ProtocolService _protocol = new();
    private readonly HeartbeatService _heartbeat = new();

    [ObservableProperty]
    private ConnectionInfo _connection = new() { Port = 9000 };

    [ObservableProperty]
    private string _messageToSend = string.Empty;

    [ObservableProperty]
    private bool _autoReconnect = true;

    public LogViewModel Log { get; } = new();

    public ClientViewModel()
    {
        _client.OnLog += entry => Log.AddEntry(entry);
        _client.OnConnectionChanged += connected =>
        {
            Connection.State = connected ? ConnectionState.Connected : ConnectionState.Disconnected;
            if (connected) _heartbeat.Start();
            else _heartbeat.Stop();
        };
        _client.OnMessageReceived += msg =>
        {
            if (msg.GetPayloadAsText() == "PING")
            {
                _heartbeat.RecordHeartbeatReceived();
                _ = _client.SendAsync(_protocol.PackMessage("PONG"));
            }
        };

        _heartbeat.OnLog += entry => Log.AddEntry(entry);
        _heartbeat.OnHeartbeatSend += () =>
        {
            if (Connection.State == ConnectionState.Connected)
                _ = _client.SendAsync(_protocol.PackMessage("PING"));
        };
        _heartbeat.OnTimeout += () =>
        {
            Log.AddEntry(LogEntry.CreateSystem("Heartbeat 超時，嘗試重新連線"));
            _client.Disconnect();
            if (AutoReconnect)
                _ = _client.ConnectAsync(Connection.IpAddress, Connection.Port, true);
        };
    }

    [RelayCommand]
    private async Task Connect()
    {
        Connection.State = ConnectionState.Connecting;
        await _client.ConnectAsync(Connection.IpAddress, Connection.Port, AutoReconnect);
    }

    [RelayCommand]
    private void Disconnect()
    {
        _heartbeat.Stop();
        _client.Disconnect();
        Connection.State = ConnectionState.Disconnected;
    }

    [RelayCommand]
    private async Task Send()
    {
        if (string.IsNullOrWhiteSpace(MessageToSend)) return;
        var data = _protocol.PackMessage(MessageToSend);
        await _client.SendAsync(data);
        MessageToSend = string.Empty;
    }
}
