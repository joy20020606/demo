# ImageInspector

模擬 E-beam 檢測場景的 WPF 影像檢視器，支援縮放、平移、亮度/對比度/Gamma 調整，以及缺陷區域標註功能。

## 功能列表

- **影像載入**：支援 PNG、JPG、BMP、TIFF 等格式，使用 `WriteableBitmap` 高效渲染
- **縮放 & 平移**：Ctrl + 滾輪縮放（0.05x ~ 32x），Alt + 左鍵拖曳平移
- **影像調整**：即時調整亮度（-128 ~ +128）、對比度（0.1 ~ 4.0）、Gamma（0.1 ~ 5.0）
- **標註繪製**：左鍵拖曳在影像上繪製矩形或橢圓標註
- **標註管理**：選取、設定標籤/顏色、刪除單筆、清除全部
- **JSON 匯出/匯入**：標註資料序列化為 JSON，可重新載入還原

## 技術架構

```
MVVM Pattern
┌─────────────┐     Data Binding     ┌──────────────────┐
│    Views     │ ◄──────────────────► │   ViewModels     │
│ MainWindow   │                      │ MainViewModel    │
│ ImageCanvas  │                      │ AnnotationViewModel│
└─────────────┘                      └────────┬─────────┘
                                              │ uses
                                    ┌─────────▼─────────┐
                                    │     Services       │
                                    │ ImageProcessing    │
                                    │ AnnotationService  │
                                    └─────────┬─────────┘
                                              │ operates on
                                    ┌─────────▼─────────┐
                                    │      Models        │
                                    │   Annotation       │
                                    │  ImageAdjustment   │
                                    └───────────────────┘
```

### Design Patterns

| Pattern | 應用位置 |
|---------|---------|
| MVVM | Views ↔ ViewModels 透過 Data Binding 解耦 |
| Command Pattern | `RelayCommand` / `ICommand` 處理 UI 操作 |
| Observer Pattern | `INotifyPropertyChanged`、`ObservableCollection` 驅動 UI 更新 |

### 專案結構

```
WPF/ImageInspector/
├── ImageInspector.sln
└── ImageInspector/
    ├── ImageInspector.csproj          (.NET 8, net8.0-windows)
    ├── App.xaml / App.xaml.cs
    ├── Models/
    │   ├── Annotation.cs              (標註資料：Shape/位置/大小/標籤)
    │   └── ImageAdjustment.cs         (亮度/對比度/Gamma 參數)
    ├── ViewModels/
    │   ├── MainViewModel.cs           (主 ViewModel：影像載入、標註管理、調整)
    │   └── AnnotationViewModel.cs     (單一標註的 ViewModel wrapper)
    ├── Views/
    │   ├── MainWindow.xaml/.cs        (主視窗：側邊欄 + 標註列表)
    │   └── ImageCanvas.xaml/.cs       (自訂控制項：縮放/平移/繪製)
    ├── Services/
    │   ├── ImageProcessingService.cs  (LUT-based 亮度/對比度/Gamma)
    │   └── AnnotationService.cs       (JSON 匯入匯出)
    └── Converters/
        └── BoolToVisibilityConverter.cs
```

## 如何 Build & Run

### 前置需求

- .NET 8 SDK（[下載](https://dotnet.microsoft.com/download/dotnet/8.0)）
- Windows 10/11（WPF 僅支援 Windows）

### Build

```bash
cd WPF/ImageInspector
dotnet build
```

### Run

```bash
cd WPF/ImageInspector
dotnet run --project ImageInspector
```

或直接開啟 `ImageInspector.sln` 用 Visual Studio 2022 執行。

## 使用方式

1. **載入影像**：點選 `File → 載入影像` 或左側「載入影像」按鈕
2. **縮放**：按住 `Ctrl` 並滾動滾輪
3. **平移**：按住 `Alt` 並左鍵拖曳
4. **繪製標註**：選擇矩形或橢圓，在影像上左鍵拖曳
5. **選取標註**：點擊已繪製的標註框（右側列表也可點選）
6. **設定標籤**：在右側屬性欄輸入標籤文字與顏色 HEX 值
7. **刪除標註**：選取後按 `Delete` 鍵或點「刪除選取標註」
8. **匯出 JSON**：`File → 匯出標註 JSON`
9. **匯入 JSON**：`File → 匯入標註 JSON`
