/**
 * Seed sample data — 1 個 tenant、3 個使用者（不同角色）、3 條 route、1 個獎勵方案。
 * Run: pnpm db:seed
 */
import { PrismaClient } from '@prisma/client';
import bcrypt from 'bcryptjs';

const prisma = new PrismaClient();

async function main() {
  console.log('🌱 Seeding...');

  // Clean slate（dev only）
  await prisma.incentiveProgram.deleteMany();
  await prisma.route.deleteMany();
  await prisma.user.deleteMany();
  await prisma.tenant.deleteMany();

  // 1. Tenant
  const tenant = await prisma.tenant.create({
    data: {
      name: 'Houston Transit Authority',
    },
  });
  console.log(`✓ Tenant: ${tenant.name} (${tenant.id})`);

  // 2. Users (password: password123 for all)
  const passwordHash = await bcrypt.hash('password123', 10);
  const users = await Promise.all([
    prisma.user.create({
      data: {
        email: 'admin@houston-transit.gov',
        passwordHash,
        role: 'ADMIN',
        tenantId: tenant.id,
      },
    }),
    prisma.user.create({
      data: {
        email: 'operator@houston-transit.gov',
        passwordHash,
        role: 'OPERATOR',
        tenantId: tenant.id,
      },
    }),
    prisma.user.create({
      data: {
        email: 'viewer@houston-transit.gov',
        passwordHash,
        role: 'VIEWER',
        tenantId: tenant.id,
      },
    }),
  ]);
  users.forEach((u) => console.log(`✓ User: ${u.email} (${u.role})`));

  // 3. Routes (Houston downtown sample)
  const routes = await Promise.all([
    prisma.route.create({
      data: {
        name: 'Downtown → Med Center',
        startLocation: { lat: 29.7604, lng: -95.3698, address: '900 Bagby St, Houston, TX' },
        endLocation: { lat: 29.7104, lng: -95.4012, address: '6500 Main St, Houston, TX' },
        distanceKm: 8.2,
        tenantId: tenant.id,
      },
    }),
    prisma.route.create({
      data: {
        name: 'Energy Corridor → Galleria',
        startLocation: { lat: 29.7838, lng: -95.6353, address: 'Energy Corridor, Houston, TX' },
        endLocation: { lat: 29.7401, lng: -95.4634, address: 'Galleria, Houston, TX' },
        distanceKm: 18.5,
        tenantId: tenant.id,
      },
    }),
    prisma.route.create({
      data: {
        name: 'Sugar Land → Downtown',
        startLocation: { lat: 29.6196, lng: -95.6349, address: 'Sugar Land, TX' },
        endLocation: { lat: 29.7604, lng: -95.3698, address: 'Downtown Houston, TX' },
        distanceKm: 32.0,
        tenantId: tenant.id,
      },
    }),
  ]);
  routes.forEach((r) => console.log(`✓ Route: ${r.name}`));

  // 4. IncentiveProgram
  const program = await prisma.incentiveProgram.create({
    data: {
      name: 'Off-peak Carpool Reward Q3',
      description: 'Encourage carpooling during 10am-3pm to reduce peak congestion.',
      budget: 50000.0,
      status: 'DRAFT',
      tenantId: tenant.id,
    },
  });
  console.log(`✓ IncentiveProgram: ${program.name}`);

  console.log('\n✅ Seed complete!');
  console.log('\nTest credentials:');
  console.log('  admin@houston-transit.gov / password123');
  console.log('  operator@houston-transit.gov / password123');
  console.log('  viewer@houston-transit.gov / password123');
}

main()
  .catch((e) => {
    console.error('❌ Seed failed:', e);
    process.exit(1);
  })
  .finally(async () => {
    await prisma.$disconnect();
  });
