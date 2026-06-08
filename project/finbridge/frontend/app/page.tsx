const capabilities = [
  ["Multi-tenant isolation", "Per-tenant data scoping with a tenant registry"],
  ["Financial data ingestion", "Normalize feeds from heterogeneous upstream sources"],
  ["Numeric-safe modeling", "Money & quantity stored as Decimal (Numeric), never Float"],
  ["Resilient connectors", "tenacity retries + httpx for upstream calls"],
  ["Streaming-ready", "Optional Kafka publishing of normalized events"],
  ["Portable deploy", "Docker Compose locally, Railway + Vercel in production"],
];

export default function Home() {
  return (
    <div className="space-y-10">
      <section className="space-y-3">
        <h1 className="text-3xl font-bold">FinBridge</h1>
        <p className="text-slate-600">
          A multi-tenant financial data integration gateway: ingest, normalize,
          and serve financial feeds across tenants with numeric-safe modeling.
        </p>
      </section>

      <section className="grid gap-4 sm:grid-cols-2">
        {capabilities.map(([title, desc]) => (
          <div key={title} className="rounded-lg border bg-white p-4">
            <h2 className="font-semibold text-ink">{title}</h2>
            <p className="mt-1 text-sm text-slate-600">{desc}</p>
          </div>
        ))}
      </section>
    </div>
  );
}
