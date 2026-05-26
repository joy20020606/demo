# ADR 002: Polyglot Persistence — Three Databases

| Field | Value |
|-------|-------|
| Status | Accepted |
| Date | 2026-05-14 |
| Author | Joy Chuang |
| Supersedes | — |

## Context

Day 2 起，系統需要儲存三種性質完全不同的資料：

1. **業務核心資料** — 租戶、使用者、路線、獎勵方案。結構固定、彼此有關聯、需要交易一致性。
2. **通勤事件資料** — 上車打卡、共乘配對、壅塞回報等。量大、只新增不修改、每種事件欄位不同。
3. **暫時性資料** — 查詢快取、限流計數器。讀寫極頻繁、可容忍遺失、需要自動過期。

問題：要用一種資料庫硬撐全部，還是各用適合的？

## Decision

採用 **polyglot persistence（多元儲存）** —— 三種資料庫分工：

| 資料庫 | 存什麼 | 存取方式 |
|--------|--------|----------|
| PostgreSQL | 業務核心資料 | Prisma ORM |
| MongoDB | 通勤事件 | Mongoose ODM |
| Redis | 快取 + 限流計數器 | ioredis |

## Rationale

### 為什麼業務資料留在 PostgreSQL
- 租戶 / 使用者 / 路線之間有明確的外鍵關聯
- 需要交易一致性（ACID）
- 結構穩定，關聯式模型最合適

### 為什麼通勤事件用 MongoDB
- **彈性 schema 是關鍵**：`CHECK_IN` 的 payload 是 `{commuterCount}`，`CONGESTION_REPORT` 是 `{congestionIndex, avgSpeedKmh}` —— 每種事件結構不同。關聯式資料庫要嘛開很多張表、要嘛塞一堆 nullable 欄位；MongoDB 一個 collection、每筆文件自帶所需欄位。
- 事件是 append-heavy（只新增、幾乎不改），MongoDB 寫入吞吐量高
- 未來事件量會非常大，文件式資料庫水平擴展容易

### 為什麼快取 / 限流用 Redis
- 記憶體儲存，讀寫是微秒級
- 原生支援 TTL（自動過期）—— 快取失效、限流視窗都靠它
- `INCR` 是原子操作，多個 server 實例共用計數器不會 race condition

## Consequences

### 正面
- 每種資料用最合適的引擎，效能與開發體驗都最佳
- 展示對不同資料庫特性的理解（架構能力）

### 負面
- 維運複雜度上升：要顧三個資料庫
  - 緩解：docker-compose 一鍵起；正式環境用 managed service（RDS / Atlas / Upstash）
- 跨資料庫沒有交易：寫了 MongoDB 又寫 PostgreSQL 無法原子性保證
  - 緩解：目前的寫入路徑不跨庫；未來若需要，用 outbox pattern / 事件補償
- 沒有跨庫 join：事件的 `routeId` 對應 PostgreSQL 的 Route，需在應用層組合
  - 緩解：可接受；必要時在 API 層 join，或於 Day 5 用 ClickHouse 做分析

## Alternatives Considered

| 方案 | 不選的理由 |
|------|-----------|
| 全部用 PostgreSQL（事件塞 JSONB 欄位） | 可行，但事件量大時效能不如文件式資料庫；快取用 DB 表也遠慢於 Redis |
| 全部用 MongoDB | 業務資料的關聯與交易一致性會很難處理 |
| 加 ClickHouse 做事件分析 | Day 2 暫不需要；規劃於 Day 5 處理 OLAP 分析場景 |

## References

- Martin Fowler, "PolyglotPersistence"
- ADR-001: Fastify over Express
