# InstrumentDashboard

半導體檢測設備控制面板模擬器（WPF / .NET 8）。

## 功能

- **儀器參數設定面板**：電壓、電流、掃描速度、取樣率、溫度、壓力、真空度等參數設定
- **即時狀態儀表板**：自訂 Gauge 控件顯示溫度、壓力、真空、電壓、電流及進度
- **設備狀態機**：Idle → Initializing → Running → Complete（State Pattern 實作）
- **操作日誌**：帶顏色等級標示，支援匯出為 `.txt`
- **Recipe 管理**：儲存/載入 JSON 格式製程預設檔

## 架構

```
MVVM + State Pattern + Mediator (DispatcherTimer)

InstrumentDashboard/
├── Models/
│   ├── InstrumentState.cs      # 狀態機狀態定義（State Pattern）
│   ├── InstrumentParameter.cs  # 儀器參數資料模型
│   └── Recipe.cs               # 製程預設檔模型
├── ViewModels/
│   ├── MainViewModel.cs        # 主控 VM，協調狀態機與模擬
│   ├── ParameterPanelViewModel.cs
│   ├── StatusDashboardViewModel.cs
│   └── LogViewModel.cs
├── Views/
│   ├── MainWindow.xaml
│   ├── ParameterPanel.xaml
│   ├── StatusDashboard.xaml
│   └── LogPanel.xaml
├── Services/
│   ├── StateMachineService.cs  # State Pattern 實作
│   ├── SimulationService.cs    # 感測器數據模擬
│   └── RecipeService.cs        # JSON Recipe 存取
├── Controls/
│   └── GaugeControl.xaml/.cs  # 自訂 Arc Gauge 控件
└── Converters/
    └── LogLevelToColorConverter.cs
```

## 狀態機轉換

```
Idle ──Start──► Initializing ──(2s auto)──► Running ──(progress=100%)──► Complete
  ▲                  │                         │                              │
  └──────────────────┴─────────Stop────────────┘              Reset──────────┘
                     │
                   Error
```

## 技術棧

- .NET 8 (`net8.0-windows`)
- WPF
- CommunityToolkit.Mvvm 8.x
- System.Text.Json

## Build & Run

```bash
cd WPF/InstrumentDashboard
dotnet build
dotnet run --project InstrumentDashboard/InstrumentDashboard.csproj
```

或在 Visual Studio 2022 開啟 `InstrumentDashboard.sln`，直接 F5 執行。

## Recipe 檔案位置

Recipe JSON 檔預設儲存於：

```
%APPDATA%\InstrumentDashboard\Recipes\
```
