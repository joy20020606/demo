import { NextRequest, NextResponse } from 'next/server'
import { prisma } from '@/lib/prisma'
import { auth } from '@/lib/auth'

export async function GET(req: NextRequest) {
  const { searchParams } = new URL(req.url)
  const limit = searchParams.get('limit') ? parseInt(searchParams.get('limit')!) : undefined

  const products = await prisma.product.findMany({
    where: { isActive: true },
    orderBy: { createdAt: 'desc' },
    take: limit,
  })

  return NextResponse.json(products)
}

export async function POST(req: NextRequest) {
  const session = await auth()
  if (!session || session.user?.role !== 'ADMIN') {
    return NextResponse.json({ error: '未授權' }, { status: 401 })
  }

  const { name, description, price, stock, imageUrl } = await req.json()

  if (!name || price === undefined || stock === undefined) {
    return NextResponse.json({ error: '請填寫必要欄位' }, { status: 400 })
  }

  const product = await prisma.product.create({
    data: { name, description, price: parseFloat(price), stock: parseInt(stock), imageUrl },
  })

  return NextResponse.json(product, { status: 201 })
}
