using CommunityToolkit.Mvvm.ComponentModel;

namespace SocketCommSimulator.ViewModels;

public partial class MainViewModel : ObservableObject
{
    public ServerViewModel Server { get; } = new();
    public ClientViewModel Client { get; } = new();
}
