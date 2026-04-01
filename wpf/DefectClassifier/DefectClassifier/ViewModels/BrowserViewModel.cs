using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DefectClassifier.Models;
using DefectClassifier.Services;
using DefectClassifier.Services.ClassificationStrategy;

namespace DefectClassifier.ViewModels;

public partial class BrowserViewModel : ObservableObject
{
    private readonly DefectRepository _repository;
    private readonly IReadOnlyList<IClassificationStrategy> _strategies;

    public event Action<string>? StatusChanged;

    public ObservableCollection<DefectRecord> Defects { get; } = new();

    [ObservableProperty]
    private DefectRecord? _selectedDefect;

    [ObservableProperty]
    private string _filterWaferId = string.Empty;

    public IReadOnlyList<DefectType> ClassificationTypes =>
        Enum.GetValues<DefectType>().Where(t => t != DefectType.Unknown).ToArray();

    public BrowserViewModel(DefectRepository repository, IReadOnlyList<IClassificationStrategy> strategies)
    {
        _repository = repository;
        _strategies = strategies;
        LoadDefects();
    }

    public void LoadDefects()
    {
        Defects.Clear();
        foreach (var d in _repository.GetAll())
            Defects.Add(d);
        StatusChanged?.Invoke($"已載入 {Defects.Count} 筆缺陷記錄");
    }

    [RelayCommand]
    private void ApplyFilter()
    {
        Defects.Clear();
        var all = _repository.GetAll().AsEnumerable();
        if (!string.IsNullOrWhiteSpace(FilterWaferId))
            all = all.Where(d => d.WaferId.Contains(FilterWaferId, StringComparison.OrdinalIgnoreCase));
        foreach (var d in all)
            Defects.Add(d);
        StatusChanged?.Invoke($"篩選結果：{Defects.Count} 筆");
    }

    [RelayCommand]
    private void ClearFilter()
    {
        FilterWaferId = string.Empty;
        LoadDefects();
    }

    [RelayCommand]
    private void SetDefectType(DefectType type)
    {
        if (SelectedDefect == null) return;
        var defect = SelectedDefect;
        defect.Type = type;
        _repository.Update(defect);

        var idx = Defects.IndexOf(defect);
        if (idx >= 0)
        {
            Defects.RemoveAt(idx);
            Defects.Insert(idx, defect);
            SelectedDefect = Defects[idx];
        }

        StatusChanged?.Invoke($"已將缺陷 #{defect.Id} 分類為 {GetTypeName(type)}");
    }

    [RelayCommand]
    private void DeleteDefect()
    {
        if (SelectedDefect == null) return;
        _repository.Delete(SelectedDefect.Id);
        Defects.Remove(SelectedDefect);
        SelectedDefect = null;
        StatusChanged?.Invoke("已刪除缺陷記錄");
    }

    public static string GetTypeName(DefectType type) => type switch
    {
        DefectType.Scratch => "刮傷",
        DefectType.Particle => "粒子",
        DefectType.PatternDefect => "圖案缺陷",
        DefectType.Pit => "坑洞",
        DefectType.Bridge => "橋接",
        _ => "未知"
    };
}
