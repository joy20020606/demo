"use client";

import {
  createContext,
  useContext,
  useEffect,
  useState,
  type ReactNode,
} from "react";

import type { TenantSlug } from "@/lib/api";

const STORAGE_KEY = "finbridge.tenant";

interface TenantContextValue {
  tenant: TenantSlug;
  setTenant: (t: TenantSlug) => void;
}

const TenantContext = createContext<TenantContextValue | null>(null);

export function TenantProvider({ children }: { children: ReactNode }) {
  const [tenant, setTenantState] = useState<TenantSlug>("acme");

  useEffect(() => {
    const stored = window.localStorage.getItem(STORAGE_KEY);
    if (stored === "acme" || stored === "globex") {
      setTenantState(stored);
    }
  }, []);

  function setTenant(t: TenantSlug) {
    setTenantState(t);
    window.localStorage.setItem(STORAGE_KEY, t);
  }

  return (
    <TenantContext.Provider value={{ tenant, setTenant }}>
      {children}
    </TenantContext.Provider>
  );
}

export function useTenant(): TenantContextValue {
  const ctx = useContext(TenantContext);
  if (ctx === null) {
    throw new Error("useTenant must be used within a TenantProvider");
  }
  return ctx;
}
