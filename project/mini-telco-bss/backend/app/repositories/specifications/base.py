from __future__ import annotations

from abc import ABC, abstractmethod
from functools import reduce
from typing import Generic, TypeVar

from sqlalchemy import ColumnElement, and_, or_

from app.db.base import Base

ModelT = TypeVar("ModelT", bound=Base)


class Specification(ABC, Generic[ModelT]):
    """Composable WHERE clauses. Mirrors C# ISpecification<T> pattern."""

    @abstractmethod
    def to_clause(self) -> ColumnElement[bool]: ...

    def __and__(self, other: Specification[ModelT]) -> Specification[ModelT]:
        return AndSpecification(self, other)

    def __or__(self, other: Specification[ModelT]) -> Specification[ModelT]:
        return OrSpecification(self, other)


class AndSpecification(Specification[ModelT]):
    def __init__(self, left: Specification[ModelT], right: Specification[ModelT]) -> None:
        self.left = left
        self.right = right

    def to_clause(self) -> ColumnElement[bool]:
        return and_(self.left.to_clause(), self.right.to_clause())


class OrSpecification(Specification[ModelT]):
    def __init__(self, left: Specification[ModelT], right: Specification[ModelT]) -> None:
        self.left = left
        self.right = right

    def to_clause(self) -> ColumnElement[bool]:
        return or_(self.left.to_clause(), self.right.to_clause())


def combine_and(specs: list[Specification[ModelT]]) -> Specification[ModelT] | None:
    if not specs:
        return None
    return reduce(lambda a, b: a & b, specs)
