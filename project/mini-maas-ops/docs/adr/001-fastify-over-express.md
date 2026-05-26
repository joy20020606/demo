# ADR 001: Use Fastify over Express for the API Gateway

| Field | Value |
|-------|-------|
| Status | Accepted |
| Date | 2026-05-13 |
| Author | Joy Chuang |
| Supersedes | — |

## Context

Mini MaaS Ops 需要選擇一個 Node.js 後端框架做為 `api-gateway` 的基礎。候選清單：

1. **Express** — 業界最老牌、生態最大
2. **Fastify** — 較新的高效能框架
3. **NestJS** — 企業級、重量級
4. **Hono** — 新興、輕量、edge-ready

評估的維度：效能、TypeScript 友善度、學習曲線、文件自動化能力、社群成熟度。

## Decision

選 **Fastify**。

## Rationale

### 1. 效能 Performance
- Fastify 官方 benchmark 顯示比 Express 快約 2 倍（requests/sec）
- 對於未來要承載 MaaS 平台規模的請求量（百萬使用者）是有意義的差距

### 2. TypeScript 原生友善
- Fastify 核心 API 自帶完整 type definition
- Express 需額外裝 `@types/express`，且 type 完整度較差

### 3. 內建 Schema 驗證
- Fastify 內建 JSON Schema validation（基於 ajv）
- 結合 `@fastify/swagger` 可**自動產生 OpenAPI 文件**，直接對應 Metropia JD 對 Swagger 的要求
- Express 沒有內建，需自己組合 `joi` / `zod` + middleware

### 4. 比 NestJS 輕量
- NestJS 雖然功能完整，但學習曲線陡、樣板碼多
- 對 7 天開發週期不利
- Fastify 簡潔，與 ASP.NET Core Minimal API 風格近似

### 5. 比 Hono 穩定
- Hono 更新，社群與 plugin 生態尚未成熟
- Fastify 已被 Atlassian、Walmart、Microsoft 等用於生產

## Consequences

### 正面 Positive
- 高效能 baseline
- TypeScript 開發體驗好
- API 文件自動化（Day 4 接 `@fastify/swagger`）
- 與 ASP.NET Core 體驗類似，C# 出身工程師上手快

### 負面 Negative
- 第三方 middleware 生態比 Express 小
  - 緩解：核心需求 Fastify 官方都有 plugin（`@fastify/jwt`、`@fastify/cors`、`@fastify/sensible` 等）
- 團隊新人可能對 Fastify 不熟
  - 緩解：與 Express 模型接近，幾小時可上手

## Alternatives Considered

| 框架 | 優點 | 不選的理由 |
|------|------|-----------|
| Express | 生態最大、隨手可得的範例 | 效能低、TS 體驗差、文件需手寫 |
| NestJS | 企業級結構、DI、Swagger 整合 | 重量級、學習曲線陡、7 天 demo 不划算 |
| Hono | 超輕量、Edge runtime ready | 生態太新、生產案例少 |

## References

- [Fastify benchmarks](https://www.fastify.io/benchmarks/)
- [Fastify vs Express comparison](https://blog.logrocket.com/fastify-vs-express/)
