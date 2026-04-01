# WPF Portfolio - 10 Demo Projects

這是一個 **.NET 8 + WPF + MVVM** 的作品集，展示 C# 桌面應用開發能力，主題圍繞**半導體檢測設備軟體開發**場景，涵蓋影像處理、即時資料監控、設備通訊、批次管理等核心功能。

GitHub Repo：[https://github.com/joy20020606/demo](https://github.com/joy20020606/demo)

---

## 共通技術規範

| 項目 | 規格 |
|------|------|
| Runtime | .NET 8 |
| UI Framework | WPF (Windows Presentation Foundation) |
| MVVM Framework | CommunityToolkit.Mvvm |
| 版本控制流程 | Git Flow |

---

## 專案一覽

| # | 專案名稱 | 中文標題 | 核心 Design Patterns | 技術亮點 |
|---|----------|----------|----------------------|----------|
| 1 | [ImageInspector](https://github.com/joy20020606/demo/tree/main/wpf/ImageInspector) | 影像瑕疵檢測工具 | MVVM、Command | WriteableBitmap、自訂標記 Canvas、影像縮放 |
| 2 | [DefectClassifier](https://github.com/joy20020606/demo/tree/main/wpf/DefectClassifier) | 瑕疵分類管理器 | MVVM、Repository | DataGrid 動態分類、規則引擎、CSV 匯出 |
| 3 | [WaferMapViewer](https://github.com/joy20020606/demo/tree/main/wpf/WaferMapViewer) | 晶圓圖檢視器 | MVVM、Composite | 自訂 WaferMap Control、Die 狀態著色、縮放平移 |
| 4 | [RealtimeDataMonitor](https://github.com/joy20020606/demo/tree/main/wpf/RealtimeDataMonitor) | 即時資料監控儀表板 | MVVM、Observer | LiveCharts2、多通道即時折線圖、閾值警報 |
| 5 | [InstrumentDashboard](https://github.com/joy20020606/demo/tree/main/wpf/InstrumentDashboard) | 設備狀態儀表板 | MVVM、State | 儀表板 Gauge 控件、設備狀態機、事件日誌 |
| 6 | [RecipeEditor](https://github.com/joy20020606/demo/tree/main/wpf/RecipeEditor) | 配方編輯器 | MVVM、Memento | TreeView 配方樹、參數驗證、Undo/Redo |
| 7 | [BatchProcessManager](https://github.com/joy20020606/demo/tree/main/wpf/BatchProcessManager) | 批次處理管理器 | MVVM、Strategy | 批次佇列、進度追蹤、非同步 Task 管理 |
| 8 | [SocketCommSimulator](https://github.com/joy20020606/demo/tree/main/wpf/SocketCommSimulator) | Socket 通訊模擬器 | MVVM、Proxy | TCP Socket、封包解析、通訊日誌視覺化 |
| 9 | [PluginArchitecture](https://github.com/joy20020606/demo/tree/main/wpf/PluginArchitecture) | 插件架構展示 | MVVM、Plugin、MEF | MEF 動態載入、IPlugin 介面、模組熱插拔 |
| 10 | [UnitTestShowcase](https://github.com/joy20020606/demo/tree/main/wpf/UnitTestShowcase) | 單元測試展示 | MVVM、Dependency Injection | xUnit、Moq、ViewModel 測試、覆蓋率報告 |

---

## 各專案說明

### 1. ImageInspector — 影像瑕疵檢測工具
使用 WPF `WriteableBitmap` 進行高效能影像渲染，支援瑕疵區域標記與縮放操作。
MVVM 架構搭配 `RelayCommand`，實現影像載入、標記繪製與結果匯出等功能。
適合展示 WPF 自訂繪圖與影像處理整合能力。

### 2. DefectClassifier — 瑕疵分類管理器
以 `DataGrid` 呈現瑕疵資料，支援動態分類規則設定與批次標記。
實作 Repository Pattern 隔離資料存取層，並提供 CSV 格式匯出。
展示資料驅動 UI 與業務邏輯分層設計。

### 3. WaferMapViewer — 晶圓圖檢視器
自訂 `WaferMapControl` 以 Canvas 繪製晶圓 Die 分布圖，支援狀態著色與滑鼠互動。
實作縮放、平移手勢，Die 點擊可顯示詳細資訊 Popup。
核心展示 WPF 自訂控件開發與大量 UI 元素的效能優化。

### 4. RealtimeDataMonitor — 即時資料監控儀表板
整合 LiveCharts2 顯示多通道即時折線圖，資料以 `ObservableCollection` 動態更新。
支援閾值設定與超限警報標記，模擬設備感測器資料串流。
展示 WPF 非同步資料繫結與圖表效能管理。

### 5. InstrumentDashboard — 設備狀態儀表板
以儀表板 Gauge 控件顯示設備各項參數，並實作設備狀態機（Idle/Running/Error/Paused）。
事件日誌以時間序列呈現，支援篩選與匯出。
展示 State Pattern 與複雜 UI 佈局設計。

### 6. RecipeEditor — 配方編輯器
以 `TreeView` 呈現階層式配方結構，支援新增、刪除、複製節點。
實作 Memento Pattern 提供完整的 Undo/Redo 功能，並具備參數範圍驗證。
展示 WPF 樹狀資料結構操作與複雜 ViewModel 狀態管理。

### 7. BatchProcessManager — 批次處理管理器
以佇列管理多個批次任務，支援暫停、取消與優先順序調整。
使用 `async/await` 與 `CancellationToken` 實現非同步任務控制，進度以 ProgressBar 即時顯示。
展示 WPF 非同步程式設計與批次作業 UI 設計。

### 8. SocketCommSimulator — Socket 通訊模擬器
實作 TCP Server/Client 雙端模擬，支援自訂封包格式定義與解析。
通訊封包以色彩區分方向並顯示於日誌面板，支援 HEX/ASCII 切換。
展示 .NET Socket 程式設計與 WPF 即時日誌 UI。

### 9. PluginArchitecture — 插件架構展示
以 MEF（Managed Extensibility Framework）實現插件動態載入，定義 `IPlugin` 標準介面。
支援執行期插件掃描與熱插拔，主程式無需重新編譯即可擴充功能。
展示企業級 WPF 應用的模組化架構設計。

### 10. UnitTestShowcase — 單元測試展示
以 xUnit 為測試框架，Moq 進行依賴隔離，覆蓋 ViewModel、Service 與 Repository 層。
整合 Dependency Injection（`Microsoft.Extensions.DependencyInjection`）提升可測試性。
展示 WPF MVVM 應用的完整測試策略與程式碼覆蓋率報告。

---

## Clone & Build

```bash
# Clone 專案
git clone https://github.com/joy20020606/demo.git
cd demo/wpf

# 進入任一專案並建置（以 ImageInspector 為例）
cd ImageInspector
dotnet build

# 執行
dotnet run
```

> **需求**：Visual Studio 2022 或 .NET 8 SDK，Windows 10/11 環境。
