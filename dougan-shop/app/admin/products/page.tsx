'use client'

import { useEffect, useState } from 'react'
import Link from 'next/link'
import { Button } from '@/components/ui/button'

type Product = {
  id: string
  name: string
  price: number
  stock: number
  isActive: boolean
  createdAt: string
}

export default function AdminProducts() {
  const [products, setProducts] = useState<Product[]>([])
  const [loading, setLoading] = useState(true)

  const fetchProducts = async () => {
    const res = await fetch('/api/products/admin')
    const data = await res.json()
    setProducts(data)
    setLoading(false)
  }

  useEffect(() => { fetchProducts() }, [])

  const toggleActive = async (id: string, isActive: boolean) => {
    await fetch(`/api/products/${id}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ isActive: !isActive }),
    })
    fetchProducts()
  }

  if (loading) return <div>載入中...</div>

  return (
    <div>
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-2xl font-bold">商品管理</h1>
        <Link href="/admin/products/new">
          <Button>+ 新增商品</Button>
        </Link>
      </div>
      <div className="bg-white rounded-lg shadow-sm overflow-hidden">
        <table className="w-full text-sm">
          <thead className="bg-gray-50">
            <tr>
              <th className="text-left px-4 py-3">商品名稱</th>
              <th className="text-left px-4 py-3">價格</th>
              <th className="text-left px-4 py-3">庫存</th>
              <th className="text-left px-4 py-3">狀態</th>
              <th className="text-left px-4 py-3">操作</th>
            </tr>
          </thead>
          <tbody>
            {products.map((p) => (
              <tr key={p.id} className="border-t hover:bg-gray-50">
                <td className="px-4 py-3">{p.name}</td>
                <td className="px-4 py-3">NT${p.price}</td>
                <td className="px-4 py-3">{p.stock}</td>
                <td className="px-4 py-3">
                  <span className={`px-2 py-0.5 rounded text-xs ${p.isActive ? 'bg-green-100 text-green-700' : 'bg-gray-100 text-gray-500'}`}>
                    {p.isActive ? '上架' : '下架'}
                  </span>
                </td>
                <td className="px-4 py-3 flex gap-2">
                  <Link href={`/admin/products/${p.id}/edit`}>
                    <Button variant="outline" size="sm">編輯</Button>
                  </Link>
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() => toggleActive(p.id, p.isActive)}
                  >
                    {p.isActive ? '下架' : '上架'}
                  </Button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  )
}
