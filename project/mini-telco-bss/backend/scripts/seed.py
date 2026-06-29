from __future__ import annotations

import asyncio
import random
from datetime import UTC, date, datetime, timedelta
from decimal import Decimal

from sqlalchemy import delete, select

from app.core.enums import (
    BillStatus,
    CustomerStatus,
    CustomerType,
    PlanType,
    SubscriptionStatus,
    UsageType,
    UserRole,
)
from app.core.security import hash_password
from app.db.session import AsyncSessionFactory
from app.models import (
    AuditLog,
    Bill,
    Customer,
    Plan,
    Subscription,
    UsageRecord,
    User,
    WebhookEvent,
)

random.seed(42)


USERS = [
    {"email": "admin@telco.local", "password": "admin1234", "full_name": "System Admin",
     "role": UserRole.ADMIN},
    {"email": "agent@telco.local", "password": "agent1234", "full_name": "Front Desk Agent",
     "role": UserRole.AGENT},
    {"email": "viewer@telco.local", "password": "viewer1234", "full_name": "Audit Viewer",
     "role": UserRole.READONLY},
]


PLANS = [
    {
        "code": "PLAN-MONTHLY-499", "name": "標準月租 NT$499", "plan_type": PlanType.MONTHLY,
        "monthly_fee": Decimal("499.00"),
        "included_minutes": 200, "included_sms": 100, "included_data_mb": 3072,
        "overage_minute_rate": Decimal("1.5"), "overage_sms_rate": Decimal("0.5"),
        "overage_mb_rate": Decimal("0.05"),
    },
    {
        "code": "PLAN-MONTHLY-999", "name": "進階月租 NT$999", "plan_type": PlanType.MONTHLY,
        "monthly_fee": Decimal("999.00"),
        "included_minutes": 500, "included_sms": 300, "included_data_mb": 10240,
        "overage_minute_rate": Decimal("1.0"), "overage_sms_rate": Decimal("0.3"),
        "overage_mb_rate": Decimal("0.03"),
    },
    {
        "code": "PLAN-PREPAID-100", "name": "預付卡 100 點", "plan_type": PlanType.PREPAID,
        "monthly_fee": Decimal("0"),
        "included_minutes": 0, "included_sms": 0, "included_data_mb": 0,
        "overage_minute_rate": Decimal("2.5"), "overage_sms_rate": Decimal("1.0"),
        "overage_mb_rate": Decimal("0.1"),
    },
    {
        "code": "PLAN-UNLIMITED-1399", "name": "吃到飽 NT$1399", "plan_type": PlanType.UNLIMITED,
        "monthly_fee": Decimal("1399.00"),
        "included_minutes": 999999, "included_sms": 999999, "included_data_mb": 999999999,
        "overage_minute_rate": Decimal("0"), "overage_sms_rate": Decimal("0"),
        "overage_mb_rate": Decimal("0"),
    },
    {
        "code": "PLAN-BIZ-2999", "name": "企業方案 NT$2999", "plan_type": PlanType.MONTHLY,
        "monthly_fee": Decimal("2999.00"),
        "included_minutes": 2000, "included_sms": 1000, "included_data_mb": 30720,
        "overage_minute_rate": Decimal("0.8"), "overage_sms_rate": Decimal("0.2"),
        "overage_mb_rate": Decimal("0.02"),
    },
]


CUSTOMER_NAMES_INDIVIDUAL = [
    "王小明", "陳大華", "李美玲", "張志強", "林淑芬",
    "黃志宏", "吳麗華", "蔡明達", "謝佳穎", "鄭文傑",
    "許雅婷", "劉宗翰", "周建良", "曾雅雯", "葉俊宏",
]
CUSTOMER_NAMES_BUSINESS = [
    "台北科技股份有限公司", "永和食品有限公司", "新竹半導體股份有限公司",
    "高雄物流股份有限公司", "台中設計工坊",
]


async def clear_tables() -> None:
    async with AsyncSessionFactory() as s:
        for model in [
            AuditLog, WebhookEvent, Bill, UsageRecord, Subscription,
            Customer, Plan, User,
        ]:
            await s.execute(delete(model))
        await s.commit()
        print("[seed] cleared all tables")


async def seed_users() -> None:
    async with AsyncSessionFactory() as s:
        for u in USERS:
            s.add(User(
                email=u["email"],
                hashed_password=hash_password(u["password"]),
                full_name=u["full_name"],
                role=u["role"],
                is_active=True,
            ))
        await s.commit()
        print(f"[seed] inserted {len(USERS)} users")


async def seed_plans() -> list[Plan]:
    async with AsyncSessionFactory() as s:
        plans = [Plan(**p, is_active=True) for p in PLANS]
        s.add_all(plans)
        await s.commit()
        for p in plans:
            await s.refresh(p)
        print(f"[seed] inserted {len(plans)} plans")
        return plans


