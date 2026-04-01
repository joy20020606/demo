using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SocketCommSimulator.Models;
using SocketCommSimulator.Services;

namespace SocketCommSimulator.ViewModels;

public partial class ServerViewModel : ObservableObject
{
    private readonly TcpServerService _server = new();
    private readonly ProtocolService _protocol = new();
    private readonly HeartbeatService _heartbeat = new();

    [ObservableProperty]
    private ConnectionInfo _connection = new() { Port = 9000 };

    [ObservableProperty]
    private string _messageToSend = string.Empty;

    [ObservableProperty]
    private bool _isListening = false;

    public LogViewModel Log { get; } = new();

    public ServerViewModel()
    {
        _server.OnLog += entry => Log.AddEntry(entry);
        _server.OnClientCountChanged += count =>
        {
            Application.Current?.Dispatcher.InvokeAsync(() =>
                Connection.ConnectedClients = count);
        };
        _server.OnMessageReceived += (session, msg) =>
        {
            if (msg.GetPayloadAsText() == "PING")
                _heartbeat.RecordHeartbeatReceived();
            _ = _server.BroadcastAsync(_protocol.PackMessage(msg.GetPayloadAsText()));
        };

        _heartbeat.OnLog += entry => Log.AddEntry(entry);
        _heartbeat.OnHeartbeatSend += () =>
        {
            if (IsListening)
                _ = _server.BroadcastAsync(_protocol.PackMessage("PING"));
        };
        _heartbeat.OnTimeout += () =>
        {
            Log.AddEntry(LogEntry.CreateSystem("Heartbeat 超時，部分 Client 可能已斷線"));
        };
    }

    [RelayCommand]
    private async Task StartServer()
    {
        if (IsListening) return;
        IsListening = true;
        Connection.State = ConnectionState.Listening;
        _heartbeat.Start();
        await _server.StartAsync(Connection.IpAddress, Connection.Port);
        IsListening = false;
        Connection.State = ConnectionState.Disconnected;
    }

    [RelayCommand]
    private void StopServer()
    {
        _heartbeat.Stop();
        _server.Stop();
        IsListening = false;
        Connection.State = ConnectionState.Disconnected;
    }

    [RelayCommand]
    private async Task Send()
    {
        if (string.IsNullOrWhiteSpace(MessageToSend)) return;
        var data = _protocol.PackMessage(MessageToSend);
        await _server.BroadcastAsync(data);
        MessageToSend = string.Empty;
    }
}
