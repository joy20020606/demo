import type { Metadata } from "next";
import Link from "next/link";
import "./globals.css";

import TenantSwitcher from "@/components/TenantSwitcher";
import { TenantProvider } from "@/components/TenantProvider";

export const metadata: Metadata = {
  title: "FinBridge",
  description: "Multi-tenant financial data integration gateway — demo",
};

const nav = [
  { href: "/", label: "Overview" },
  { href: "/connectors", label: "Connectors" },
  { href: "/reconciliation", label: "Reconciliation" },
  { href: "/dead-letter", label: "Dead Letter" },
];

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="en">
      <body>
        <TenantProvider>
          <header className="border-b bg-white">
            <nav className="mx-auto flex max-w-5xl items-center gap-6 px-6 py-4">
              <span className="font-semibold text-accent">FinBridge</span>
              <div className="flex gap-4 text-sm text-slate-600">
                {nav.map((n) => (
                  <Link key={n.href} href={n.href} className="hover:text-accent">
                    {n.label}
                  </Link>
                ))}
              </div>
              <div className="ml-auto">
                <TenantSwitcher />
              </div>
            </nav>
          </header>
          <main className="mx-auto max-w-5xl px-6 py-8">{children}</main>
        </TenantProvider>
      </body>
    </html>
  );
}
