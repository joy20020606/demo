import type { Metadata } from "next";
import Link from "next/link";
import "./globals.css";

export const metadata: Metadata = {
  title: "知識庫問答 Agent",
  description: "RAG + LangGraph Agent with citations — demo",
};

const nav = [
  { href: "/", label: "首頁" },
  { href: "/upload", label: "上傳文件" },
  { href: "/chat", label: "問答" },
  { href: "/eval", label: "評估" },
];

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="zh-Hant">
      <body>
        <header className="border-b bg-white">
          <nav className="mx-auto flex max-w-5xl items-center gap-6 px-6 py-4">
            <span className="font-semibold text-accent">知識庫問答 Agent</span>
            <div className="flex gap-4 text-sm text-slate-600">
              {nav.map((n) => (
                <Link key={n.href} href={n.href} className="hover:text-accent">
                  {n.label}
                </Link>
              ))}
            </div>
          </nav>
        </header>
        <main className="mx-auto max-w-5xl px-6 py-8">{children}</main>
      </body>
    </html>
  );
}
