'use client'

import { useState, useEffect } from 'react'
import { useRouter } from 'next/navigation'
import { useSession } from 'next-auth/react'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { useCartStore } from '@/store/cartStore'

export default function CheckoutPage() {
  const { data: session, status } = useSession()
  const router = useRouter()
  const items = useCartStore((s) => s.items)
  const getTotalPrice = useCartStore((s) => s.getTotalPrice)
  const clearCart = useCartStore((s) => s.clearCart)
  const [name, setName] = useState('')
  const [address, setAddress] = useState('')
  const [phone, setPhone] = useState('')
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')

  useEffect(() => {
    if (status === 'unauthenticated') {
      router.push('/login')
    }
  }, [status, router])

  if (status === 'loading') return <div className="py-16 text-center text-gray-500">載入中...</div>

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setLoading(true)
    setError('')
    try {
      const res = await fetch('/api/orders', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ name, address, phone, items }),
      })
      if (!res.ok) {
        const data = await res.json()
        setError(data.error ?? '結帳失敗')
        return
      }
      clearCart()
      router.push('/account')
    } catch {
      setError('網路錯誤，請稍後再試')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="mx-auto max-w-2xl">
      <h1 className="mb-6 text-2xl font-bold text-gray-800">結帳</h1>
      <div className="mb-6 rounded-xl border p-4">
        <h2 className="mb-3 font-semibold text-gray-800">訂單摘要</h2>
        {items.map((item) => (
          <div key={item.productId} className="flex justify-between py-1 text-sm text-gray-600">
            <span>{item.name} x {item.quantity}</span>
            <span>NT$ {item.price * item.quantity}</span>
          </div>
        ))}
        <div className="mt-3 flex justify-between border-t pt-3 font-bold text-gray-800">
          <span>總計</span>
          <span className="text-amber-600">NT$ {getTotalPrice()}</span>
        </div>
      </div>
      <form onSubmit={handleSubmit} className="flex flex-col gap-4">
        <div>
          <Label htmlFor="name">姓名</Label>
          <Input
            id="name"
            value={name}
            onChange={(e) => setName(e.target.value)}
            required
            className="mt-1"
          />
        </div>
        <div>
          <Label htmlFor="address">地址</Label>
          <Input
            id="address"
            value={address}
            onChange={(e) => setAddress(e.target.value)}
            required
            className="mt-1"
          />
        </div>
        <div>
          <Label htmlFor="phone">電話</Label>
          <Input
            id="phone"
            value={phone}
            onChange={(e) => setPhone(e.target.value)}
            required
            className="mt-1"
          />
        </div>
        {error && <p className="text-sm text-red-600">{error}</p>}
        <Button type="submit" size="lg" disabled={loading}>
          {loading ? '處理中...' : '確認訂購'}
        </Button>
      </form>
    </div>
  )
}
