import { NextResponse } from 'next/server'
import { prisma } from '@/lib/prisma'
import { auth } from '@/lib/auth'

export async function GET() {
  const session = await auth()
  if (!session || session.user?.role !== 'ADMIN') {
    return NextResponse.json({ error: '未授權' }, { status: 401 })
  }

  const products = await prisma.product.findMany({
    orderBy: { createdAt: 'desc' },
  })

  return NextResponse.json(products)
}
