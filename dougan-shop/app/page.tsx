import { SessionProvider } from 'next-auth/react'
import Navbar from '@/components/frontend/Navbar'
import HomePage from '@/app/(frontend)/page'
import { Toaster } from '@/components/ui/sonner'

export default function RootPage() {
  return (
    <SessionProvider>
      <Navbar />
      <main className="mx-auto max-w-6xl px-4 py-8">
        <HomePage />
      </main>
      <Toaster position="bottom-right" richColors />
    </SessionProvider>
  )
}
