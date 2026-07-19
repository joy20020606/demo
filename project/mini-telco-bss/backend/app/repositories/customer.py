from collections.abc import Sequence

from sqlalchemy import func, select

from app.models.customer import Customer
from app.repositories.base import BaseRepository
from app.repositories.specifications.base import Specification


class CustomerRepository(BaseRepository[Customer]):
    model = Customer

    async def get_by_code(self, code: str) -> Customer | None:
        stmt = select(Customer).where(Customer.code == code)
        result = await self.session.execute(stmt)
        return result.scalar_one_or_none()

    async def search(
        self,
        spec: Specification[Customer] | None,
        limit: int = 20,
        offset: int = 0,
    ) -> tuple[Sequence[Customer], int]:
        base_stmt = select(Customer)
        count_stmt = select(func.count()).select_from(Customer)
        if spec is not None:
            clause = spec.to_clause()
            base_stmt = base_stmt.where(clause)
            count_stmt = count_stmt.where(clause)

        items_stmt = base_stmt.order_by(Customer.code).limit(limit).offset(offset)
        items = (await self.session.execute(items_stmt)).scalars().all()
        total = (await self.session.execute(count_stmt)).scalar_one()
        return items, total
