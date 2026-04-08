'use client'

import { useEffect, useState } from 'react'
import { Card, CardContent, CardFooter } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { useCartStore } from '@/store/cartStore'
import { toast } from 'sonner'

type Product = {
  id: string
  name: string
  price: number
  imageUrl: string | null
  description: string | null
  stock: number
}

export default function HomePage() {
  const [products, setProducts] = useState<Product[]>([])
  const addItem = useCartStore((s) => s.addItem)

  useEffect(() => {
    fetch('/api/products?featured=true&limit=6')
      .then((r) => r.json())
      .then((data) => setProducts(data.products ?? data))
      .catch(() => {})
  }, [])

  return (
    <div>
      <section className="bg-amber-50 rounded-2xl px-8 py-16 text-center mb-12">
        <h1 className="mb-4 text-4xl font-bold text-amber-800">豆干專賣店</h1>
        <p className="mb-6 text-lg text-amber-700">嚴選優質豆干，傳統工藝，美味健康</p>
        <Button size="lg" className="bg-amber-600 hover:bg-amber-700 text-white">
          立即選購
        </Button>
      </section>

      <section>
        <h2 className="mb-6 text-2xl font-semibold text-gray-800">精選商品</h2>
        <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
          {products.map((product) => (
            <Card key={product.id} className="overflow-hidden">
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
              </CardContent>
              <CardFooter>
                <Button
                  className="w-full"
                  onClick={() => {
                    addItem({
                      productId: product.id,
                      name: product.name,
                      price: product.price,
                      quantity: 1,
                      imageUrl: product.imageUrl ?? undefined,
                    })
                    toast.success(`已加入購物車`, { description: product.name })
                  }}
                >
                  加入購物車
                </Button>
              </CardFooter>
            </Card>
          ))}
        </div>
      </section>
    </div>
  )
}
