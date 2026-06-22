import './App.css'

export default function App() {
  return (
    <main className="shell">
      <header className="hero">
        <span className="badge">P0 · scaffold</span>
        <h1>
          crypto-realtime-dashboard <span className="muted">/ coming soon</span>
        </h1>
        <p className="lede">
          A pure-frontend, 60fps order book + trade tape + candlestick chart over Binance public
          WebSocket. Built to demonstrate React performance under high-frequency data.
        </p>
        <ul className="checklist">
          <li>
            <code>P0</code> scaffold (Vite + React 19 + TS + Zustand)
          </li>
          <li>
            <code>P1</code> WebSocket plumbing (reconnect, heartbeat, sequence check)
          </li>
          <li>
            <code>P2</code> Order book with RAF batching &amp; useSyncExternalStore
          </li>
          <li>
            <code>P3</code> Trade tape (virtualized) + lightweight-charts K-line
          </li>
          <li>
            <code>P4</code> Perf evidence: Profiler before/after + Lighthouse
          </li>
          <li>
            <code>P5</code> Multi-symbol switching + dark mode + RWD
          </li>
        </ul>
      </header>
    </main>
  )
}
