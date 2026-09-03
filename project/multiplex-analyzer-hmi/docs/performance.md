# 效能筆記

這個 App 有兩個地方做了明確的效能取捨,而且證據都直接放在 UI 上,不是寫在註解裡。

## 1. Event Log — UI 虛擬化

5,000 筆日誌放在一個 `ListBox`。頁面上有一個開關,把 `ItemsPanel` 在 `VirtualizingStackPanel`(預設)和普通 `StackPanel` 之間切換,然後重新量測。

數字怎麼來的:

- **Realized containers(實際建立的容器數)** — 等 layout 結束後(`DispatcherPriority.ContextIdle`),對 5,000 筆逐一呼叫 `ItemContainerGenerator.ContainerFromIndex(i)`,數非 null 的個數。這是真正存在的 `ListBoxItem` 物件數量。
- **Filter → first frame(篩選到第一幀)** — `Stopwatch` 從篩選條件改變起算,到 `ContextIdle` callback 執行為止,也就是畫面已經畫完的時間點。
- **Private memory** — 同一時間點取 `Process.PrivateMemorySize64`。

開發機實測(Windows 10,1440 × 900 視窗,5,000 筆全部顯示、無篩選):

| | 虛擬化 ON | 虛擬化 OFF | 差距 |
|---|---|---|---|
| Realized containers | **12** | **5,000** | 417× |
| Filter → first frame | **417 ms** | **7,725 ms** | 18.5× |
| Private memory | 575 MB | 582 MB | +7 MB |

三個數字各說明一件事:

- **12 vs 5,000** — 視窗高度只放得下 12 列,虛擬化就只建 12 個 `ListBoxItem`。關掉後 5,000 個全建,而且每個都要 measure / arrange。
- **417 ms vs 7.7 秒** — 這是操作者按下篩選後要等多久才看到畫面。7.7 秒在 HMI 上等於當機。ON 的 417 ms 包含第一次進入頁面時整個 `ListBox` 的建立,實際切換篩選條件時遠低於此。
- **記憶體只差 7 MB** — `ListBoxItem` 本身不重,主要成本在 **layout 時間**而不是記憶體。這也是為什麼「省記憶體」不是虛擬化的主要理由,「不卡」才是。

量測是頁面上的即時數字,不是事後推算;任何人拿到 exe 都能重現。

兩個漏掉就會**靜默失效**的設定,都寫在日誌清單的 Style 裡:

- `ScrollViewer.CanContentScroll="True"` — 沒設會變成像素捲動,虛擬化面板等於普通面板。
- 用 `ListBox` 不用 `ItemsControl` — `ItemsControl` 預設面板不是虛擬化的。

`VirtualizationMode="Recycling"` 讓捲動時重用容器,而不是建新的。

## 2. Plate Map — 一個 visual 取代 96 個

畫 96 孔盤最直覺的做法是 `ItemsControl` + `UniformGrid` 面板 + 每孔一個 `Ellipse`。這樣 visual tree 裡會有 96 個 `ContentPresenter` + 96 個 `Ellipse`(還有標籤),每個都有自己的 layout、hit-test 條目和事件路由。

`PlateMapControl` 改成覆寫 `OnRender`,把盤面、標籤、孔、選取環全部畫進一個 `DrawingContext`:

| | ItemsControl 做法 | `OnRender` 做法(本專案) |
|---|---|---|
| 盤面的 visual tree 節點數 | 約 200 | 1 |
| 每孔的 hit testing | WPF 對每個元素做 routed event | 一次 `WellAt(point)` 計算 |
| 縮放 / 平移 | 面板上套 `LayoutTransform` → 整個重新 layout | render 時 `PushTransform` → 不觸發 layout |
| 主題切換 | 96 個 `DynamicResource` 監聽 | 7 個帶 `AffectsRender` 的畫刷 DP |
| 孔的顏色 | 每孔一個 binding + converter | 9 個 frozen brush 快取,依摘要值索引 |

能 freeze 的全部 freeze(`Pen`、`SolidColorBrush`、`PathGeometry`),WPF 就能在 render thread 直接共用,不用複製。

取捨:`OnRender` 放棄了每孔的 automation peer、tooltip 和焦點。對於孔不需要鍵盤導覽、也沒有 hover 的觸控 HMI 來說,這是對的取捨。如果需求變成每孔要有無障礙支援,選擇會反過來。

## 3. 進度環 — 動畫作用在私有屬性上

`ProgressRing.Progress` 是對外 API。它一變,控制項就對內部的 `AnimatedProgress` DependencyProperty 跑一段 400 ms 的 `DoubleAnimation`,弧形幾何是從動畫值算出來的。使用端每個 tick 設一次數字,就免費得到平滑動畫。幾何每幀重建(並 freeze)—— 只有一個 `ArcSegment`,便宜到不用管。
