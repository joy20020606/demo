from collections.abc import Sequence
from uuid import UUID

from fastapi import HTTPException, status

from app.models.customer import Customer
from app.repositories.specifications.base import combine_and
from app.repositories.specifications.customer_specs import (
    CustomerByStatus,
    CustomerByType,
    CustomerSearchQuery,
)
from app.schemas.customer import (
    CustomerCreate,
    CustomerSearchFilters,
    CustomerUpdate,
)
from app.services.uow import AsyncUnitOfWork


class CustomerService:
    def __init__(self, uow: AsyncUnitOfWork) -> None:
        self.uow = uow

    async def get(self, customer_id: UUID) -> Customer:
        customer = await self.uow.customers.get(customer_id)
        if customer is None:
            raise HTTPException(status.HTTP_404_NOT_FOUND, "Customer not found")
        return customer

    async def search(
        self, filters: CustomerSearchFilters, limit: int, offset: int
    ) -> tuple[Sequence[Customer], int]:
        specs = []
        if filters.status is not None:
            specs.append(CustomerByStatus(filters.status))
        if filters.customer_type is not None:
            specs.append(CustomerByType(filters.customer_type))
        if filters.q:
            specs.append(CustomerSearchQuery(filters.q))
        combined = combine_and(specs)
        return await self.uow.customers.search(combined, limit=limit, offset=offset)

    async def create(self, payload: CustomerCreate) -> Customer:
        existing = await self.uow.customers.get_by_code(payload.code)
        if existing is not None:
            raise HTTPException(
                status.HTTP_409_CONFLICT, f"Customer code '{payload.code}' already exists"
            )
        customer = Customer(**payload.model_dump())
        await self.uow.customers.add(customer)
        await self.uow.commit()
        return customer

    async def update(self, customer_id: UUID, payload: CustomerUpdate) -> Customer:
        customer = await self.get(customer_id)
        for field, value in payload.model_dump(exclude_unset=True).items():
            setattr(customer, field, value)
        await self.uow.commit()
        return customer
