'use client'

import { useEffect, useState } from 'react'
import { Button } from '@/components/ui/button'

type OrderItem = { id: string; quantity: number; price: number; product: { name: string } }
type Order = {
  id: string; status: string; totalPrice: number; address: string; phone: string
  createdAt: string; user: { name: string | null; email: string }; items: OrderItem[]
}

const STATUS_OPTIONS = ['PENDING', 'PROCESSING', 'SHIPPED', 'DELIVERED', 'CANCELLED']
const STATUS_LABELS: Record<string, string> = {
  PENDING: '待處理', PROCESSING: '處理中', SHIPPED: '已出貨', DELIVERED: '已送達', CANCELLED: '已取消'
}
const STATUS_COLORS: Record<string, string> = {
  PENDING: 'bg-yellow-100 text-yellow-700', PROCESSING: 'bg-blue-100 text-blue-700',
  SHIPPED: 'bg-purple-100 text-purple-700', DELIVERED: 'bg-green-100 text-green-700',
  CANCELLED: 'bg-gray-100 text-gray-500'
}

export default function AdminOrders() {
  const [orders, setOrders] = useState<Order[]>([])
  const [loading, setLoading] = useState(true)
  const [expanded, setExpanded] = useState<string | null>(null)

  const fetchOrders = async () => {
    const res = await fetch('/api/orders?all=true')
    setOrders(await res.json())
    setLoading(false)
  }

  useEffect(() => { fetchOrders() }, [])

  const updateStatus = async (id: string, status: string) => {
    await fetch(`/api/orders/${id}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ status }),
    })
    fetchOrders()
  }

  if (loading) return <div>載入中...</div>

  return (
    <div>
      <h1 className="text-2xl font-bold mb-6">訂單管理</h1>
      <div className="bg-white rounded-lg shadow-sm overflow-hidden">
        <table className="w-full text-sm">
          <thead className="bg-gray-50">
            <tr>
              <th className="text-left px-4 py-3">訂單</th>
              <th className="text-left px-4 py-3">會員</th>
              <th className="text-left px-4 py-3">金額</th>
              <th className="text-left px-4 py-3">狀態</th>
              <th className="text-left px-4 py-3">日期</th>
              <th className="text-left px-4 py-3">操作</th>
            </tr>
          </thead>
          <tbody>
            {orders.map((order) => (
              <>
                <tr key={order.id} className="border-t hover:bg-gray-50 cursor-pointer" onClick={() => setExpanded(expanded === order.id ? null : order.id)}>
                  <td className="px-4 py-3 font-mono text-xs">{order.id.slice(0, 8)}...</td>
                  <td className="px-4 py-3">{order.user.name || order.user.email}</td>
                  <td className="px-4 py-3">NT${order.totalPrice.toFixed(0)}</td>
                  <td className="px-4 py-3">
                    <span className={`px-2 py-0.5 rounded text-xs ${STATUS_COLORS[order.status]}`}>
                      {STATUS_LABELS[order.status]}
                    </span>
                  </td>
                  <td className="px-4 py-3 text-gray-500">{new Date(order.createdAt).toLocaleDateString('zh-TW')}</td>
                  <td className="px-4 py-3" onClick={(e) => e.stopPropagation()}>
                    <select
                      value={order.status}
                      onChange={(e) => updateStatus(order.id, e.target.value)}
                      className="text-xs border rounded px-2 py-1"
                    >
                      {STATUS_OPTIONS.map((s) => <option key={s} value={s}>{STATUS_LABELS[s]}</option>)}
                    </select>
                  </td>
                </tr>
                {expanded === order.id && (
                  <tr key={`${order.id}-detail`} className="bg-gray-50">
                    <td colSpan={6} className="px-4 py-3">
                      <div className="text-xs text-gray-600 space-y-1">
                        <p><strong>地址：</strong>{order.address}</p>
                        <p><strong>電話：</strong>{order.phone}</p>
                        <div className="mt-2">
                          {order.items.map((item) => (
                            <div key={item.id} className="flex justify-between">
                              <span>{item.product.name} x{item.quantity}</span>
                              <span>NT${(item.price * item.quantity).toFixed(0)}</span>
                            </div>
                          ))}
                        </div>
                      </div>
                    </td>
                  </tr>
                )}
              </>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  )
}
