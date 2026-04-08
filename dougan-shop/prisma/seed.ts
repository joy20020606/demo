import { PrismaClient } from '../app/generated/prisma/client'
import { PrismaBetterSqlite3 } from '@prisma/adapter-better-sqlite3'
import bcrypt from 'bcryptjs'

const adapter = new PrismaBetterSqlite3({ url: './dev.db' })
const prisma = new PrismaClient({ adapter })

async function main() {
  const adminPassword = await bcrypt.hash('admin123', 10)
  const userPassword = await bcrypt.hash('user123', 10)

  await prisma.user.upsert({
    where: { email: 'admin@dougan.com' },
    update: {},
    create: { email: 'admin@dougan.com', name: '管理員', password: adminPassword, role: 'ADMIN' },
  })

  await prisma.user.upsert({
    where: { email: 'user@dougan.com' },
    update: {},
    create: { email: 'user@dougan.com', name: '測試會員', password: userPassword, role: 'CUSTOMER' },
  })

  const products = [
    { name: '經典五香豆干', description: '傳統配方，香氣濃郁，口感紮實', price: 80, stock: 100 },
    { name: '辣味豆干', description: '微辣帶勁，越吃越過癮', price: 85, stock: 80 },
    { name: '滷汁豆干', description: '獨家滷汁，入口即化', price: 90, stock: 60 },
    { name: '海苔豆干', description: '搭配日式海苔，風味獨特', price: 95, stock: 50 },
    { name: '黑胡椒豆干', description: '黑胡椒提味，層次豐富', price: 88, stock: 70 },
    { name: '蒜香豆干', description: '濃郁蒜香，下酒一絕', price: 82, stock: 90 },
  ]

  for (const p of products) {
    await prisma.product.create({ data: p })
  }

  console.log('Seed 完成！')
  console.log('管理員帳號: admin@dougan.com / admin123')
  console.log('測試會員帳號: user@dougan.com / user123')
}

main().finally(() => prisma.$disconnect())
