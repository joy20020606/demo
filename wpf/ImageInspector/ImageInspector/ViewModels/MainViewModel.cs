using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImageInspector.Models;
using ImageInspector.Services;
using Microsoft.Win32;

namespace ImageInspector.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly ImageProcessingService _imageService = new();
    private readonly AnnotationService _annotationService = new();

    private BitmapSource? _originalImage;

    [ObservableProperty]
    private BitmapSource? _displayImage;

    [ObservableProperty]
    private bool _hasImage;

    [ObservableProperty]
    private string _statusText = "請載入影像";

    [ObservableProperty]
    private double _brightness = 0;

    [ObservableProperty]
    private double _contrast = 1.0;

    [ObservableProperty]
    private double _gamma = 1.0;

    [ObservableProperty]
    private AnnotationShape _selectedShape = AnnotationShape.Rectangle;

    [ObservableProperty]
    private AnnotationViewModel? _selectedAnnotation;

    [ObservableProperty]
    private bool _isDrawingMode;

    public ObservableCollection<AnnotationViewModel> Annotations { get; } = new();

    partial void OnBrightnessChanged(double value) => RefreshImage();
    partial void OnContrastChanged(double value) => RefreshImage();
    partial void OnGammaChanged(double value) => RefreshImage();

    [RelayCommand]
    private void LoadImage()
    {
        var dlg = new OpenFileDialog
        {
            Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp;*.tif;*.tiff|All Files|*.*",
            Title = "開啟影像"
        };
        if (dlg.ShowDialog() != true) return;

        try
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(dlg.FileName);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();

            _originalImage = bitmap;
            HasImage = true;
            StatusText = $"已載入: {System.IO.Path.GetFileName(dlg.FileName)} ({bitmap.PixelWidth}×{bitmap.PixelHeight})";
            RefreshImage();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"載入影像失敗: {ex.Message}", "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void ResetAdjustments()
    {
        Brightness = 0;
        Contrast = 1.0;
        Gamma = 1.0;
    }

    [RelayCommand]
    private void ExportAnnotations()
    {
        if (Annotations.Count == 0)
        {
            MessageBox.Show("沒有標註資料可匯出", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        var dlg = new SaveFileDialog
        {
            Filter = "JSON Files|*.json",
            FileName = "annotations.json",
            Title = "匯出標註"
        };
        if (dlg.ShowDialog() != true) return;
        try
        {
            _annotationService.Export(Annotations.Select(a => a.ToModel()), dlg.FileName);
            StatusText = $"已匯出 {Annotations.Count} 筆標註至 {System.IO.Path.GetFileName(dlg.FileName)}";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"匯出失敗: {ex.Message}", "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void ImportAnnotations()
    {
        var dlg = new OpenFileDialog
        {
            Filter = "JSON Files|*.json",
            Title = "匯入標註"
        };
        if (dlg.ShowDialog() != true) return;
        try
        {
            var models = _annotationService.Import(dlg.FileName);
            Annotations.Clear();
            foreach (var m in models)
                Annotations.Add(AnnotationViewModel.FromModel(m));
            StatusText = $"已匯入 {Annotations.Count} 筆標註";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"匯入失敗: {ex.Message}", "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void DeleteSelectedAnnotation()
    {
        if (SelectedAnnotation != null)
        {
            Annotations.Remove(SelectedAnnotation);
            SelectedAnnotation = null;
            StatusText = $"已刪除標註，剩餘 {Annotations.Count} 筆";
        }
    }

    [RelayCommand]
    private void ClearAnnotations()
    {
        if (Annotations.Count == 0) return;
        var result = MessageBox.Show("確定要清除所有標註嗎？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result == MessageBoxResult.Yes)
        {
            Annotations.Clear();
            SelectedAnnotation = null;
            StatusText = "已清除所有標註";
        }
    }

    public void AddAnnotation(Annotation annotation)
    {
        var vm = AnnotationViewModel.FromModel(annotation);
        Annotations.Add(vm);
        SelectedAnnotation = vm;
        StatusText = $"已新增標註，共 {Annotations.Count} 筆";
    }

    private void RefreshImage()
    {
        if (_originalImage == null) return;
        var adj = new ImageAdjustment
        {
            Brightness = Brightness,
            Contrast = Contrast,
            Gamma = Gamma
        };
        DisplayImage = _imageService.ApplyAdjustments(_originalImage, adj);
    }
}
