'use client'

import Link from 'next/link'
import { Button } from '@/components/ui/button'
import { useCartStore } from '@/store/cartStore'

export default function CartPage() {
  const items = useCartStore((s) => s.items)
  const removeItem = useCartStore((s) => s.removeItem)
  const updateQuantity = useCartStore((s) => s.updateQuantity)
  const getTotalPrice = useCartStore((s) => s.getTotalPrice)

  if (items.length === 0) {
    return (
      <div className="py-16 text-center">
        <p className="text-lg text-gray-500">購物車是空的</p>
        <Link href="/products">
          <Button className="mt-4">去逛逛</Button>
        </Link>
      </div>
    )
  }

  return (
    <div className="mx-auto max-w-2xl">
      <h1 className="mb-6 text-2xl font-bold text-gray-800">購物車</h1>
      <div className="flex flex-col gap-4">
        {items.map((item) => (
          <div key={item.productId} className="flex items-center gap-4 rounded-xl border p-4">
            {item.imageUrl ? (
              <img src={item.imageUrl} alt={item.name} className="h-16 w-16 rounded-lg object-cover" />
            ) : (
              <div className="h-16 w-16 rounded-lg bg-gray-200" />
            )}
            <div className="flex-1">
              <p className="font-semibold text-gray-800">{item.name}</p>
              <p className="text-amber-600">NT$ {item.price}</p>
            </div>
            <div className="flex items-center gap-2">
              <Button
                variant="outline"
                size="icon"
                onClick={() => updateQuantity(item.productId, item.quantity - 1)}
              >
                -
              </Button>
              <span className="w-6 text-center">{item.quantity}</span>
              <Button
                variant="outline"
                size="icon"
                onClick={() => updateQuantity(item.productId, item.quantity + 1)}
              >
                +
              </Button>
            </div>
            <p className="w-20 text-right font-semibold text-gray-800">NT$ {item.price * item.quantity}</p>
            <Button variant="destructive" size="sm" onClick={() => removeItem(item.productId)}>
              移除
            </Button>
          </div>
        ))}
      </div>
      <div className="mt-6 flex items-center justify-between rounded-xl bg-gray-50 border p-4">
        <p className="text-lg font-bold text-gray-800">總計</p>
        <p className="text-xl font-bold text-amber-600">NT$ {getTotalPrice()}</p>
      </div>
      <div className="mt-4 flex justify-end">
        <Link href="/checkout">
          <Button size="lg">前往結帳</Button>
        </Link>
      </div>
    </div>
  )
}
