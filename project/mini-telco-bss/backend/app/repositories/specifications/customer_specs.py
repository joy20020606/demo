from sqlalchemy import ColumnElement, or_

from app.core.enums import CustomerStatus, CustomerType
from app.models.customer import Customer
from app.repositories.specifications.base import Specification


class CustomerByStatus(Specification[Customer]):
    def __init__(self, status: CustomerStatus) -> None:
        self.status = status

    def to_clause(self) -> ColumnElement[bool]:
        return Customer.status == self.status


class CustomerByType(Specification[Customer]):
    def __init__(self, customer_type: CustomerType) -> None:
        self.customer_type = customer_type

    def to_clause(self) -> ColumnElement[bool]:
        return Customer.customer_type == self.customer_type


class CustomerSearchQuery(Specification[Customer]):
    def __init__(self, q: str) -> None:
        self.q = q

    def to_clause(self) -> ColumnElement[bool]:
        like = f"%{self.q}%"
        return or_(
            Customer.name.ilike(like),
            Customer.code.ilike(like),
            Customer.email.ilike(like),
        )
