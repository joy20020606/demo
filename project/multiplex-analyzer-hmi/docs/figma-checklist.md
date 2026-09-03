# Figma 清單

## 0. 用外掛一鍵生成(建議)

`figma-plugin/` 裡是一個 Figma 開發用外掛,會在目前檔案裡自動建出下面第 1–4 節的所有東西(variables、文字樣式、8 個 component、4 張畫面 × Light/Dark),名字與程式碼完全一致。

1. Figma 開一個**空白**設計檔(桌面版或網頁版都可以)
2. 右鍵畫布 → **Plugins → Development → Import plugin from manifest…**
3. 選 `figma-plugin/manifest.json`
4. 右鍵畫布 → **Plugins → Development → MultiPlex HMI Design System**
5. 等幾秒,會出現兩個 page:**Design System**(component)和 **Screens**(4 張畫面,左 Light 右 Dark)

字型:如果你的 Figma 找得到 Segoe UI 就用它,否則自動退回 Inter(結構不變)。

免費版限制:一個 collection 只能有 1 個 mode,所以外掛建的是 **`Color / Light`** 和 **`Color / Dark`** 兩個 collection(變數名相同),Dark 畫面的每個綁定都指到 Dark 那組。付費版可以把它們合併成一個 collection 的兩個 mode,效果相同。

生成後你要做的只有:看一下排版有沒有要微調的、Share → Anyone with the link → can view,把連結貼到 README。

免費版可用;開發模式匯入的外掛不需要發佈。

---

Figma 檔要包含什麼,才能跟程式碼一對一對上。**名字比像素精準更重要**:目的是讓審閱者把 Figma 和 XAML 並排打開時,看到的是同一套詞彙。

免費版 Figma 夠用。發佈設定「Anyone with the link → can view」,連結放在 `README.md` 最上面。

## 1. Variables(Local variables 面板)

建一個 collection 叫 **Color**,兩個 mode:**Light** 和 **Dark**。依 `docs/design-token-map.md` 的值加入這些變數:

```
surface/base      surface/raised     surface/sunken
text/primary      text/secondary     text/disabled     text/on-accent
border/subtle     border/strong
accent/default    accent/hover       accent/pressed
status/idle       status/running     status/paused     status/error     status/ok
```

再建一個 collection 叫 **Layout**(單一 mode),放 number 變數:

```
space/1 = 4    space/2 = 8    space/3 = 12    space/4 = 16    space/5 = 24    space/6 = 32
radius/s = 4   radius/m = 8   radius/l = 16
touch/min = 48
```

把一個 frame 的 mode 從 Light 切到 Dark,整張應該會換色 —— 這就是 Figma 版的 `ThemeService.Apply`。

## 2. 文字樣式

| 樣式名 | 字型 | 大小 | 粗細 |
|---|---|---|---|
| Display | Segoe UI | 32 | Semibold |
| Title | Segoe UI | 20 | Semibold |
| Body | Segoe UI | 16 | Regular |
| Caption | Segoe UI | 13 | Regular |

## 3. Components

每一個都做成帶 variant 的 component。全部用 Auto Layout,padding 和 gap 從 Layout 變數取。

| Component | Variants | 對應到 |
|---|---|---|
| `Button / Primary` | Default, Hover, Pressed, Disabled | `Themes/Controls/Button.xaml` → `Button.Primary` |
| `Button / Secondary` | Default, Hover, Pressed, Disabled | `Button.Secondary` |
| `ToggleSwitch` | Off, On, Disabled | `Themes/Controls/ToggleSwitch.xaml` |
| `SegmentedControl` | 三個項目,一個選中 | `Themes/Controls/SegmentedControl.xaml` |
| `NumericStepper` | Default, At minimum, At maximum | `Themes/Controls/NumericStepper.xaml` |
| `StatusDot` | Idle, Running, Paused, Completed, Error | `Themes/Controls/Status.xaml` |
| `NavItem` | Default, Selected | `Themes/Controls/NavList.xaml` |
| `LevelPill` | Info, Warning, Error | `Views/EventLogView.xaml` |

高度:按鈕、開關、stepper 都是 48(touch/min)。Segmented 的項目是 40,外框 48。

## 4. Frames(1920 × 1080,一頁一張)

每張 frame 用上面的 component 拼。每張再複製一份切成 Dark mode。

1. **Dashboard** — 側邊欄(寬 88)、頂部列(高 64)、左邊進度環直徑 260、右邊 protocol 清單寬 380、左下四顆按鈕。
2. **Plate** — 工具列一排、盤面填滿剩餘空間、右側明細面板寬 340 滑入。
3. **Settings** — 「Run parameters」卡片五列(左標籤、右控制項),下方 token 展示台。
4. **Log** — 篩選列、量測卡片、每列高 40 的清單。

## 5. README 用的截圖

從跑起來的 App 截這幾張,放進 `docs/img/`:

| 檔名 | 內容 |
|---|---|
| `dashboard-light.png` | Dashboard 跑到一半,Light |
| `dashboard-dark.png` | 同一狀態,Dark |
| `plate-dark.png` | Plate 放大到約 250%,選中一個陽性孔、面板打開 |
| `settings-light.png` | Settings 頁上半 |
| `log-metrics.png` | Log 頁,虛擬化 OFF,realized 顯示 5000 |

然後把四張 Figma frame 用 1× 匯出,跟截圖放進 `README.md` 的兩欄表格(左 Figma、右 App)。

## 6. GIF(ScreenToGif,每段約 6 秒,≤ 3 MB)

| 檔名 | 錄什麼 |
|---|---|
| `theme-toggle.gif` | Settings 頁,按兩次主題按鈕 |
| `state-machine.gif` | Dashboard:Start → Pause → Resume → Abort 對話框 → Keep running |
| `plate-gestures.gif` | Plate:滾輪在某個孔上縮放、拖曳、點孔、面板滑入 |
