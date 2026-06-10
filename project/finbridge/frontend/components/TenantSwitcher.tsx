"use client";

import type { TenantSlug } from "@/lib/api";
import { useTenant } from "./TenantProvider";

const tenants: TenantSlug[] = ["acme", "globex"];

export default function TenantSwitcher() {
  const { tenant, setTenant } = useTenant();

  return (
    <div className="flex items-center gap-1 rounded-md border bg-white p-0.5 text-sm">
      {tenants.map((t) => (
        <button
          key={t}
          type="button"
          onClick={() => setTenant(t)}
          className={
            t === tenant
              ? "rounded bg-accent px-3 py-1 font-medium text-white"
              : "rounded px-3 py-1 text-slate-600 hover:text-accent"
          }
        >
          {t}
        </button>
      ))}
    </div>
  );
}
