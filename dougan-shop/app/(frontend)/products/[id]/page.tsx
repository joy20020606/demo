'use client'

import { useEffect, useState } from 'react'
import { useParams } from 'next/navigation'
import { Button } from '@/components/ui/button'
import { useCartStore } from '@/store/cartStore'
import { toast } from 'sonner'

type Product = {
  id: string
  name: string
  description: string | null
  price: number
  stock: number
  imageUrl: string | null
}

export default function ProductDetailPage() {
  const { id } = useParams<{ id: string }>()
  const [product, setProduct] = useState<Product | null>(null)
  const [quantity, setQuantity] = useState(1)
  const addItem = useCartStore((s) => s.addItem)

  useEffect(() => {
    fetch(`/api/products/${id}`)
      .then((r) => r.json())
      .then((data) => setProduct(data))
      .catch(() => {})
  }, [id])

  if (!product) {
    return <div className="text-center py-16 text-gray-500">載入中...</div>
  }

  return (
    <div className="mx-auto max-w-3xl">
      <div className="grid grid-cols-1 gap-8 md:grid-cols-2">
        <div className="bg-gray-200 rounded-xl overflow-hidden">
          {product.imageUrl ? (
            <img src={product.imageUrl} alt={product.name} className="w-full rounded-xl object-cover" />
          ) : (
            <div className="h-72 w-full rounded-xl bg-gray-200 flex items-center justify-center text-gray-500">
              無圖片
            </div>
          )}
        </div>
        <div className="flex flex-col gap-4">
          <h1 className="text-2xl font-bold text-gray-800">{product.name}</h1>
          {product.description && (
            <p className="text-gray-600">{product.description}</p>
          )}
          <p className="text-amber-600 text-2xl font-bold">NT$ {product.price}</p>
          <p className="text-sm text-gray-500">庫存: {product.stock} 件</p>
          <div className="flex items-center gap-3">
            <Button
              variant="outline"
              size="icon"
              onClick={() => setQuantity((q) => Math.max(1, q - 1))}
            >
              -
            </Button>
            <span className="w-8 text-center font-semibold">{quantity}</span>
            <Button
              variant="outline"
              size="icon"
              onClick={() => setQuantity((q) => Math.min(product.stock, q + 1))}
            >
              +
            </Button>
          </div>
          <Button
            size="lg"
            disabled={product.stock === 0}
            onClick={() => {
              addItem({
                productId: product.id,
                name: product.name,
                price: product.price,
                quantity,
                imageUrl: product.imageUrl ?? undefined,
              })
              toast.success('已加入購物車', {
                description: `${product.name} × ${quantity}`,
              })
            }}
          >
            加入購物車
          </Button>
        </div>
      </div>
    </div>
  )
}
