# BatchProcessManager - 批次處理管理器

第九個 WPF 示範專案，模擬批次檢測任務的排程、執行與進度追蹤，支援並行處理與錯誤重試。

## 功能特色

- **任務佇列管理**：新增、移除、排序任務，支援優先順序調整（上移/下移）
- **並行執行引擎**：基於 `SemaphoreSlim` 控制最大並行數（1-10）
- **即時進度追蹤**：整體進度條、已耗時、預估完成時間
- **錯誤重試機制**：可設定重試次數與延遲秒數，自動重試失敗任務
- **執行記錄**：即時顯示每個任務的啟動/完成/重試記錄
- **報表匯出**：執行結果統計 + JSON 匯出至桌面

## 架構

```
MVVM + Command Pattern + Chain of Responsibility
```

### 設計模式

| 模式 | 應用位置 |
|------|----------|
| MVVM | ViewModels ↔ Views 雙向綁定 |
| Command Pattern | RelayCommand (ICommand) |
| Chain of Responsibility | RetryService 重試鏈 |
| Observer | 事件通知 (TaskStarted, TaskCompleted, AllCompleted) |

## 專案結構

```
BatchProcessManager/
├── BatchProcessManager.sln
└── BatchProcessManager/
    ├── Models/
    │   ├── BatchTask.cs       # 任務模型（ID、名稱、狀態、優先順序、進度）
    │   ├── TaskResult.cs      # 結果模型（成功/失敗、耗時、錯誤訊息）
    │   └── BatchConfig.cs     # 批次設定（並行數、重試、逾時）
    ├── ViewModels/
    │   ├── MainViewModel.cs   # 主協調器
    │   ├── TaskQueueViewModel.cs
    │   ├── ExecutionViewModel.cs
    │   └── ReportViewModel.cs
    ├── Views/
    │   ├── MainWindow.xaml
    │   ├── TaskQueuePanel.xaml
    │   ├── ExecutionPanel.xaml
    │   └── ReportPanel.xaml
    ├── Services/
    │   ├── BatchExecutionService.cs  # 並行引擎 (SemaphoreSlim + Task.WhenAll)
    │   ├── RetryService.cs           # 重試策略
    │   └── ReportService.cs          # JSON 報表匯出
    └── Converters/
        └── Converters.cs
```

## 技術棧

- **.NET 8** (`net8.0-windows`)
- **WPF** - UI 框架
- **CommunityToolkit.Mvvm 8.3.2** - ObservableObject, RelayCommand, [ObservableProperty]
- **Task Parallel Library** - `SemaphoreSlim`, `Task.WhenAll`, `Interlocked`
- **System.Text.Json** - 報表 JSON 序列化

## Build & Run

```bash
cd WPF/BatchProcessManager
dotnet build
dotnet run --project BatchProcessManager/BatchProcessManager.csproj
```

## 使用流程

1. **任務佇列** 頁籤：按「加入範例」快速填入測試任務，或手動輸入
2. 調整優先順序（↑↓）、或按「依優先排序」自動排列
3. 按右上角 **▶ 開始執行批次** 啟動
4. **執行監控** 頁籤：即時查看進度條、日誌、並行設定
5. 完成後自動跳至 **執行報表** 頁籤，可按「匯出 JSON」將報表存至桌面
