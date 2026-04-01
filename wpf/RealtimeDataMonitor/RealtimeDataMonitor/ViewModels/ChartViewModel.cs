using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using RealtimeDataMonitor.Models;

namespace RealtimeDataMonitor.ViewModels;

public partial class ChartViewModel : ObservableObject
{
    private const int MaxDisplayPoints = 500;

    private readonly List<ChannelConfig> _channels;
    private readonly Dictionary<int, LineSeries> _seriesMap = new();
    private readonly Dictionary<int, List<SensorData>> _historyMap = new();

    [ObservableProperty]
    private PlotModel _plotModel;

    [ObservableProperty]
    private double _timeWindowSeconds = 30;

    private DateTimeAxis? _timeAxis;

    public ChartViewModel(List<ChannelConfig> channels)
    {
        _channels = channels;
        _plotModel = BuildPlotModel();

        foreach (var ch in channels)
            _historyMap[ch.Id] = new List<SensorData>(4096);
    }

    private PlotModel BuildPlotModel()
    {
        var model = new PlotModel
        {
            Background = OxyColor.FromRgb(18, 18, 24),
            PlotAreaBorderColor = OxyColor.FromRgb(60, 60, 80),
            TextColor = OxyColors.LightGray,
        };

        _timeAxis = new DateTimeAxis
        {
            Position = AxisPosition.Bottom,
            StringFormat = "HH:mm:ss",
            MajorGridlineStyle = LineStyle.Dot,
            MajorGridlineColor = OxyColor.FromRgb(50, 50, 65),
            TicklineColor = OxyColors.LightGray,
            TextColor = OxyColors.LightGray,
            TitleColor = OxyColors.LightGray,
        };

        var valueAxis = new LinearAxis
        {
            Position = AxisPosition.Left,
            Title = "Value",
            MajorGridlineStyle = LineStyle.Dot,
            MajorGridlineColor = OxyColor.FromRgb(50, 50, 65),
            TicklineColor = OxyColors.LightGray,
            TextColor = OxyColors.LightGray,
            TitleColor = OxyColors.LightGray,
            Minimum = -10,
            Maximum = 110,
        };

        model.Axes.Add(_timeAxis);
        model.Axes.Add(valueAxis);

        foreach (var ch in _channels)
        {
            var series = new LineSeries
            {
                Title = ch.Name,
                Color = ch.Color,
                StrokeThickness = 1.5,
                MarkerType = MarkerType.None,
            };
            model.Series.Add(series);
            _seriesMap[ch.Id] = series;
        }

        return model;
    }

    public void AppendData(IReadOnlyList<SensorData> newData)
    {
        foreach (var d in newData)
        {
            if (!_historyMap.TryGetValue(d.ChannelId, out var history)) continue;
            history.Add(d);
        }

        var now = DateTime.Now;
        var windowStart = now - TimeSpan.FromSeconds(TimeWindowSeconds);

        foreach (var ch in _channels)
        {
            if (!_seriesMap.TryGetValue(ch.Id, out var series)) continue;
            if (!_historyMap.TryGetValue(ch.Id, out var history)) continue;

            var visible = history
                .Where(d => d.Timestamp >= windowStart)
                .ToList();

            var step = Math.Max(1, visible.Count / MaxDisplayPoints);
            series.Points.Clear();
            for (int i = 0; i < visible.Count; i += step)
            {
                var d = visible[i];
                series.Points.Add(new DataPoint(
                    DateTimeAxis.ToDouble(d.Timestamp),
                    d.Value));
            }
        }

        if (_timeAxis != null)
        {
            _timeAxis.Minimum = DateTimeAxis.ToDouble(windowStart);
            _timeAxis.Maximum = DateTimeAxis.ToDouble(now);
        }

        Application.Current?.Dispatcher.Invoke(() =>
            PlotModel.InvalidatePlot(true));
    }

    public IReadOnlyList<SensorData> GetAllHistory()
    {
        return _historyMap.Values
            .SelectMany(x => x)
            .OrderBy(x => x.Timestamp)
            .ToList();
    }

    public void Clear()
    {
        foreach (var h in _historyMap.Values) h.Clear();
        foreach (var s in _seriesMap.Values) s.Points.Clear();
        PlotModel.InvalidatePlot(true);
    }
}
