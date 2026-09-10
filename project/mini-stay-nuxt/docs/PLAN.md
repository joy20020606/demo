# mini-stay-nuxt 實作計畫

> 民宿訂房網站 + 簡易後台。單一 Nuxt 4 App,Figma 設計稿像素級還原,強調「設計稿→HTML」「跨瀏覽器/RWD」「jQuery 遷移」「重構」。
> 目標職缺:拓境科技 前端開發工程師(遠端)。
> 事實查核(2026-09):Nuxt 4.5.2 / Nitro `vercel` preset 零設定 / Drizzle 0.45.2 stable(1.0 RC 不採用) / Playwright 1.62 / Pinia 3.x。
> 設計稿:8AM.DESIGN「Free Hotel Responsive Landing Page」(CC BY 4.0,含 Desktop / Tablet / Mobile)
> https://www.figma.com/community/file/1377492425738686159/free-hotel-responsive-landing-page
> 設計稿只給首頁;內頁(列表 / 詳情 / 訂房 / 後台)用同一套 tokens 延伸,README 註明並 credit 設計者。

## 1. 一句話定位 + 面試金句

**定位**:一個「拿到 Figma 就能交付上線的響應式網站」的完整證據鏈,不是 CRUD 展示。

| JD 關鍵字 | Demo 裡的證據 | 一句話 |
|---|---|---|
| 設計稿→功能完整 HTML | `docs/design/*.png` vs `docs/screenshots/*.png` 並排比對表(README) | 「三個斷點我都對著 Figma 逐段還原,差異用截圖矩陣自證。」 |
| 跨瀏覽器相容 | Playwright Chromium/WebKit/Firefox × 3 viewport 截圖矩陣;browserslist + autoprefixer | 「相容性不是靠信仰,是 9 張截圖跑 CI 出來的。」 |
| 行動裝置響應式 | mobile-first mixins、`clamp()` 流式字級、漢堡選單、Grid + `@supports` fallback | 「先寫 375,再往上加 breakpoint,永遠不寫 max-width 反向覆蓋。」 |
| CSS/SASS | 手寫 SCSS:tokens → mixins → BEM 元件,零 UI 框架 | 「不用 Tailwind 是刻意的,這份作業要看得到 CSS 功力。」 |
| jQuery | `<LegacyDateRangePicker>` 包裝 jQuery 外掛,props/emits 橋接,unmount 銷毀 | 「Strangler:先包起來讓它活在 Vue 生命週期裡,再逐步替換。」 |
| RESTful API 串接 | Nitro `server/api` + Zod schema 前後端共用,409 衝突處理 | 「Schema 只寫一次,client/server 驗證同一份。」 |
| 重構 | 獨立 commit:mock JSON → Drizzle,API 契約與 e2e 測試零改動 | 「重構的定義是測試不變、行為不變、實作換掉。」 |
| Vue.js / Nuxt.js (MVC) | `<script setup>` + `useAsyncData` SSR + Pinia + route/server middleware | 「第一個 Nuxt 專案,但 SSR/資料流/中介層的心智模型跟 Next.js 是同一套。」 |

## 2. 架構圖 + 資料夾結構

```
Browser ──HTTP──▶ Vercel (Nitro `vercel` preset, Node serverless)
                     │
                     ├─ SSR: app/pages/*  ── useAsyncData ──▶ server/api/*  (同源,無 CORS)
                     │                                           │ Zod (shared/schemas)
                     │                                           ▼
                     │                                     server/db (Drizzle + postgres-js, prepare:false)
                     │                                           │ sslmode=require
                     └─ /admin/* ── route middleware ─▶ server/middleware/admin-guard ─▶ Railway Postgres
                                   (cookie: admin_session, httpOnly, HMAC signed)
```

