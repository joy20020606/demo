'use client'

import { useEffect, useState } from 'react'
import { useRouter } from 'next/navigation'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'

type Product = { id: string; name: string; description: string | null; price: number; stock: number; imageUrl: string | null; isActive: boolean }

export default function EditProduct({ params }: { params: Promise<{ id: string }> }) {
  const router = useRouter()
  const [form, setForm] = useState({ name: '', description: '', price: '', stock: '', imageUrl: '', isActive: true })
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)
  const [id, setId] = useState('')

  useEffect(() => {
    params.then(async ({ id }) => {
      setId(id)
      const res = await fetch(`/api/products/${id}`)
      const p: Product = await res.json()
      setForm({
        name: p.name,
        description: p.description ?? '',
        price: String(p.price),
        stock: String(p.stock),
        imageUrl: p.imageUrl ?? '',
        isActive: p.isActive,
      })
    })
  }, [params])

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setLoading(true)
    setError('')

    const res = await fetch(`/api/products/${id}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ ...form, price: parseFloat(form.price), stock: parseInt(form.stock) }),
    })

    if (res.ok) {
      router.push('/admin/products')
    } else {
      const data = await res.json()
      setError(data.error || '更新失敗')
      setLoading(false)
    }
  }

  return (
    <div className="max-w-lg">
      <h1 className="text-2xl font-bold mb-6">編輯商品</h1>
      <form onSubmit={handleSubmit} className="bg-white rounded-lg shadow-sm p-6 space-y-4">
        <div>
          <Label htmlFor="name">商品名稱 *</Label>
          <Input id="name" value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} required />
        </div>
        <div>
          <Label htmlFor="description">描述</Label>
          <textarea
            id="description"
            value={form.description}
            onChange={(e) => setForm({ ...form, description: e.target.value })}
            className="w-full border rounded-md px-3 py-2 text-sm h-24 resize-none focus:outline-none focus:ring-2 focus:ring-ring"
          />
        </div>
        <div className="grid grid-cols-2 gap-4">
          <div>
            <Label htmlFor="price">價格 (NT$) *</Label>
            <Input id="price" type="number" min="0" value={form.price} onChange={(e) => setForm({ ...form, price: e.target.value })} required />
          </div>
          <div>
            <Label htmlFor="stock">庫存數量 *</Label>
            <Input id="stock" type="number" min="0" value={form.stock} onChange={(e) => setForm({ ...form, stock: e.target.value })} required />
          </div>
        </div>
        <div>
          <Label htmlFor="imageUrl">圖片 URL</Label>
          <Input id="imageUrl" value={form.imageUrl} onChange={(e) => setForm({ ...form, imageUrl: e.target.value })} placeholder="https://..." />
        </div>
        <div className="flex items-center gap-2">
          <input type="checkbox" id="isActive" checked={form.isActive} onChange={(e) => setForm({ ...form, isActive: e.target.checked })} />
          <Label htmlFor="isActive">上架中</Label>
        </div>
        {error && <p className="text-red-500 text-sm">{error}</p>}
        <div className="flex gap-3">
          <Button type="submit" disabled={loading}>{loading ? '更新中...' : '儲存變更'}</Button>
          <Button type="button" variant="outline" onClick={() => router.back()}>取消</Button>
        </div>
      </form>
    </div>
  )
}
