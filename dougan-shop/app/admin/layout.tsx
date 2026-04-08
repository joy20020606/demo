import Link from 'next/link'
import { auth } from '@/lib/auth'
import { redirect } from 'next/navigation'

export default async function AdminLayout({ children }: { children: React.ReactNode }) {
  const session = await auth()
  if (!session || session.user?.role !== 'ADMIN') redirect('/login')

  return (
    <div className="flex min-h-screen">
      <aside className="w-56 bg-gray-900 text-white flex flex-col">
        <div className="p-4 text-lg font-bold border-b border-gray-700">後台管理</div>
        <nav className="flex flex-col gap-1 p-2 flex-1">
          <Link href="/admin" className="px-3 py-2 rounded hover:bg-gray-700">Dashboard</Link>
          <Link href="/admin/products" className="px-3 py-2 rounded hover:bg-gray-700">商品管理</Link>
          <Link href="/admin/orders" className="px-3 py-2 rounded hover:bg-gray-700">訂單管理</Link>
          <Link href="/admin/members" className="px-3 py-2 rounded hover:bg-gray-700">會員管理</Link>
        </nav>
        <div className="p-4 border-t border-gray-700 text-sm text-gray-400">
          {session.user?.name || session.user?.email}
        </div>
      </aside>
      <main className="flex-1 p-8 bg-gray-50">{children}</main>
    </div>
  )
}
