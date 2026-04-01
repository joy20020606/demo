# RealtimeDataMonitor

即時數據監控系統 — 模擬多通道感測器資料擷取與視覺化，支援波形顯示、警報機制與 CSV 匯出。

## 功能

- **多通道即時波形圖**：4 個通道同時顯示，每秒可處理數百筆資料點（OxyPlot）
- **背景執行緒感測器模擬**：Producer-Consumer Pattern，ConcurrentQueue + async/await
- **閾值警報系統**：Warning / Critical 兩級警報，超標即時顯示
- **資料暫停 / 繼續**：Start / Stop 控制擷取
- **時間軸縮放**：5 ~ 120 秒滑桿調整顯示視窗
- **歷史數據匯出 CSV**：感測器資料與警報紀錄分開匯出

## 架構

```
RealtimeDataMonitor/
├── Models/
│   ├── SensorData.cs          # 時間戳 + 數值 + 通道 ID
│   ├── ChannelConfig.cs       # 通道名稱、顏色、閾值上下限
│   └── AlarmEvent.cs          # 警報事件模型
├── Services/
│   ├── DataAcquisitionService # 背景執行緒產生模擬資料（Producer-Consumer）
│   ├── AlarmService.cs        # 閾值監控與警報觸發
│   └── CsvExportService.cs    # CSV 匯出
├── ViewModels/
│   ├── MainViewModel.cs       # 主要協調 VM
│   ├── ChartViewModel.cs      # OxyPlot PlotModel 管理
│   ├── AlarmViewModel.cs      # 警報列表
│   └── ChannelSettingsViewModel.cs
└── Views/
    ├── MainWindow.xaml        # 主視窗
    ├── ChartPanel.xaml        # 波形圖 UserControl
    ├── AlarmPanel.xaml        # 警報列表 UserControl
    └── SettingsPanel.xaml     # 設定 UserControl
```

### Design Patterns

| Pattern | 用途 |
|---|---|
| MVVM | UI 與邏輯分離（CommunityToolkit.Mvvm） |
| Observer | `event Action<>` 串接 Service → ViewModel |
| Producer-Consumer | `ConcurrentQueue` 分離資料產生與消費執行緒 |

## 技術棧

- .NET 8 (net8.0-windows)
- WPF + C#
- CommunityToolkit.Mvvm 8.x
- OxyPlot.Wpf 2.x
- ConcurrentQueue、async/await、Task

## Build & Run

```bash
cd WPF/RealtimeDataMonitor
dotnet build
dotnet run --project RealtimeDataMonitor/RealtimeDataMonitor.csproj
```

## 操作說明

1. 點 **Start** 開始擷取模擬資料
2. 右側 Settings 調整時間視窗（秒）與取樣率
3. 超出閾值時右側 Alarm Panel 自動顯示警報
4. 點 **Stop** 暫停，可再次 **Start** 繼續
5. **Export Data** / **Export Alarms** 匯出 CSV 至指定路徑