```
mini-stay-nuxt/
├─ app/
│  ├─ app.vue, error.vue
│  ├─ assets/scss/  _tokens.scss _mixins.scss _reset.scss _base.scss main.scss
│  ├─ components/   AppHeader.vue AppFooter.vue HeroSection.vue SearchBar.vue
│  │                AmenitiesSection.vue RoomCard.vue RoomsPreview.vue TestimonialCta.vue
│  │                BookingForm.vue BookingSummary.vue SkeletonCard.vue
│  │                LegacyDateRangePicker.client.vue   admin/BookingsTable.vue admin/StatusBadge.vue
│  ├─ composables/  useRoomsQuery.ts useAvailability.ts useAdminSession.ts
│  ├─ layouts/      default.vue admin.vue
│  ├─ middleware/   admin.ts
│  ├─ pages/        index.vue rooms/index.vue rooms/[slug].vue book/[slug].vue book/success/[code].vue
│  │                admin/login.vue admin/index.vue
│  ├─ plugins/      jquery.client.ts
│  └─ stores/       booking.ts
├─ server/
│  ├─ api/          rooms/index.get.ts rooms/[slug].get.ts rooms/[slug]/availability.get.ts
│  │                bookings/index.post.ts admin/login.post.ts admin/bookings/index.get.ts admin/bookings/[id].patch.ts
│  ├─ db/           schema.ts client.ts seed.ts
│  ├─ middleware/   admin-guard.ts
│  └─ utils/        errors.ts session.ts availability.ts
├─ shared/
│  ├─ schemas/      room.ts booking.ts admin.ts        (import via #shared/schemas/*)
│  └─ utils/        dates.ts price.ts                  (auto-import)
├─ tests/  e2e/ (screenshots.spec.ts booking.spec.ts admin.spec.ts)  unit/ (BookingForm.spec.ts availability.spec.ts)
├─ docs/   design/ (figma-{desktop,tablet,mobile}.png)  screenshots/ ({page}-{browser}-{vw}.png)  cover-letter.md
├─ public/images/rooms/*.webp
├─ drizzle.config.ts  playwright.config.ts  vitest.config.ts  .browserslistrc  nuxt.config.ts  .env.example
```

## 3. 資料模型

| 表 | 欄位 |
|---|---|
| `rooms` | `id serial PK`, `slug text unique`, `name text`, `description text`, `price_per_night int`(TWD 整數), `max_guests int`, `image_url text`, `amenities jsonb` (`string[]`), `created_at timestamptz default now()` |
| `bookings` | `id serial PK`, `code text unique`(如 `MS-7K3P2Q`), `room_id int FK→rooms`, `guest_name`, `email`, `phone`, `check_in date`, `check_out date`, `guests int`, `status booking_status enum('pending','confirmed','cancelled') default 'pending'`, `total_price int`, `created_at timestamptz` |
| Index | `bookings(room_id, check_in, check_out)`;CHECK `check_out > check_in` |

**可用性規則**:同房型、`status != 'cancelled'`、且 `check_in < :newOut AND check_out > :newIn`(半開區間,退房日可入住)→ 衝突。
**強制點**:`POST /api/bookings` 內 `db.transaction(async tx => { SELECT ... FOR UPDATE; if conflict throw 409; INSERT })`。
**Seed**:6 房型(對應 Figma rooms 區塊風格)、10 筆訂單(含 2 筆 cancelled 證明不阻擋)。冪等:`onConflictDoNothing({ target: rooms.slug })`;bookings 以固定 `code` 去重。

## 4. RESTful API

