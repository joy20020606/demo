'use client'

import { useEffect, useState } from 'react'
import { useRouter } from 'next/navigation'
import { useSession } from 'next-auth/react'
import { Badge } from '@/components/ui/badge'

type OrderItem = {
  id: string
  productId: string
  quantity: number
  price: number
  product: { name: string }
}

type Order = {
  id: string
  status: string
  totalPrice: number
  createdAt: string
  items: OrderItem[]
}

const statusMap: Record<string, string> = {
  PENDING: '待處理',
  PROCESSING: '處理中',
  SHIPPED: '已出貨',
  DELIVERED: '已送達',
  CANCELLED: '已取消',
}

const statusVariant: Record<string, 'default' | 'secondary' | 'destructive' | 'outline'> = {
  PENDING: 'outline',
  PROCESSING: 'secondary',
  SHIPPED: 'default',
  DELIVERED: 'default',
  CANCELLED: 'destructive',
}

export default function AccountPage() {
  const { data: session, status } = useSession()
  const router = useRouter()
  const [orders, setOrders] = useState<Order[]>([])

  useEffect(() => {
    if (status === 'unauthenticated') {
      router.push('/login')
    }
  }, [status, router])

  useEffect(() => {
    if (status === 'authenticated') {
      fetch('/api/orders')
        .then((r) => r.json())
        .then((data) => setOrders(data.orders ?? data))
        .catch(() => {})
    }
  }, [status])

  if (status === 'loading') return <div className="py-16 text-center text-gray-500">載入中...</div>

  return (
    <div className="mx-auto max-w-2xl">
      <h1 className="mb-2 text-2xl font-bold text-gray-800">我的帳號</h1>
      <div className="mb-6 rounded-xl bg-white border p-4">
        <p className="font-semibold text-gray-800">{session?.user?.name}</p>
        <p className="text-sm text-gray-500">{session?.user?.email}</p>
      </div>

      <h2 className="mb-4 text-lg font-semibold text-gray-800">我的訂單</h2>
      {orders.length === 0 ? (
        <p className="text-gray-500">尚無訂單記錄</p>
      ) : (
        <div className="flex flex-col gap-4">
          {orders.map((order) => (
            <div key={order.id} className="rounded-xl border p-4">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-xs text-gray-500">
                    {new Date(order.createdAt).toLocaleDateString('zh-TW')}
                  </p>
                  <p className="font-semibold text-amber-600">NT$ {order.totalPrice}</p>
                </div>
                <Badge variant={statusVariant[order.status] ?? 'outline'}>
                  {statusMap[order.status] ?? order.status}
                </Badge>
              </div>
              <div className="mt-2 text-sm text-gray-600">
                {order.items.map((item) => (
                  <span key={item.id}>
                    {item.product?.name} x{item.quantity}{' '}
                  </span>
                ))}
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  )
}
