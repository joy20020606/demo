using CommunityToolkit.Mvvm.ComponentModel;

namespace SocketCommSimulator.Models;

public enum ConnectionState
{
    Disconnected,
    Connecting,
    Connected,
    Listening
}

public partial class ConnectionInfo : ObservableObject
{
    [ObservableProperty]
    private string _ipAddress = "127.0.0.1";

    [ObservableProperty]
    private int _port = 9000;

    [ObservableProperty]
    private ConnectionState _state = ConnectionState.Disconnected;

    [ObservableProperty]
    private string _statusText = "已斷線";

    [ObservableProperty]
    private int _connectedClients = 0;

    partial void OnStateChanged(ConnectionState value)
    {
        StatusText = value switch
        {
            ConnectionState.Disconnected => "已斷線",
            ConnectionState.Connecting => "連線中...",
            ConnectionState.Connected => "已連線",
            ConnectionState.Listening => "監聽中",
            _ => "未知"
        };
    }
}
