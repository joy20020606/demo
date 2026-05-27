import Link from 'next/link';

export default function HomePage() {
  return (
    <div className="space-y-8">
      <section>
        <h1 className="text-3xl font-bold text-gray-900">Mini SME CRM</h1>
        <p className="mt-2 text-gray-600">
          AI-powered CRM for Taiwanese SMEs — Next.js + Fastify + Prisma + Claude
        </p>
      </section>

      <section className="grid gap-4 sm:grid-cols-2">
        <Link
          href="/customers"
          className="rounded-lg border border-gray-200 bg-white p-5 transition hover:border-blue-300 hover:shadow"
        >
          <h2 className="text-lg font-semibold text-gray-900">客戶管理</h2>
          <p className="mt-1 text-sm text-gray-600">
            建立、查詢、編輯客戶資料
          </p>
          <p className="mt-3 text-sm font-medium text-blue-600">前往 →</p>
        </Link>

        <Link
          href="/deals"
          className="rounded-lg border border-gray-200 bg-white p-5 transition hover:border-blue-300 hover:shadow"
        >
          <h2 className="text-lg font-semibold text-gray-900">商機 Kanban</h2>
          <p className="mt-1 text-sm text-gray-600">
            拖拉卡片管理銷售階段（LEAD → WON）
          </p>
          <p className="mt-3 text-sm font-medium text-blue-600">前往 →</p>
        </Link>
      </section>

      <section className="rounded-lg border border-gray-200 bg-white p-5">
        <h2 className="text-base font-semibold text-gray-900">系統狀態</h2>
        <ul className="mt-3 space-y-1 text-sm text-gray-700">
          <li>
            🌐 <strong>Web</strong> — Next.js 14 on Vercel
          </li>
          <li>
            ⚡ <strong>API</strong> — Fastify on Railway ·{' '}
            <a
              href={`${process.env.NEXT_PUBLIC_API_URL ?? 'http://localhost:3001'}/docs`}
              target="_blank"
              rel="noreferrer"
              className="text-blue-600 hover:underline"
            >
              Swagger UI
            </a>
          </li>
          <li>
            💾 <strong>DB</strong> — PostgreSQL 16 (Prisma)
          </li>
          <li>
            🤖 <strong>AI</strong> — Claude API (Phase 5+)
          </li>
        </ul>
      </section>
    </div>
  );
}