| Method | Path | Query/Body (Zod) | 200 回應 | 錯誤 |
|---|---|---|---|---|
| GET | `/api/rooms` | `?guests&checkIn&checkOut`(全選填;三者齊備才過濾可用) | `{ data: Room[] }` | 400 |
| GET | `/api/rooms/:slug` | – | `{ data: Room }` | 404 |
| GET | `/api/rooms/:slug/availability` | `?month=YYYY-MM` | `{ data: { month, blocked: ['YYYY-MM-DD',...] } }` | 400/404 |
| POST | `/api/bookings` | `CreateBookingInput` | 201 `{ data: { code, totalPrice, ... } }` | 400/404/**409** |
| POST | `/api/admin/login` | `{ password }` | `{ data: { ok: true } }` + Set-Cookie | 401 |
| GET | `/api/admin/bookings` | `?status=` | `{ data: BookingWithRoom[] }` | 401 |
| PATCH | `/api/admin/bookings/:id` | `{ status }` | `{ data: Booking }` | 400/401/404 |

**錯誤信封**:`{ error: { code: 'VALIDATION_ERROR' | 'NOT_FOUND' | 'DATE_CONFLICT' | 'UNAUTHORIZED', message, issues?: ZodIssue[] } }`,由 `server/utils/errors.ts` 的 `createApiError(status, code, message)` 統一產生。
**Schema 位置**:`shared/schemas/booking.ts` 匯出 `createBookingSchema`、`bookingStatusSchema`、`roomsQuerySchema`;前端 `BookingForm` 與 `server/api/bookings/index.post.ts` 同一份 `safeParse`。
**狀態碼慣例**:201 建立、400 Zod 失敗、401 未登入、404 找不到、409 日期衝突、500 其他(不外洩 stack)。

## 5. Phase 分解

### Phase 1 設計稿與 tokens(人工,1h)
- **目標**:專案骨架 + 設計系統基礎 + 版型殼。
- **產出**:`nuxt.config.ts`(modules: `@pinia/nuxt`; css: `~/assets/scss/main.scss`; vite `additionalData` 注入 tokens+mixins)、`_tokens.scss`(SCSS map + `:root` CSS vars:`--color-brown-900/700`, `--color-gold`, `--color-cream`, `--font-serif`, `--font-sans`, `--space-1..12`, `$breakpoints: (sm:375px, md:768px, lg:1280px)`)、`_mixins.scss`(`@mixin up($bp)`, `@mixin container`, `@mixin visually-hidden`)、`_reset.scss`、`layouts/default.vue`、`AppHeader.vue`(漢堡 + `aria-expanded`)、`AppFooter.vue`、`.browserslistrc`(`defaults, not IE 11`)、ESLint(`@nuxt/eslint`)、`docs/design/figma-{desktop,tablet,mobile}.png`。
- **完成標準**:`pnpm install && pnpm lint && pnpm typecheck && pnpm build` exit 0;`docs/design/` 有 3 張 PNG;`grep -c "clamp(" app/assets/scss/_tokens.scss` ≥ 1;dev 下 375px 漢堡可開合(人工)。
- **講點**:tokens 同時輸出 SCSS map(編譯期 breakpoint)與 CSS vars(執行期主題),兩者分工。

### Phase 2 首頁像素級還原(人工,1.5h)
- **目標**:`index.vue` 六個區塊三斷點對齊 Figma。
- **產出**:`HeroSection` `SearchBar`(check-in/out/guests,submit 導向 `/rooms?...`)`AmenitiesSection` `RoomsPreview`(`useAsyncData('rooms-preview', () => $fetch('/api/rooms'))`,Phase 3 前先讀 mock)`TestimonialCta`;`tests/e2e/screenshots.spec.ts`(只跑 chromium、輸出 `docs/screenshots/home-chromium-{375,768,1280}.png`);README「設計稿 vs 實作」表。
- **完成標準**:`pnpm screenshots` 產出 3 檔;`pnpm lint typecheck build` 通過;人工目測差異 < 4px(hero 標題、search bar 對齊、卡片間距);Lighthouse a11y ≥ 90(本地 `pnpm dlx lighthouse http://localhost:3000 --preset=desktop --only-categories=accessibility`)。
- **講點**:每個 section 一個 BEM block(`.hero__title`),無 utility class;section 內用 Grid,卡片用 `auto-fit/minmax` 免寫 breakpoint。

### Phase 3 房型列表與詳情(Workflow,1.5h)
- **目標**:資料頁 SSR + mock→Drizzle 重構。
- **步驟 A(mock)**:`server/api/rooms/*.get.ts` 讀 `server/data/rooms.json`;`rooms/index.vue` 以 `useRoute().query` + `roomsQuerySchema` 驅動 `useAsyncData(key含query)`;`SkeletonCard` 於 `status==='pending'`;`rooms/[slug].vue` 用 `useSeoMeta` + `createError({statusCode:404})`;`tests/e2e/rooms.spec.ts` 驗證 3 個 API 契約 + 404。
- **步驟 B(refactor commit `refactor: migrate rooms API from JSON to Drizzle`)**:`server/db/schema.ts` `client.ts`(`postgres(url,{prepare:false,max:1})`)、`drizzle.config.ts`、`seed.ts`;handler 換實作,**測試檔零 diff**。
- **完成標準**:`pnpm db:push && pnpm db:seed`(重跑兩次不報錯);`pnpm test:e2e --project=chromium tests/e2e/rooms.spec.ts` 通過;`git diff <A>..<B> -- tests/` 為空;`curl /api/rooms/nope` → 404 且 body 符合錯誤信封;`curl "/api/rooms?checkIn=2026-10-01&checkOut=2026-10-03&guests=2"` 排除 seed 中衝突房。
- **講點**:重構 = 契約不變 + 測試不變;`useAsyncData` key 含 query 才能正確 SSR/cache。

### Phase 4 訂房流程(Workflow,1h)
- **產出**:`stores/booking.ts`(draft: roomSlug/checkIn/checkOut/guests/guest info;`totalNights`/`totalPrice` getters;`persist` 不做)、`BookingForm.vue`(欄位級錯誤、`aria-describedby`、submit disabled 規則)、`book/[slug].vue`(`BookingSummary` 側欄)、`server/api/bookings/index.post.ts`(transaction + 409)、`book/success/[code].vue`。
- **409 處理**:store `submit()` 先 `status='submitting'` 顯示 optimistic 摘要,失敗 → `conflict=true`,表單頂部 alert「該日期已被預訂」並呼叫 availability API 標示 blocked。
- **完成標準**:`tests/e2e/booking.spec.ts`:成功流程到 success 頁看到 `MS-` code;對 seed 已占用日期 POST → 409 且 UI 顯示 alert;`tests/unit/BookingForm.spec.ts`(空 email / checkOut ≤ checkIn 顯示錯誤);`pnpm test:unit && pnpm test:e2e --project=chromium` 通過。
- **講點**:Pinia 只存跨頁草稿,不存 server state(那是 useAsyncData 的事)。

### Phase 5 jQuery 互操作(Workflow,45m)
- **選擇**:**jQuery date-range picker(`daterangepicker` by Dan Grossman)**,不選 Slick。理由:它直接落在核心流程(SearchBar + BookingForm),且雙向資料橋接(props→`setStartDate`、`apply.daterangepicker`→emit)比輪播更能展示 interop 深度;Slick 只是展示層。
- **產出**:`plugins/jquery.client.ts`(`import $ from 'jquery'; window.jQuery=$; await import('daterangepicker')`)、`LegacyDateRangePicker.client.vue`(`.client` 後綴 = 自動 ClientOnly;`onMounted` 初始化,`watch(props)` 同步,`onBeforeUnmount` 呼叫 `.data('daterangepicker').remove()`)、`docs/strangler.md`(現況 → 邊界 → 替換路徑)。
- **完成標準**:`pnpm build` 無 hydration warning(`tests/e2e/booking.spec.ts` 增 `page.on('console')` 斷言無 `Hydration` 字串);切換路由 3 次後 `document.querySelectorAll('.daterangepicker').length === 0`(記憶體洩漏檢查);e2e 用 picker 選日期後 store 值更新。
- **講點**:Strangler 三步:包裝(現在)→ 介面對齊(props/emits 與未來原生元件一致)→ 換掉(SSR 端不需知道)。

### Phase 6 後台(Workflow,1h)
- **產出**:`server/utils/session.ts`(`hmac(sha256, NUXT_SESSION_SECRET)` 簽 `admin:<exp>`)、`admin/login.post.ts`(`timingSafeEqual` 比對 `NUXT_ADMIN_PASSWORD`,`setCookie(...,{httpOnly,secure:!dev,sameSite:'lax',maxAge:8h})`)、`server/middleware/admin-guard.ts`(僅攔 `/api/admin/*` 排除 login)、`app/middleware/admin.ts`(`definePageMeta({middleware:'admin', layout:'admin'})`,呼叫 `/api/admin/bookings` 探針 401 → redirect)、`admin/index.vue`(`BookingsTable` + status filter + PATCH optimistic,失敗回滾)。
- **完成標準**:`tests/e2e/admin.spec.ts`:未登入 GET `/api/admin/bookings` → 401;`/admin` 導向 `/admin/login`;錯密碼 401;正確密碼後表格 ≥ 10 列;PATCH 改 confirmed 後 reload 仍為 confirmed。
- **講點**:單一密碼 + 簽章 cookie 是「知道不做完整 auth」的取捨;guard 兩層(route 為 UX、server 為安全)。

### Phase 7 品質與上線(Workflow + 人工 deploy,1.5h)
- **產出**:`playwright.config.ts`(9 projects = `{chromium,firefox,webkit} × {375×812, 768×1024, 1280×800}`,`webServer:{command:'pnpm preview', url, reuseExistingServer:!CI}`)、`screenshots.spec.ts` 擴為 home/rooms/room-detail × 9、`tests/unit/availability.spec.ts`(半開區間 5 案例)、`.github/workflows/ci.yml`(lint/typecheck/unit/e2e chromium)、README、`docs/cover-letter.md`。
- **完成標準**:`pnpm test:e2e` 全綠;`ls docs/screenshots | wc -l` ≥ 27;`pnpm dlx lighthouse <vercel-url> --preset=mobile --only-categories=performance,accessibility,seo` 三項 ≥ 90;README 含 badges、live URL、CC BY credit、技術點↔檔案表、「不做清單」。
- **講點**:截圖矩陣是「跨瀏覽器」的可驗證交付物,不是口頭承諾。

## 6. 跨瀏覽器 / 響應式策略

| 技術 | 作法 |
|---|---|
| Mobile-first | `@mixin up($bp) { @media (min-width: map.get($breakpoints,$bp)) { @content } }`;禁用 `max-width` query |
| 流式字級 | `--fs-h1: clamp(2rem, 1.2rem + 3.5vw, 4rem)`;body `clamp(1rem, .95rem + .25vw, 1.125rem)` |
| 容器 | `.container{ width:min(100% - 2*var(--space-4), 1200px); margin-inline:auto }` |
| Grid + fallback | 卡片 `grid-template-columns: repeat(auto-fit, minmax(280px,1fr))`;`@supports not (display:grid)` → flex-wrap |
| 新特性守門 | `@supports (inset: 0)`、`aspect-ratio` 缺席時 padding-top hack |
| Autoprefixer | `.browserslistrc: defaults, not IE 11, not op_mini all`;Nuxt/Vite 內建 postcss autoprefixer 讀取 |
| a11y | `:focus-visible` outline gold、`prefers-reduced-motion` 關閉 hero 動畫、`prefers-color-scheme` 不做 |
| 圖片 | `<picture>` + webp/jpg、`loading="lazy"`(hero 除外 + `fetchpriority="high"`) |
| 測試矩陣 | 3 瀏覽器 × 3 viewport;WebKit 代表 Safari(Windows 無真 Safari 的最佳替代) |

## 7. 風險與雷

| 風險 | 對策 |
|---|---|
| Nuxt 4 `app/` 目錄:`~`/`@` alias 指向 `app/`,`server/` 引用 `shared/` 需 `#shared` | 統一 `#shared/schemas/*`;`shared/` 內不 import Vue/Nitro(官方限制) |
| jQuery 於 SSR 造成 hydration mismatch | 元件 `.client.vue` 後綴 + plugin `.client.ts`;picker 容器由 Vue 渲染,jQuery 只掛在 `ref` 上 |
| Drizzle on Vercel serverless 連線爆量 | `postgres(url,{prepare:false, max:1, idle_timeout:20})`;Railway 公開 URL 加 `?sslmode=require`;client 用模組單例 |
| Drizzle 1.0 RC 與文件混雜 | 鎖 `drizzle-orm@^0.45 drizzle-kit@^0.31`,不用 `@rc` |
| env 命名 | `NUXT_DATABASE_URL` `NUXT_ADMIN_PASSWORD` `NUXT_SESSION_SECRET`(private);無 public 變數 |
| Vercel env 未勾 Production | `.env.example` 註解提醒;deploy 後 `curl /api/rooms` 驗證 |
| Nitro preset | 零設定自動偵測 `vercel`;`nitro.vercel.functions.maxDuration` 不需改 |
| CORS | 同源不需要;README 註明 |
| 圖片來源 | 本地 `public/images/rooms/*.webp`(Unsplash 下載後壓 1600w),**不用 `@nuxt/image`**:6 張固定圖,加 module 只增加建置面;README 說明 |
| Windows 換行 | 不寫 `.sh`;腳本一律 `scripts/*.mjs` + `cross-env`;`.gitattributes: * text=auto eol=lf` |
| Playwright WebKit on Windows | `pnpm exec playwright install --with-deps` 於 CI;本地首次安裝約 300MB |
| 時區 | 日期以 `YYYY-MM-DD` 字串傳遞,DB `date` 型別,不用 `timestamp` |

## 8. 不做清單

| 不做 | 為何不做 |
|---|---|
| i18n | JD 未提;加 `@nuxtjs/i18n` 會讓每個字串多一層間接,遮蔽 CSS/RWD 重點 |
| 付款 | 需第三方沙盒 + webhook,與前端職缺評估點無關 |
| 完整 auth(註冊/JWT/RBAC) | 後台只有一個角色;單密碼 + 簽章 cookie 已涵蓋「路由保護」考點 |
| 獨立後端/微服務 | Nitro 已是 RESTful 層;拆出去只增加部署面與 CORS |
| Tailwind / Nuxt UI | 職缺要看 CSS/SASS 能力,工具類會把它藏起來 |
| `@nuxt/image` | 6 張靜態圖,手動 webp 即可;少一個建置依賴 |
| Storybook | 元件數 < 15,截圖矩陣已提供視覺回歸 |
| 即時可用性(WebSocket) | 訂房衝突用 409 + 重查即可,serverless 不適合長連線 |

## 9. 驗證指令總表

| script | 指令 | 用途 |
|---|---|---|
| `dev` | `nuxt dev` | 開發 |
| `build` / `preview` | `nuxt build` / `node .output/server/index.mjs` | 生產建置 / 本地預覽(e2e 用) |
| `lint` | `eslint .` | @nuxt/eslint |
| `typecheck` | `nuxt typecheck` | vue-tsc |
| `test:unit` | `vitest run` | Vitest + `@nuxt/test-utils` env |
| `test:e2e` | `playwright test` | 9 projects |
| `screenshots` | `playwright test tests/e2e/screenshots.spec.ts` | 寫入 `docs/screenshots/` |
| `db:push` | `drizzle-kit push` | schema → Railway |
| `db:seed` | `tsx server/db/seed.ts` | 冪等 seed |
| `lighthouse` | `lighthouse http://localhost:3000 --preset=mobile --output=json --output-path=docs/lighthouse.json` | 品質數據 |

**Phase 門檻總覽**:P1 `lint+typecheck+build` → P2 `+screenshots(3)` → P3 `+db:push/seed + e2e rooms` → P4 `+unit + e2e booking` → P5 `+無 hydration warn` → P6 `+e2e admin` → P7 全矩陣 + Lighthouse ≥ 90 + Vercel live。

## 參考來源

- Nuxt 4 directory structure: https://nuxt.com/docs/4.x/directory-structure
- Nuxt shared/: https://nuxt.com/docs/4.x/guide/directory-structure/shared
- Nitro Vercel: https://nitro.build/deploy/providers/vercel
- Drizzle postgres-js: https://orm.drizzle.team/docs/get-started-postgresql
- Playwright projects: https://playwright.dev/docs/test-projects
- Pinia Nuxt: https://pinia.vuejs.org/ssr/nuxt.html
