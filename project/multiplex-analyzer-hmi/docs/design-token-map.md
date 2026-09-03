# Design token 對照表 — Figma ↔ XAML

App 裡每一個顏色都是 token。Figma variable 和 XAML resource key 用同一套名字、只差分隔符號,所以設計師說「我改了 `surface/raised`」,就直接對到 `Tokens.Light.xaml` 和 `Tokens.Dark.xaml` 各一行。

規則:**控制項樣式只引用 key,兩個 token 檔以外不出現任何字面色碼。**

## 顏色

| Figma variable | XAML key | Light | Dark | 用在 |
|---|---|---|---|---|
| `color/surface/base` | `Brush.Surface.Base` | `#FFFFFF` | `#161616` | 視窗、卡片底 |
| `color/surface/raised` | `Brush.Surface.Raised` | `#F4F4F4` | `#262626` | 側邊欄、面板、hover |
| `color/surface/sunken` | `Brush.Surface.Sunken` | `#E0E0E0` | `#0B0B0B` | 軌道、disabled 填色、空孔 |
| `color/text/primary` | `Brush.Text.Primary` | `#161616` | `#F4F4F4` | 內文 |
| `color/text/secondary` | `Brush.Text.Secondary` | `#525252` | `#A8A8A8` | 說明文字、標籤 |
| `color/text/disabled` | `Brush.Text.Disabled` | `#A8A8A8` | `#6F6F6F` | disabled 控制項 |
| `color/text/on-accent` | `Brush.Text.OnAccent` | `#FFFFFF` | `#FFFFFF` | accent 底上的文字 |
| `color/border/subtle` | `Brush.Border.Subtle` | `#E0E0E0` | `#393939` | 卡片外框、分隔線 |
| `color/border/strong` | `Brush.Border.Strong` | `#8D8D8D` | `#6F6F6F` | 輸入框外框、孔的邊線 |
| `color/accent/default` | `Brush.Accent.Default` | `#0F62FE` | `#4589FF` | 主要操作、選取 |
| `color/accent/hover` | `Brush.Accent.Hover` | `#0353E9` | `#78A9FF` | 主要按鈕 hover |
| `color/accent/pressed` | `Brush.Accent.Pressed` | `#002D9C` | `#0F62FE` | 主要按鈕 pressed |
| `color/status/idle` | `Brush.Status.Idle` | `#8D8D8D` | `#6F6F6F` | 狀態燈 |
| `color/status/running` | `Brush.Status.Running` | `#0F62FE` | `#4589FF` | 狀態燈 |
| `color/status/paused` | `Brush.Status.Paused` | `#D2A106` | `#F1C21B` | 狀態燈、warning 標籤 |
| `color/status/error` | `Brush.Status.Error` | `#DA1E28` | `#FA4D56` | 狀態燈、error 標籤、陽性孔 |
| `color/status/ok` | `Brush.Status.Ok` | `#198038` | `#42BE65` | 完成狀態、已完成步驟 |

Dark 的值不是單純反轉:accent 和 status 在 Dark 都往亮拉,才能在 `#161616` 底上維持對比。

## 字級

| Figma 文字樣式 | XAML style key | 大小 | 粗細 |
|---|---|---|---|
| `Display` | `Text.Display` | 32 | SemiBold |
| `Title` | `Text.Title` | 20 | SemiBold |
| `Body` | `Text.Body` | 16 | Regular |
| `Caption` | `Text.Caption` | 13 | Regular |

字型 token:`Font.Family` = Segoe UI。最小字級 13(caption),來自「操作者站著看螢幕」的限制。

## 間距與形狀

| Figma | XAML key | 值 |
|---|---|---|
| `space/1 … 6` | `Space.1 … Space.6` | 4, 8, 12, 16, 24, 32 |
| `radius/s` | `Radius.S` | 4 |
| `radius/m` | `Radius.M` | 8 |
| `radius/l` | `Radius.L` | 16 |
| `touch/min` | `TouchTarget.Min` | 48 |

`TouchTarget.Min` 套用在 `Button.Base`、`ToggleSwitch`、`NumericStepper` 和 stepper 的連發按鈕。**48 dp 的規則只有這一個地方可以被違反。**

## Static vs Dynamic

| 種類 | 查找方式 | 理由 |
|---|---|---|
| 顏色 | `DynamicResource` | 會隨主題變 |
| 尺寸、圓角、字型、樣板 | `StaticResource` | 永遠不變,省掉監聽成本 |

Settings 頁有並排對照:兩個方塊綁同一個 key,切主題時只有 Dynamic 那個會重繪。
