'use client'

import Link from 'next/link'
import { useSession, signOut } from 'next-auth/react'
import { useCartStore } from '@/store/cartStore'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'

export default function Navbar() {
  const { data: session } = useSession()
  const getTotalItems = useCartStore((s) => s.getTotalItems)
  const totalItems = getTotalItems()

  return (
    <nav className="sticky top-0 z-50 bg-white border-b shadow-sm px-4 py-3">
      <div className="mx-auto flex max-w-6xl items-center justify-between">
        <Link href="/" className="text-amber-700 font-bold text-xl">
          豆干專賣店
        </Link>
        <div className="flex items-center gap-6">
          <Link href="/" className="text-sm text-gray-600 hover:text-amber-700">
            首頁
          </Link>
          <Link href="/products" className="text-sm text-gray-600 hover:text-amber-700">
            商品
          </Link>
          <Link href="/cart" className="relative text-sm text-gray-600 hover:text-amber-700">
            購物車
            {totalItems > 0 && (
              <Badge className="absolute -top-2 -right-4 h-4 min-w-4 px-1 text-[10px]">
                {totalItems}
              </Badge>
            )}
          </Link>
        </div>
        <div className="flex items-center gap-3">
          {session?.user ? (
            <>
              <span className="text-sm text-gray-600">{session.user.name}</span>
              <Button variant="outline" size="sm" onClick={() => signOut({ callbackUrl: '/' })}>
                登出
              </Button>
            </>
          ) : (
            <>
              <Link href="/login">
                <Button variant="ghost" size="sm" className="text-gray-600 hover:text-amber-700">登入</Button>
              </Link>
              <Link href="/register">
                <Button size="sm" className="bg-amber-600 hover:bg-amber-700 text-white">註冊</Button>
              </Link>
            </>
          )}
        </div>
      </div>
    </nav>
  )
}
