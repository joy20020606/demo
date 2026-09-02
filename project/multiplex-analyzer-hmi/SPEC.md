# multiplex-analyzer-hmi — 規格書 v0.3

> **用途**:針對「WPF / XAML UI 軟體工程師(專案外包)— 博錸生技」職缺的對位 demo。
> **定位**:純 Desktop UI 作品,**刻意不做 Backend / Web API / DB**(JD 明講方向不同)。
> **技術**:.NET 9 + WPF + MVVM(CommunityToolkit.Mvvm)。
> **開發環境**:Windows 10 開發即可(WPF 在 Win10/Win11 行為一致)。

---

## 0. 與該公司的「正確距離」

查過博錸官網後,只取**一個**對 UI 有意義的事實,其餘全部泛用化:

| 取用 | 不取用 | 理由 |
|---|---|---|
| ✅ **一孔 = 多個檢測指標**(資料是一對多) | ❌ πCode / IntelliPlex 等產品名 | 資料形狀決定 UI 架構,是真的技術決策 |
| ✅ 96 孔盤(業界通用規格) | ❌ EGFR / KRAS 等真實基因名 | 猜錯領域細節,面試被問就露餡 |
| ✅ 執行需時約 1 小時、無人看管 | ❌ 真實 protocol 溫度/時間 | 只影響 UI 要有進度顯示,不需要正確化學 |

**面試講法(誠實且專業)**:
> 「我沒有貴司的實際規格,所以做的是泛用的多重檢測分析儀 HMI。我唯一從官網取用的是『一孔對應多個指標』這個資料形狀 —— 因為它直接決定了 Plate Map 要做成 master-detail 而不是單純熱區圖。其他領域細節我刻意留白,免得猜錯。」

