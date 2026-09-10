# Figma 設計稿筆記(Phase 1 抽 tokens 用)

來源:8AM.DESIGN「Free Hotel Responsive Landing Page」(CC BY 4.0)
https://www.figma.com/community/file/1377492425738686159/free-hotel-responsive-landing-page
使用者複本:https://www.figma.com/design/DuCxqcTLhyqeIUPU2fa09L/(fileKey `DuCxqcTLhyqeIUPU2fa09L`)
資料來源:Figma 官方 MCP(`get_variable_defs` / `get_screenshot` / `get_metadata`),2026-09-03 抓取

## 設計 tokens(get_variable_defs 於 Desktop frame)

### 顏色

| Figma 名稱 | hex | 用途 |
|---|---|---|
| `$-Gold-500` | `#BF9766` | 強調色(副標、按鈕、hover、focus ring) |
| `$-Dark Grey-500` | `#2D2C2C` | 深底(Booking Form 卡片、深色區塊) |
| `$-Dark Grey-400` | `#575656` | 次要深灰 |
| `$-Dark Grey-300` | `#818080` | 淡文字 / placeholder |
| `Heading Text` | `#171717` | 淺底上的標題文字 |
| `Gray 4` | `#BDBDBD` | 邊線 / 分隔線 |
| `White` | `#FFFFFF` | 深底上的文字 |
| Figma 畫布背景 | `#1E1E1E` | 僅畫布,非設計色 |

### 字型(Google Fonts 都有)

| Figma 樣式 | family | weight | size / line-height | letter-spacing |
|---|---|---|---|---|
| Heading/H1 | Forum | 400 | 112 / 1.24 | 0 |
| Heading/H3 | Forum | 400 | 64 / 1.26 | 0 |
| Heading/H6 | Forum | 400 | 24 / 1.0 | 0 |
| Subheading/SH1 | Poppins Light | 300 | 24 / 1.0 | **24px**(全大寫小標) |
| Body/Body1 | Poppins Light | 300 | 18 / 1.0 | 0 |
| Body/Body2 | Poppins Light | 300 | 16 / 1.0 | 0 |
| Body/Body3 | Poppins Light | 300 | 14 / 1.0 | 0 |

> Figma 的 line-height 100 = 1.0,實作時 body 建議放寬到 1.6 才好讀,H1 保留 1.24。

## Frame 與 node ID

| Frame | node ID | 寬 × 高 | PNG |
|---|---|---|---|
| Desktop - 02. Home Page 02 | `0:91` | 1428 × 7139 | figma-desktop.png |
| Tablet - 02. Home Page 3 | `72:20` | 769 × 6446 | figma-tablet.png |
| mobile - 02. Home Page 4 | `127:614` | 480 × 8174 | figma-mobile.png |
| tablet - responsive menu | `26:1511` | 769 × 1024 | figma-tablet-menu.png |
| mobile - responsive menu | `26:1487` | 480 × 800 | figma-mobile-menu.png |

## Desktop 區塊 node ID(Phase 2 逐段呼叫 get_design_context 用)

| 區塊 | node ID | 位置 / 尺寸 |
|---|---|---|
| Header(hero) | `0:92` | y 0,1440 × 901 |
| Navbar | `0:133` | x 120 y 36,1186 × 58 |
| Booking Form | `0:136` | x 907 y 621,405 × 517 |
| About | `0:213` | y 1081,1192 × 609 |
| Room Category | `0:231` | y 1834,1434 × 1033 |
| Testimonials | `0:314` | y 3057,757 × 687 |
| Staff | `0:341` | y 3924,1429 × 903 |
| Blog | `0:378` | y 5007,1201 × 886 |
| CTA | `0:448` | y 6073,1206 × 362 |
| footer | `16:430` | y 6615,1434 × 524 |

Tablet 區塊:Header `72:21`、Navbar `72:75`、Booking Form `72:88`、About `72:179`、Room Category `72:204`、Testimonials `72:304`、Staff `72:344`、Blog `72:390`、CTA `72:509`、footer `72:527`
Mobile 區塊:Header `127:615`、Navbar `127:669`、Booking Form `127:683`、About `127:774`、Room Category `127:799`、Testimonials `127:898`、Staff `127:938`、Blog `127:984`、CTA `127:1103`、footer `127:1121`

## 版面數字

- Desktop 內容區:x 120 起、寬 1186~1206 → container max-width **1200px**,兩側 padding 120
- Tablet 內容區:x 73 起、寬 623 → padding 72
- Mobile 內容區:x 32 起、寬 416 → padding 32
- Breakpoints 建議:sm 375(mobile 稿 480 縮放)、md 768(tablet 稿 769)、lg 1280(desktop 稿 1428)
