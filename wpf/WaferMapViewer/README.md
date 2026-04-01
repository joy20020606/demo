# WaferMapViewer

晶圓圖視覺化工具 — 以視覺化方式呈現晶圓上各 Die 的檢測結果，支援色彩映射、篩選與統計。

## 功能

- **DrawingVisual 高效繪製**：圓形晶圓 Die Map，不使用大量 UI 元素
- **色彩映射**：Standard / Monochrome / Heat Map 三種方案（Factory Pattern）
- **點擊 Die**：顯示詳細檢測數據（位置、結果、量測值）
- **篩選高亮**：依缺陷類型高亮特定 Die
- **統計面板**：良率、Pass/Fail 數量、各類缺陷分佈

## 架構

```
MVVM + Factory Pattern + Template Method Pattern

MainViewModel
├── WaferMapViewModel   → 管理晶圓資料、選取、色彩映射
├── DieDetailViewModel  → 選取 Die 的詳細資訊
└── StatisticsViewModel → 良率統計與缺陷分析

Services
├── WaferDataGenerator  → 產生模擬晶圓資料（熱區分佈）
└── ColorMapFactory     → Factory Pattern，產生色彩映射方案

Controls
└── WaferMapControl     → 自訂控件，DrawingVisual 繪製晶圓圖
```

## 技術棧

- .NET 8 / WPF
- CommunityToolkit.Mvvm 8.3.2
- WPF DrawingVisual / DrawingContext
- System.Text.Json

## Build & Run

```bash
cd WPF/WaferMapViewer
dotnet build
dotnet run --project WaferMapViewer
```

## Die 結果類型

| 類型 | 顏色 | 說明 |
|------|------|------|
| Pass | 綠色 | 通過檢測 |
| Fail | 紅色 | 一般失效 |
| Scratch | 橘色 | 刮傷 |
| Particle | 黃色 | 顆粒污染 |
| Void | 紫色 | 空洞缺陷 |
| Crack | 橘紅 | 裂紋 |
| Edge Die | 灰色 | 邊緣 Die（不計入良率）|
