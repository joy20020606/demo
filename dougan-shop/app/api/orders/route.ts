import { NextRequest, NextResponse } from 'next/server'
import { prisma } from '@/lib/prisma'
import { auth } from '@/lib/auth'
import { sendOrderConfirmationEmail, sendNewOrderNotificationEmail } from '@/lib/email'

export async function GET(req: NextRequest) {
  const session = await auth()
  if (!session) return NextResponse.json({ error: '未授權' }, { status: 401 })

  const { searchParams } = new URL(req.url)
  const all = searchParams.get('all') === 'true'

  if (all && session.user?.role !== 'ADMIN') {
    return NextResponse.json({ error: '未授權' }, { status: 401 })
  }

  const where = all ? {} : { userId: session.user?.id }

  const orders = await prisma.order.findMany({
    where,
    include: {
      items: { include: { product: { select: { name: true } } } },
      user: { select: { name: true, email: true } },
    },
    orderBy: { createdAt: 'desc' },
  })

  return NextResponse.json(orders)
}

export async function POST(req: NextRequest) {
  const session = await auth()
  if (!session) return NextResponse.json({ error: '未授權' }, { status: 401 })

  const { address, phone, items } = await req.json()

  if (!address || !phone || !items?.length) {
    return NextResponse.json({ error: '請填寫必要欄位' }, { status: 400 })
  }

  const productIds = items.map((i: { productId: string }) => i.productId)
  const products = await prisma.product.findMany({ where: { id: { in: productIds } } })

  const orderItems = items.map((item: { productId: string; quantity: number }) => {
    const product = products.find((p) => p.id === item.productId)
    if (!product) throw new Error(`找不到商品 ${item.productId}`)
    return {
      productId: item.productId,
      quantity: item.quantity,
      price: product.price,
    }
  })

  const totalPrice = orderItems.reduce(
    (sum: number, item: { price: number; quantity: number }) => sum + item.price * item.quantity,
    0
  )

  const order = await prisma.order.create({
    data: {
      userId: session.user!.id,
      address,
      phone,
      totalPrice,
      items: { create: orderItems },
    },
    include: { items: { include: { product: { select: { name: true } } } } },
  })

  try {
    await Promise.all([
      sendOrderConfirmationEmail(order, session.user!.email!, session.user!.name ?? null),
      sendNewOrderNotificationEmail(order, session.user!.email!),
    ])
  } catch {
  }

  return NextResponse.json(order, { status: 201 })
}
