# PluginArchitecture — 外掛式影像分析系統

展示可擴展的外掛架構設計，支援動態載入分析模組（如不同影像濾波演算法）。

## 設計模式

| 模式 | 應用位置 |
|------|---------|
| **MVVM** | App 層 ViewModel / View 分離 |
| **Plugin Pattern** | `IAnalysisPlugin` 介面 + `PluginLoader` 動態載入 |
| **Abstract Factory** | `IPluginFactory` 介面（可擴充） |
| **Dependency Injection** | `App.xaml.cs` 以 `Microsoft.Extensions.DependencyInjection` 組裝服務 |

## 專案結構

```
PluginArchitecture/
├── PluginArchitecture.sln
├── README.md
├── PluginArchitecture.Core/              ← 共用介面（class library, net8.0）
│   ├── IAnalysisPlugin.cs                ← 外掛主介面
│   ├── IPluginFactory.cs                 ← 抽象工廠介面
│   └── PluginMetadata.cs
├── PluginArchitecture.Plugins/           ← 內建外掛（class library, net8.0）
│   ├── EdgeDetectionPlugin.cs            ← Sobel 邊緣偵測
│   ├── BinarizationPlugin.cs             ← 閾值二值化
│   └── NoiseReductionPlugin.cs           ← 均值模糊雜訊濾除
└── PluginArchitecture.App/               ← WPF 主程式（net8.0-windows）
    ├── App.xaml.cs                       ← DI 容器設定
    ├── Services/
    │   ├── PluginLoader.cs               ← 動態 DLL 載入（System.Reflection）
    │   └── ImageProcessingService.cs     ← 影像處理服務
    ├── ViewModels/
    │   ├── MainViewModel.cs
    │   └── PluginDetailViewModel.cs
    └── Views/
        └── MainWindow.xaml
```

## 核心介面

```csharp
public interface IAnalysisPlugin
{
    string Name { get; }
    string Description { get; }
    string Version { get; }
    bool IsEnabled { get; set; }
    IReadOnlyList<PluginParameter> Parameters { get; }

    // 處理 BGRA32 像素陣列，回傳結果像素
    byte[] Process(byte[] inputPixels, int width, int height);
    void SetParameter(string key, object value);
}
```

## 內建外掛

| 外掛 | 演算法 | 可調參數 |
|------|--------|---------|
| 邊緣偵測 | Sobel 運算子 | 邊緣閾值（0–255） |
| 二值化 | 全域閾值 | 二值化閾值（0–255） |
| 雜訊濾除 | 均值模糊 | 卷積核大小（3/5/7） |

## Build & Run

```bash
cd WPF/PluginArchitecture

# 建置
dotnet build

# 執行
dotnet run --project PluginArchitecture.App/PluginArchitecture.App.csproj
```

## 開發新外掛指南

### 1. 建立 Class Library 專案

```bash
dotnet new classlib -n MyPlugin --framework net8.0
dotnet add MyPlugin/MyPlugin.csproj reference ../PluginArchitecture.Core/PluginArchitecture.Core.csproj
```

### 2. 實作 `IAnalysisPlugin`

```csharp
using PluginArchitecture.Core;

public class MyPlugin : IAnalysisPlugin
{
    public string Name => "我的外掛";
    public string Description => "外掛描述";
    public string Version => "1.0.0";
    public bool IsEnabled { get; set; } = true;

    private readonly List<PluginParameter> _parameters = new()
    {
        new PluginParameter
        {
            Key = "intensity",
            DisplayName = "強度",
            Value = 100,
            DefaultValue = 100,
            Type = PluginParameterType.Integer,
            Min = 0,
            Max = 255
        }
    };

    public IReadOnlyList<PluginParameter> Parameters => _parameters;

    public byte[] Process(byte[] inputPixels, int width, int height)
    {
        // 輸入/輸出為 BGRA32 格式，每像素 4 bytes
        byte[] output = new byte[inputPixels.Length];
        // ... 實作影像處理 ...
        return output;
    }

    public void SetParameter(string key, object value)
    {
        var param = _parameters.Find(p => p.Key == key);
        if (param != null) param.Value = value;
    }
}
```

### 3. 動態載入 DLL

將編譯後的 DLL 放置於指定目錄，呼叫 `PluginLoader.LoadFromDirectory(path)` 即可自動掃描並載入所有實作 `IAnalysisPlugin` 的類型。

```csharp
var loader = serviceProvider.GetRequiredService<PluginLoader>();
loader.LoadFromDirectory("./ExternalPlugins");
```

### 4. 透過 DI 容器注入（內建外掛）

在 `App.xaml.cs` 的 `ConfigureServices` 中：

```csharp
services.AddSingleton<IAnalysisPlugin, MyPlugin>();
```

## 技術棧

- **.NET 8** / **WPF**
- **CommunityToolkit.Mvvm 8.x** — Source Generator 自動產生命令與屬性通知
- **Microsoft.Extensions.DependencyInjection** — IoC 容器
- **System.Reflection** — Assembly.LoadFrom 動態載入外部 DLL
