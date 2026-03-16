import { hooks, categoryColors } from "@/data/hooks";

export default function Home() {
  return (
    <div className="min-h-screen bg-zinc-50 font-sans dark:bg-zinc-950">
      <header className="border-b border-zinc-200 bg-white dark:border-zinc-800 dark:bg-zinc-900">
        <div className="mx-auto max-w-6xl px-6 py-8">
          <h1 className="text-3xl font-bold tracking-tight text-zinc-900 dark:text-zinc-50">
            HookHub
          </h1>
          <p className="mt-2 text-lg text-zinc-600 dark:text-zinc-400">
            Discover Open Source Cloud Hooks
          </p>
        </div>
      </header>

      <main className="mx-auto max-w-6xl px-6 py-10">
        <div className="grid grid-cols-1 gap-6 md:grid-cols-2 lg:grid-cols-3">
          {hooks.map((hook) => (
            <div
              key={hook.id}
              className="flex flex-col justify-between rounded-xl border border-zinc-200 bg-white p-6 shadow-sm transition-shadow hover:shadow-md dark:border-zinc-800 dark:bg-zinc-900"
            >
              <div>
                <div className="mb-3 flex items-start justify-between gap-2">
                  <h2 className="text-lg font-semibold text-zinc-900 dark:text-zinc-50">
                    {hook.name}
                  </h2>
                  <span
                    className={`shrink-0 rounded-full px-2.5 py-0.5 text-xs font-medium ${categoryColors[hook.category] ?? "bg-zinc-100 text-zinc-800"}`}
                  >
                    {hook.category}
                  </span>
                </div>
                <p className="line-clamp-2 text-sm leading-relaxed text-zinc-600 dark:text-zinc-400">
                  {hook.description}
                </p>
              </div>
              <a
                href={hook.repoUrl}
                target="_blank"
                rel="noopener noreferrer"
                className="mt-4 inline-flex items-center text-sm font-medium text-blue-600 hover:text-blue-800 dark:text-blue-400 dark:hover:text-blue-300"
              >
                View Repo &rarr;
              </a>
            </div>
          ))}
        </div>
      </main>
    </div>
  );
}
