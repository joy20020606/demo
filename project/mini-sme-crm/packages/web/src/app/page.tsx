export default function HomePage() {
  return (
    <main className="min-h-screen p-8">
      <h1 className="text-3xl font-bold">Mini SME CRM</h1>
      <p className="mt-2 text-gray-600">
        AI-powered CRM for SMEs — Day 0 skeleton up and running.
      </p>
      <ul className="mt-4 list-disc pl-6 text-sm text-gray-700">
        <li>Web (this page) — Next.js on Vercel</li>
        <li>API — Fastify on Railway → /health</li>
        <li>DB — PostgreSQL (Prisma)</li>
        <li>AI — Claude API (summarize / score)</li>
      </ul>
    </main>
  );
}