async def seed_customers_and_subscriptions(plans: list[Plan]) -> list[Subscription]:
    subs: list[Subscription] = []
    async with AsyncSessionFactory() as s:
        idx = 1
        customers: list[Customer] = []
        for name in CUSTOMER_NAMES_INDIVIDUAL:
            customers.append(Customer(
                code=f"CUS-{idx:06d}",
                customer_type=CustomerType.INDIVIDUAL,
                name=name,
                email=f"user{idx}@example.com",
                phone=f"09{random.randint(10000000, 99999999)}",
                national_id=f"A{random.randint(100000000, 299999999)}",
                status=CustomerStatus.ACTIVE,
                credit_limit=Decimal("3000.00"),
            ))
            idx += 1
        for name in CUSTOMER_NAMES_BUSINESS:
            customers.append(Customer(
                code=f"CUS-{idx:06d}",
                customer_type=CustomerType.BUSINESS,
                name=name,
                email=f"biz{idx}@example.com",
                phone=f"02{random.randint(10000000, 99999999)}",
                national_id=f"{random.randint(10000000, 99999999)}",
                status=CustomerStatus.ACTIVE,
                credit_limit=Decimal("50000.00"),
            ))
            idx += 1

        s.add_all(customers)
        await s.commit()
        for c in customers:
            await s.refresh(c)
        print(f"[seed] inserted {len(customers)} customers")

        phone_seq = 0
        for c in customers:
            n_subs = 1 if c.customer_type == CustomerType.INDIVIDUAL else random.randint(2, 4)
            for _ in range(n_subs):
                plan = random.choice(plans)
                phone_seq += 1
                subs.append(Subscription(
                    customer_id=c.id,
                    plan_id=plan.id,
                    phone_number=f"09{phone_seq:08d}",
                    status=SubscriptionStatus.ACTIVE,
                    start_date=date.today() - timedelta(days=random.randint(30, 365)),
                ))
        s.add_all(subs)
        await s.commit()
        for sub in subs:
            await s.refresh(sub)
        print(f"[seed] inserted {len(subs)} subscriptions")
        return subs


async def seed_usage_and_bills(subs: list[Subscription], plans: list[Plan]) -> None:
    plan_by_id = {p.id: p for p in plans}
    today = date.today()
    period = today.strftime("%Y-%m")

    async with AsyncSessionFactory() as s:
        usage_count = 0
        bill_count = 0
        now = datetime.now(UTC)

        for sub in subs:
            plan = plan_by_id[sub.plan_id]

            voice_qty = random.randint(50, 600)
            sms_qty = random.randint(0, 200)
            data_qty = random.randint(500, 15000)

            for utype, qty in [
                (UsageType.VOICE, voice_qty),
                (UsageType.SMS, sms_qty),
                (UsageType.DATA, data_qty),
            ]:
                s.add(UsageRecord(
                    subscription_id=sub.id,
                    usage_type=utype,
                    quantity=qty,
                    recorded_at=now - timedelta(days=random.randint(1, 28)),
                    billing_period=period,
                ))
                usage_count += 1

            overage = Decimal("0")
            overage += max(0, voice_qty - plan.included_minutes) * plan.overage_minute_rate
            overage += max(0, sms_qty - plan.included_sms) * plan.overage_sms_rate
            overage += max(0, data_qty - plan.included_data_mb) * plan.overage_mb_rate

            total = plan.monthly_fee + overage
            s.add(Bill(
                subscription_id=sub.id,
                billing_period=period,
                monthly_fee=plan.monthly_fee,
                overage_charge=overage.quantize(Decimal("0.01")),
                total_amount=total.quantize(Decimal("0.01")),
                status=BillStatus.ISSUED,
                issued_at=now,
                due_date=today + timedelta(days=15),
            ))
            bill_count += 1

        await s.commit()
        print(f"[seed] inserted {usage_count} usage records, {bill_count} bills")


async def main() -> None:
    print("[seed] starting...")
    await clear_tables()
    await seed_users()
    plans = await seed_plans()
    subs = await seed_customers_and_subscriptions(plans)
    await seed_usage_and_bills(subs, plans)

    async with AsyncSessionFactory() as s:
        for model, label in [
            (User, "users"), (Plan, "plans"), (Customer, "customers"),
            (Subscription, "subscriptions"), (UsageRecord, "usage_records"),
            (Bill, "bills"),
        ]:
            count = (await s.execute(select(model))).scalars().all()
            print(f"  - {label}: {len(count)}")

    print("[seed] done.")
    print()
    print("Login accounts:")
    for u in USERS:
        print(f"  {u['role'].value:10s}  {u['email']:25s}  password: {u['password']}")


if __name__ == "__main__":
    asyncio.run(main())