> 官網參考:[產品頁](https://www.plexbio.com/tw/products/)

---

## 1. 產品情境

**MultiPlex Analyzer HMI** — 桌上型多重檢測分析儀的觸控操作台。
1920×1080 直立式觸控面板,操作者戴手套、站立操作、沒有滑鼠。

- 一次跑一塊 **96 孔盤**,一孔 = 一個檢體
- 每孔同時測 **8 個指標**(代號 M-01 ~ M-08,不用真實基因名)
- 一輪約 **60 分鐘**,4 個步驟:`Load → Incubate → Wash → Read`
- 狀態機:`Idle → Running ⇄ Paused → Completed / Error → Idle`

設計限制(README 要寫,面試最加分的一段):
- 戴手套 → 觸控目標 ≥ 48×48 dp,間距 ≥ 8dp
- 站立看螢幕 → 最小字級 14pt,對比 ≥ 4.5:1
- **沒有滑鼠、沒有 hover** → 關鍵資訊絕不放 tooltip
- 實驗室有明有暗 → Dark / Light 是功能需求,不是裝飾

---

## 2. JD 逐條對位

| JD 要求 | Demo 對應 | Phase |
|---|---|---|
| Figma → WPF/XAML 實作 | 自畫 Figma Design System + 4 畫面,README 並排截圖 | 各 Phase 前置 |
| Style / ResourceDictionary / ControlTemplate | `Themes/` 拆 tokens / typography / controls | P1, P3 |
| Dark / Light + DynamicResource | 執行期即時切換,不重啟視窗 | P1 |
| MVVM / Binding / Trigger | ViewModel 全 `ObservableObject`,狀態走 DataTrigger + VSM | P2, P4 |
| 客製化控制項 + Touch | ToggleSwitch / SegmentedControl / NumericStepper / PlateMap | P3, P5 |
| Animation / Storyboard / Geometry / Path | 進度環、狀態轉場、手繪盤面 | P4, P5 |
| UI 測試 / Debug / 效能優化 | FPS / 記憶體前後對照數據 | P6 |
| Git | conventional commits,一 Phase 一組 commit | 全程 |

---

## 3. 畫面規格(4 頁 + 殼層)

### Shell
左側 72px 圖示側邊欄(4 頁籤)+ 頂部狀態列(狀態燈、剩餘時間、主題切換)。
狀態燈顏色由 `RunState` enum → Converter → **DynamicResource brush key**(不硬寫色碼)。
換頁 200ms 淡入 + 8px 上滑 Storyboard。

### P-A. Run Dashboard
- 中央 **圓形進度環** — 手繪 `ArcSegment` + `DoubleAnimation` 平滑補間
- **4 步驟時間軸**(Load / Incubate / Wash / Read)— 目前步驟高亮 + 脈動動畫
  > 就是網購「已下單 → 已出貨 → 配送中」那條進度線
- 大顆 Start / Pause / Abort,依狀態 enable/disable,VisualStateManager 轉場
- Abort 走自訂 Modal Dialog(遮罩 + 縮放進場)

### P-B. Plate Map(核心頁)
- 96 孔盤(8×12)自繪,孔色 = 該孔 8 個指標的**摘要值**色階
- 點一孔 → 右側 SlideIn 面板,列出該孔 **8 條指標橫條圖**
  > 這就是「一孔多值」→ master-detail,對應 `Order → OrderItems`
- 觸控:單指點選、拖曳框選整排、雙指 pinch zoom + pan
- **滑鼠 fallback**:滾輪 = zoom、拖曳 = pan(見 §5)
- 效能:96 孔**不是 96 個 UserControl**,用單一 Canvas 手繪
  → 附「ItemsControl 版 vs 手繪版」FPS 對照

**UX 決策要在 README 寫出來**:96 格畫得下,但每格有 8 個值 → 必須壓成一個顏色。
本 demo 取「異常指標數量」當摘要,並在 README 註明「實務上應與臨床端共同定義」。

### P-C. Settings
- Dark / Light 切換(theme 展示台)
- **NumericStepper**:長按 +/- 連發加速(400ms 後啟動,每 60ms 一次)
- 自訂 **ToggleSwitch**(Thumb 位移用 Storyboard,不是換圖)
- 自訂 **SegmentedControl**(三選一)

### P-D. Event Log
- 5,000 筆假日誌,`VirtualizingStackPanel` + `ScrollViewer.CanContentScroll=True`
- 依 Level(Info/Warn/Error)篩選 + 關鍵字搜尋
- 附「虛擬化開/關」滾動 FPS 與記憶體數字

---

## 4. 主題與 Design Token 架構

```
Themes/
  Tokens.Light.xaml      # 只有 Color / Brush,key 與 Figma variables 同名
  Tokens.Dark.xaml
  Typography.xaml        # Display / Title / Body / Caption 四級
  Sizing.xaml            # Space.4 / Radius.M / TouchTarget.Min = 48
  Controls/
    Button.xaml
    ToggleSwitch.xaml
    SegmentedControl.xaml
    NumericStepper.xaml
    Dialog.xaml
```

三條規則(README 要寫,這是「有系統」的證據):
1. **控制項樣式只准引用 token key,不准出現字面色碼**
2. 隨主題變的用 `DynamicResource`;不變的(間距、圓角、字級)用 `StaticResource`
   — 理由:DynamicResource 要註冊監聽 + 執行期查表,不該濫用
3. 主題切換 = 抽換 `Application.Current.Resources.MergedDictionaries[0]`,一行

`docs/design-token-map.md` 放對照表:
`Figma variable color/surface/raised` → `{DynamicResource Brush.Surface.Raised}`

---

## 5. 觸控:雙軌設計

沒有實體觸控螢幕,而 WPF 的 `IsManipulationEnabled` **預設不吃滑鼠**。
解法 = 輸入層雙軌,共用同一份 ViewModel 邏輯:

| 意圖 | Touch | Mouse |
|---|---|---|
| Zoom | `ManipulationDelta.Scale` | `MouseWheel` |
| Pan | `ManipulationDelta.Translation` | 左鍵拖曳 |
| 選取 | `TouchDown` | `MouseLeftButtonDown` |

兩條路徑都只呼叫 `PlateViewModel.Zoom(factor, center)` / `.Pan(dx, dy)`。
**這本身是加分點**:面試官用滑鼠也能玩,且證明你懂輸入抽象。

觸控裝置 = Windows 觸控筆電 / 外接觸控螢幕(**不是 Android 平板**;機台跑 Windows,走 Windows Touch API)。
借不到就在 README 誠實寫「觸控路徑已實作,未於實機驗證」。

---

## 6. 刻意不做(面試講這段)

| 不做 | 理由 |
|---|---|
| 後端 API / DB | JD 明講方向不同;資料走 `IDeviceService` 假實作,介面留著,接真機換一個實作 |
| 第三方 UI 套件(MaterialDesign / HandyControl) | 要證明的正是「我寫得出 ControlTemplate」,套套件等於把答案遮住 |
| 第三方圖表套件 | 同上,且 HMI 對 render 成本敏感 |
| **真實生醫名詞 / protocol 細節** | 我沒有他們的規格,猜錯會露餡。指標用 M-01~M-08 泛用代號 |
| **多語系切換** | ResourceDictionary 換字典的能力,主題切換已經證明過,重複 |
| **表單驗證(INotifyDataErrorInfo)** | 偏 MVVM 資料層,不是這個 JD 的重點 |
| 高測試覆蓋率 | UI demo 價值在看得到;只對狀態機寫 ~8 個測試證明 MVVM 真解耦 |

---

## 7. Phase 規劃(每 Phase 結束都能 `dotnet run` 看到東西)

> 節奏:我先講 WHY → 給 code → **停下來讓你自己跑一次** → 對答案 → 再進下一個。
> 每 Phase 一組 commit。

### Phase 0 — 跑得起來(~30 min)
`dotnet new wpf`、裝 `CommunityToolkit.Mvvm`、一個空白視窗 + 一顆按鈕。
- **驗收**:`dotnet run` 開得起來,按鈕會動
- **學到**:WPF 專案結構、App.xaml、XAML 與 code-behind 的關係

### Phase 1 — 主題系統(最重要的地基,~2h)
Tokens.Light / Tokens.Dark(各 8 個色)、Typography、Sizing、一個 Button Style、切換按鈕。
- **驗收**:按一下整個視窗換色,**不重開視窗**
- **學到**:ResourceDictionary、MergedDictionaries、StaticResource vs DynamicResource
  (親手做一次「故意用錯 Static 就不會變」的實驗)
- Figma 前置:建色票 + 字級 + Variables

### Phase 2 — Shell 與導覽(~2h)
側邊欄 4 圖示 + 頂部狀態列 + 4 個空白 Page + 換頁動畫 + `ShellViewModel`。
- **驗收**:點側邊欄能換頁,有轉場動畫
- **學到**:MVVM 骨架、DataTemplate 選頁、第一支 Storyboard

### Phase 3 — 自訂控制項(ControlTemplate 練功房,~4h)
Settings 頁:ToggleSwitch / SegmentedControl / NumericStepper(長按連發)。
- **驗收**:三個控制項在 Dark/Light 下都正確,長按 + 會加速
- **學到**:ControlTemplate、TemplateBinding、Trigger、VisualStateManager
- Figma 前置:三個控制項做成 Component,含 default / pressed / disabled 三個 variant

### Phase 4 — Dashboard 狀態機(~4h)
`RunState` enum + `IDeviceService` 假實作(Timer 推進度)、手繪進度環、4 步驟時間軸、Start/Pause/Abort + Modal Dialog。
- **驗收**:按 Start 環會轉、狀態燈變色、步驟依序高亮
- **學到**:Geometry / Path / ArcSegment、Storyboard、Converter、狀態驅動 UI

### Phase 5 — Plate Map(壓軸,~5h)
96 孔手繪 + 色階、點選 → 右側 8 條指標明細面板、觸控 + 滑鼠雙軌手勢。
- **驗收**:滾輪縮放、拖曳平移、點孔出明細
- **學到**:DrawingVisual、hit testing、ManipulationDelta、輸入抽象
- 順手做:ItemsControl 對照組,量 FPS

### Phase 6 — Event Log + 效能量測(~3h)
5,000 筆虛擬化清單 + 篩選 + 搜尋、用診斷工具量 FPS 與記憶體。
- **驗收**:`docs/performance.md` 有兩組前後對照數字
- **學到**:UI 虛擬化、如何實際量 WPF 效能(不是憑感覺)

### Phase 7 — 包裝交付(~3h)
Figma 稿補齊 4 張、README 並排截圖、3 段 GIF、token map、單檔 exe Release。

**總計約 23 小時 / 每天 3~4 小時 → 7 天**

---

## 8. 加分題(有時間再做,沒有也不影響)

- **即時曲線** — 自繪 `StreamGeometry` 溫度曲線,1000 點滾動視窗,`Freeze()` 所有 Brush/Pen
  → 秀「不用第三方圖表套件也能高效繪製」
- **多語系** zh-TW / en
- **孔內迷你熱區圖** — 每孔畫成 2×4 小格直接顯示 8 個指標,取代單色摘要
  → 這是「壓縮問題」的另一種解法,README 可以並列比較兩種方案

---

## 9. 交付物

1. GitHub repo(README 中英各一段開頭)
2. Figma 公開連結(Component / Auto Layout / Variables 三件套)
3. README:Figma 稿 vs 實作截圖並排 ×4
4. GIF ×3(主題切換 / 狀態機轉場 / PlateMap 縮放)
5. `docs/design-token-map.md`
6. `docs/performance.md`
7. GitHub Release:自包含 `win-x64` 單檔 exe
