using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DefectClassifier.Models;
using DefectClassifier.Services;
using DefectClassifier.Services.ClassificationStrategy;

namespace DefectClassifier.ViewModels;

public partial class ClassificationViewModel : ObservableObject
{
    private readonly DefectRepository _repository;
    private readonly BrowserViewModel _browserVM;

    public event Action<string>? StatusChanged;

    public ObservableCollection<IClassificationStrategy> Strategies { get; }

    [ObservableProperty]
    private IClassificationStrategy? _selectedStrategy;

    public ObservableCollection<ClassificationResult> Results { get; } = new();

    public ClassificationViewModel(
        DefectRepository repository,
        IReadOnlyList<IClassificationStrategy> strategies,
        BrowserViewModel browserVM)
    {
        _repository = repository;
        _browserVM = browserVM;
        Strategies = new ObservableCollection<IClassificationStrategy>(strategies);
        SelectedStrategy = Strategies.FirstOrDefault();
    }

    public void Refresh() { }

    [RelayCommand]
    private void RunClassification()
    {
        if (SelectedStrategy == null) return;
        var records = _repository.GetAll();
        Results.Clear();
        foreach (var record in records)
            Results.Add(SelectedStrategy.Classify(record));
        StatusChanged?.Invoke($"已使用「{SelectedStrategy.Name}」完成 {Results.Count} 筆分類建議");
    }

    [RelayCommand]
    private void ApplyClassification()
    {
        if (Results.Count == 0) return;
        var map = _repository.GetAll().ToDictionary(r => r.Id);
        foreach (var result in Results)
        {
            if (map.TryGetValue(result.DefectId, out var record))
            {
                record.Type = result.SuggestedType;
                _repository.Update(record);
            }
        }
        _browserVM.LoadDefects();
        StatusChanged?.Invoke($"已套用 {Results.Count} 筆分類結果");
    }
}
