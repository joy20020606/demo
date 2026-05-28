import Link from 'next/link';

export default function HomePage() {
  const apiUrl = process.env.NEXT_PUBLIC_API_URL ?? 'http://localhost:3001';

  return (
    <div className="space-y-10">
      {/* Hero */}
      <section className="rounded-2xl bg-gradient-to-br from-blue-50 to-indigo-100 p-8 sm:p-12">
        <div className="inline-flex items-center gap-2 rounded-full bg-white px-3 py-1 text-xs font-medium text-blue-700 shadow-sm">
          <span className="h-2 w-2 animate-pulse rounded-full bg-green-500" />
          Live Demo · Production
        </div>
        <h1 className="mt-4 text-4xl font-bold tracking-tight text-gray-900 sm:text-5xl">
          Mini SME CRM
        </h1>
        <p className="mt-3 max-w-2xl text-lg text-gray-700">
          為中小企業打造的 AI 強化型 CRM —— 把散落在 Excel、LINE、Email 的客戶互動，
          統一進系統並轉成可行動的洞察。
        </p>
        <div className="mt-6 flex flex-wrap gap-3">
          <Link
            href="/deals"
            className="rounded-md bg-blue-600 px-5 py-2.5 text-sm font-semibold text-white shadow-sm transition hover:bg-blue-700"
          >
            試試 Kanban →
          </Link>
          <Link
            href="/customers"
            className="rounded-md border border-gray-300 bg-white px-5 py-2.5 text-sm font-semibold text-gray-700 shadow-sm transition hover:bg-gray-50"
          >
            管理客戶
          </Link>
          <a
            href={`${apiUrl}/docs`}
            target="_blank"
            rel="noreferrer"
            className="rounded-md border border-gray-300 bg-white px-5 py-2.5 text-sm font-semibold text-gray-700 shadow-sm transition hover:bg-gray-50"
          >
            API 文件 ↗
          </a>
        </div>
      </section>

      {/* 試用引導 */}
      <section>
        <h2 className="text-xl font-bold text-gray-900">90 秒看完核心功能</h2>
        <p className="mt-1 text-sm text-gray-600">建議照這個順序試一遍</p>
        <ol className="mt-4 space-y-3">
          {[
            { step: 1, title: '建一個客戶', desc: '在「客戶」頁新增一筆，看搜尋與列表' },
            { step: 2, title: '建一個商機', desc: '在「商機」頁選客戶 + 標題 + 金額' },
            { step: 3, title: '拖卡片', desc: '從 LEAD 拖到 PROPOSAL，UI 瞬間反應（optimistic update）' },
            { step: 4, title: '拖到「成交 🎉」', desc: '卡片變灰、出現 🔒 已結案 —— 終態保護生效' },
            { step: 5, title: '試著再拖 WON 卡片', desc: '拖不動（前端 disabled）；用 API 直打會回 409 Conflict' },
          ].map((item) => (
            <li
              key={item.step}
              className="flex gap-4 rounded-lg border border-gray-200 bg-white p-4"
            >
              <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-blue-100 text-sm font-bold text-blue-700">
                {item.step}
              </div>
              <div>
                <p className="font-semibold text-gray-900">{item.title}</p>
                <p className="text-sm text-gray-600">{item.desc}</p>
              </div>
            </li>
          ))}
        </ol>
      </section>

      {/* 技術亮點 */}
      <section>
        <h2 className="text-xl font-bold text-gray-900">技術亮點</h2>
        <div className="mt-4 grid gap-3 sm:grid-cols-2">
          {[
            {
              title: '🏗️ 分層架構 + DI',
              desc: 'Route → Service → Data，service 用 constructor 注入 Prisma，方便 mock / transaction reuse',
            },
            {
              title: '🔒 狀態機 + 409 Conflict',
              desc: 'Deal stage 轉移專屬 endpoint，一般 PATCH 編譯期擋掉 stage 欄位（defense in depth）',
            },
            {
              title: '⚡ Optimistic UI + Rollback',
              desc: '拖卡片 0ms 反應，背景打 API，失敗自動 rollback（React Query mutation）',
            },
            {
              title: '🧩 Monorepo Type Sharing',
              desc: 'Zod schema 寫一次，前後端共用：runtime 驗證 + TS 型別 + Swagger 文件三合一',
            },
            {
              title: '🛡️ Defense in Depth',
              desc: '前端 Zod 預驗 → 後端 fastify-type-provider-zod 再驗 → DB constraint 兜底',
            },
            {
              title: '📦 自動部署 CI/CD',
              desc: 'git push → Railway 自動 build + migrate + deploy；Vercel 同步部署前端',
            },
          ].map((item) => (
            <div
              key={item.title}
              className="rounded-lg border border-gray-200 bg-white p-4"
            >
              <h3 className="font-semibold text-gray-900">{item.title}</h3>
              <p className="mt-1 text-sm text-gray-600">{item.desc}</p>
            </div>
          ))}
        </div>
      </section>

      {/* 系統狀態 */}
      <section className="rounded-lg border border-gray-200 bg-white p-5">
        <h2 className="text-base font-semibold text-gray-900">系統狀態</h2>
        <ul className="mt-3 space-y-2 text-sm text-gray-700">
          <li className="flex items-center gap-2">
            <span className="inline-block h-2 w-2 rounded-full bg-green-500" />
            <strong>Web</strong> — Next.js 14 on Vercel
          </li>
          <li className="flex items-center gap-2">
            <span className="inline-block h-2 w-2 rounded-full bg-green-500" />
            <strong>API</strong> — Fastify on Railway ·{' '}
            <a
              href={`${apiUrl}/docs`}
              target="_blank"
              rel="noreferrer"
              className="text-blue-600 hover:underline"
            >
              Swagger UI ↗
            </a>
          </li>
          <li className="flex items-center gap-2">
            <span className="inline-block h-2 w-2 rounded-full bg-green-500" />
            <strong>DB</strong> — PostgreSQL 16 (Prisma)
          </li>
          <li className="flex items-center gap-2">
            <span className="inline-block h-2 w-2 rounded-full bg-gray-300" />
            <strong>AI</strong> — Claude API <span className="text-xs text-gray-500">(Phase 5+ 整合中)</span>
          </li>
        </ul>
      </section>
    </div>
  );
}
