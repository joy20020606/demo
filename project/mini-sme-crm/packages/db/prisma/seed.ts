/**
 * Seed script — 塞範例資料讓 demo 一打開就有東西看
 * 跑法：pnpm db:seed
 */
import { PrismaClient } from '@prisma/client';

const prisma = new PrismaClient();

async function main() {
  // 清空（dev 環境用，prod 千萬別這樣寫）
  await prisma.contactLog.deleteMany();
  await prisma.deal.deleteMany();
  await prisma.customer.deleteMany();

  // 建立 3 個客戶
  const c1 = await prisma.customer.create({
    data: {
      name: '王大明',
      email: 'wang@example.com',
      phone: '0912-345-678',
      company: '大明食品行',
      tags: ['批發', '北部'],
    },
  });

  const c2 = await prisma.customer.create({
    data: {
      name: '李小華',
      email: 'lee@xyz.com',
      company: 'XYZ 科技',
      tags: ['VIP', '科技業'],
    },
  });

  const c3 = await prisma.customer.create({
    data: {
      name: '陳美麗',
      phone: '0922-111-222',
      company: '美麗烘焙坊',
      tags: ['餐飲'],
    },
  });

  // 商機
  await prisma.deal.create({
    data: {
      customerId: c1.id,
      title: 'ERP 導入專案',
      amount: 350000,
      stage: 'PROPOSAL',
    },
  });

  await prisma.deal.create({
    data: {
      customerId: c2.id,
      title: 'CRM 升級',
      amount: 120000,
      stage: 'NEGOTIATION',
    },
  });

  // 聯絡記錄
  await prisma.contactLog.create({
    data: {
      customerId: c1.id,
      channel: 'EMAIL',
      summary: '寄了報價單，客戶說要跟內部討論。',
    },
  });

  console.log('✅ Seed complete');
  console.log(`   - 3 customers, 2 deals, 1 contact log`);
}

main()
  .catch((e) => {
    console.error(e);
    process.exit(1);
  })
  .finally(async () => {
    await prisma.$disconnect();
  });
