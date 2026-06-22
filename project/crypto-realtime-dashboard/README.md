# crypto-realtime-dashboard

> A pure-frontend, 60fps order book + trade tape + candlestick chart over Binance public WebSocket.
> Built to demonstrate React performance under high-frequency data.

**Live demo**: _coming soon (Vercel)_
**Stack**: React 19 · TypeScript · Vite 8 · Zustand · lightweight-charts · @tanstack/react-virtual

---

## Why this exists

Job posting requirements include _"trading systems / high real-time products"_ and _"Crypto / Fintech"_.
This project is a focused answer to those two lines: subscribe to Binance public WebSocket streams
(no API key, no backend), and render the resulting 50–100 messages/second into a UI that holds
60 fps without losing input responsiveness.

A backend was deliberately **not** built — Binance public endpoints have no CORS, no auth, no rate
limit at this scale, so a Node BFF would be over-engineering. The point of the demo is _front-end
depth_, not stack breadth.

---

## Phases

| Phase  | Subject                                                        | Status         |
| ------ | -------------------------------------------------------------- | -------------- |
| **P0** | Scaffold (Vite + React 19 + TS + Zustand + ESLint/Prettier)    | ✅ done        |
| **P1** | WebSocket plumbing (reconnect, heartbeat, sequence check)      | ⏳ in progress |
| **P2** | Order book with RAF batching & `useSyncExternalStore`          | ⏳             |
| **P3** | Trade tape (virtualized) + lightweight-charts K-line           | ⏳             |
| **P4** | Perf evidence: Profiler before/after + Lighthouse + Web Worker | ⏳             |
| **P5** | Multi-symbol switching + dark mode + RWD                       | ⏳             |
| **P6** | (optional) Railway BFF for klines cache + mock order           | ⏳             |
| **P7** | Deploy + docs + 30s demo recording                             | ⏳             |

---

## Architecture (target state)

```
                     ┌────────────────────────────────────────────┐
                     │   Binance public WebSocket streams         │
                     │   wss://stream.binance.com:9443/stream     │
                     └──────────────┬─────────────────────────────┘
                                    │ depth@100ms + trade + kline
                                    ▼
                  ┌────────────────────────────────────┐
                  │  src/services/binance/ws.ts        │
                  │  - reconnect (exp backoff)         │
                  │  - heartbeat / sequence check      │
                  │  - typed discriminated union msg   │
                  └──────────────┬─────────────────────┘
                                 │ raw events
                                 ▼
                  ┌────────────────────────────────────┐
                  │  src/stores/  (Zustand)            │
                  │  - orderBookStore                  │
                  │  - tradeStore                      │
                  │  - klineStore                      │
                  └──────────────┬─────────────────────┘
                  RAF-batched    │ useSyncExternalStore
                                 ▼
                  ┌────────────────────────────────────┐
                  │  React tree                         │
                  │  - <OrderBook>   memoized rows      │
                  │  - <TradeTape>   @tanstack/virtual  │
                  │  - <Chart>       lightweight-charts │
                  └────────────────────────────────────┘
```

---

## Design notes (will grow per phase)

### P0 — scaffold

- **Vite over Next.js**: data is real-time WS, SSR offers no value; the job posting names React only.
- **pnpm over npm**: faster install, smaller `node_modules`, friendly for an eventual monorepo.
- **Zustand over Redux**: high-frequency `setState` (~100/sec); Redux DevTools middleware adds
  measurable overhead per dispatch and we need every microsecond.
- **lightweight-charts over ECharts**: TradingView-native, canvas-rendered, ~5× smaller bundle.

### P1 — WebSocket plumbing

_will be written when P1 lands_

### P2 — Order book

_will be written when P2 lands_

### P3 — Trade tape & K-line

_will be written when P3 lands_

### P4 — Performance evidence

_will be written when P4 lands_

---

## Scripts

```bash
pnpm dev            # start Vite dev server (default http://localhost:5173)
pnpm build          # type-check then build production bundle to dist/
pnpm preview        # preview the production build locally
pnpm lint           # ESLint
pnpm typecheck      # tsc -b --noEmit
pnpm test           # Vitest, run once
pnpm test:watch     # Vitest, watch mode
pnpm test:ui        # Vitest UI
pnpm format         # Prettier write
pnpm format:check   # Prettier check
```

After `pnpm build`, a bundle analysis report is written to `dist/bundle-stats.html`.

---

## License

MIT — built as a public job-application demo by [@joy20020606](https://github.com/joy20020606).
