from enum import Enum


class CustomerType(str, Enum):
    INDIVIDUAL = "individual"
    BUSINESS = "business"


class CustomerStatus(str, Enum):
    ACTIVE = "active"
    SUSPENDED = "suspended"
    TERMINATED = "terminated"


class PlanType(str, Enum):
    MONTHLY = "monthly"
    PREPAID = "prepaid"
    UNLIMITED = "unlimited"


class SubscriptionStatus(str, Enum):
    ACTIVE = "active"
    SUSPENDED = "suspended"
    TERMINATED = "terminated"


class UsageType(str, Enum):
    VOICE = "voice"
    SMS = "sms"
    DATA = "data"


class BillStatus(str, Enum):
    ISSUED = "issued"
    PAID = "paid"
    OVERDUE = "overdue"
    VOID = "void"


class UserRole(str, Enum):
    ADMIN = "admin"
    AGENT = "agent"
    READONLY = "readonly"
