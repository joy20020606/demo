'use client'

import { useEffect, useState } from 'react'
import { Card, CardContent, CardFooter } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Badge } from '@/components/ui/badge'
import { useCartStore } from '@/store/cartStore'
import { toast } from 'sonner'

type Product = {
  id: string
  name: string
  price: number
  stock: number
  imageUrl: string | null
  description: string | null
}

export default function ProductsPage() {
  const [products, setProducts] = useState<Product[]>([])
  const [search, setSearch] = useState('')
  const addItem = useCartStore((s) => s.addItem)

  useEffect(() => {
    fetch('/api/products')
      .then((r) => r.json())
      .then((data) => setProducts(data.products ?? data))
      .catch(() => {})
  }, [])

  const filtered = products.filter((p) =>
    p.name.toLowerCase().includes(search.toLowerCase())
  )

  return (
    <div>
      <div className="mb-6 flex items-center justify-between">
        <h1 className="text-2xl font-bold text-gray-800">全部商品</h1>
        <Input
          placeholder="搜尋商品..."
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          className="w-64"
        />
      </div>
      <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
        {filtered.map((product) => (
          <Card key={product.id}>
            {product.imageUrl ? (
              <img src={product.imageUrl} alt={product.name} className="h-48 w-full object-cover" />
            ) : (
              <div className="h-48 w-full bg-gray-200 flex items-center justify-center text-gray-500">
                無圖片
              </div>
            )}
            <CardContent className="pt-4">
              <h3 className="font-semibold text-gray-800">{product.name}</h3>
              <p className="mt-1 text-amber-600 font-bold">NT$ {product.price}</p>
              <div className="mt-2">
                {product.stock > 0 ? (
                  <Badge variant="outline">庫存: {product.stock}</Badge>
                ) : (
                  <Badge variant="destructive">缺貨</Badge>
                )}
              </div>
            </CardContent>
            <CardFooter>
              <Button
                className="w-full"
                disabled={product.stock === 0}
                onClick={() => {
                  addItem({
                    productId: product.id,
                    name: product.name,
                    price: product.price,
                    quantity: 1,
                    imageUrl: product.imageUrl ?? undefined,
                  })
                  toast.success('已加入購物車', { description: product.name })
                }}
              >
                加入購物車
              </Button>
            </CardFooter>
          </Card>
        ))}
      </div>
    </div>
  )
}
