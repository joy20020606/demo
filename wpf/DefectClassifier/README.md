# DefectClassifier - 晶圓缺陷分類管理系統

模擬晶圓檢測後的缺陷分類與管理流程，支援影像瀏覽、分類標記與統計報表。

## 功能特色

- **缺陷瀏覽**：列表顯示所有缺陷記錄，支援晶圓ID篩選
- **拖放分類標記**：拖放類型標籤至缺陷列表，或點擊標籤直接套用
- **自動分類**：使用 Strategy Pattern 切換不同分類規則（尺寸分類 / 位置分類）
- **統計儀表板**：缺陷類型分布圖、每日趨勢圖、良率計算、各晶圓缺陷統計
- **SQLite 本地儲存**：自動建立資料庫，首次啟動自動植入 50 筆示範資料

## 架構

```
DefectClassifier/
├── Models/
│   ├── DefectType.cs            # 缺陷類型枚舉
│   ├── DefectRecord.cs          # 缺陷資料模型
│   └── ClassificationResult.cs  # 分類結果
├── Services/
│   ├── DefectRepository.cs      # SQLite CRUD (Repository Pattern)
│   ├── StatisticsService.cs     # 統計計算
│   └── ClassificationStrategy/
│       ├── IClassificationStrategy.cs  # 策略介面 (Strategy Pattern)
│       ├── SizeBasedStrategy.cs        # 依尺寸分類
│       └── LocationBasedStrategy.cs    # 依位置分類
├── ViewModels/
│   ├── MainViewModel.cs          # 主視窗 ViewModel
│   ├── BrowserViewModel.cs       # 缺陷瀏覽
│   ├── StatisticsViewModel.cs    # 統計儀表板
│   ├── ClassificationViewModel.cs # 自動分類
│   └── ChartItem.cs              # 圖表資料項目
├── Views/
│   ├── MainWindow.xaml/.cs       # 主視窗（TabControl 導航）
│   ├── BrowserPanel.xaml/.cs     # 缺陷瀏覽面板
│   ├── StatisticsPanel.xaml/.cs  # 統計儀表板面板
│   └── ClassificationPanel.xaml/.cs # 自動分類面板
└── Converters/
    ├── NullToVisibilityConverter.cs
    ├── DefectTypeToColorConverter.cs
    └── DefectTypeToStringConverter.cs
```

## 設計模式

| 模式 | 應用位置 |
|------|---------|
| MVVM | 全專案，使用 CommunityToolkit.Mvvm |
| Strategy Pattern | `IClassificationStrategy` → `SizeBasedStrategy` / `LocationBasedStrategy` |
| Repository Pattern | `DefectRepository`（封裝 SQLite CRUD） |

## 技術棧

- **Framework**：.NET 8 / WPF
- **MVVM**：CommunityToolkit.Mvvm 8.3.2
- **資料庫**：Microsoft.Data.Sqlite 8.0.0
- **圖表**：WPF 自訂 ItemsControl + 矩形繪製（無外部圖表庫）

## 缺陷類型

| 類型 | 說明 | 顏色 |
|------|------|------|
| Scratch (刮傷) | 線性刮痕 | 紅 |
| Particle (粒子) | 顆粒污染 | 橙 |
| PatternDefect (圖案缺陷) | 圖案異常 | 紫 |
| Pit (坑洞) | 表面凹坑 | 藍 |
| Bridge (橋接) | 導線短路橋接 | 綠 |

## Build & Run

**環境需求**：
- .NET 8 SDK
- Windows 10/11（WPF 限定）

**建置：**
```bash
cd WPF/DefectClassifier
dotnet build
```

**執行：**
```bash
dotnet run --project DefectClassifier/DefectClassifier.csproj
```

**資料庫位置：**
```
%LocalAppData%\DefectClassifier\defects.db
```

## 操作說明

### 缺陷瀏覽
1. 在左側列表查看所有缺陷記錄
2. 點擊列表項目查看右側詳細資訊
3. 輸入晶圓ID後點擊「篩選」可過濾記錄
4. 拖曳右側類型標籤至列表，或直接點擊標籤對選取缺陷重新分類
5. 點擊「刪除此缺陷」移除記錄

### 統計儀表板
- 自動計算缺陷類型分布、每日趨勢、各晶圓缺陷數
- 良率計算：嚴重度 ≥ 3 的缺陷視為致命缺陷，影響良率
- 點擊「重新整理」更新統計資料

### 自動分類
1. 從下拉選單選擇分類策略
2. 點擊「執行分類」產生建議結果
3. 確認表格中的建議後，點擊「套用結果至資料庫」批量更新
