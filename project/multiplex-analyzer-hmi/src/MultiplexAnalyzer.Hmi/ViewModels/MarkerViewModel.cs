using MultiplexAnalyzer.Hmi.Models;

namespace MultiplexAnalyzer.Hmi.ViewModels;

public sealed class MarkerViewModel
{
    public MarkerViewModel(MarkerResult result)
    {
        Name = result.Name;
        Signal = result.Signal;
        IsPositive = result.IsPositive;
    }

    public string Name { get; }

    public double Signal { get; }

    public bool IsPositive { get; }

    public string SignalLabel => Signal.ToString("0.00");

    public string CallLabel => IsPositive ? "POS" : "NEG";
}
