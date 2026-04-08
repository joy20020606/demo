import { SessionProvider } from 'next-auth/react'
import Navbar from '@/components/frontend/Navbar'
import { Toaster } from '@/components/ui/sonner'

export default function FrontendLayout({ children }: { children: React.ReactNode }) {
  return (
    <SessionProvider>
      <Navbar />
      <main className="mx-auto max-w-6xl px-4 py-8">{children}</main>
      <Toaster position="bottom-right" richColors />
    </SessionProvider>
  )
}
