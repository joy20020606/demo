-- Postgres docker-entrypoint-initdb.d 只在 volume 第一次建立時跑一次。
-- 這裡建立 warehouse DB + warehouse user + raw schema + raw tables。
-- analytics schema 由 dbt 自己建,不在這裡寫死。

CREATE USER warehouse WITH PASSWORD 'warehouse' SUPERUSER;
CREATE DATABASE warehouse OWNER warehouse;

\c warehouse warehouse

CREATE SCHEMA IF NOT EXISTS raw;

CREATE TABLE IF NOT EXISTS raw.customers (
    customer_id  UUID PRIMARY KEY,
    name         VARCHAR(100) NOT NULL,
    email        VARCHAR(200) NOT NULL,
    city         VARCHAR(50),
    created_at   TIMESTAMPTZ NOT NULL,
    updated_at   TIMESTAMPTZ NOT NULL
);

CREATE TABLE IF NOT EXISTS raw.products (
    product_id        UUID PRIMARY KEY,
    name              VARCHAR(200) NOT NULL,
    category          VARCHAR(50),
    list_price_cents  INTEGER NOT NULL,
    is_active         BOOLEAN NOT NULL DEFAULT TRUE,
    created_at        TIMESTAMPTZ NOT NULL
);

CREATE TABLE IF NOT EXISTS raw.orders (
    order_id      UUID PRIMARY KEY,
    customer_id   UUID NOT NULL,
    product_id    UUID NOT NULL,
    amount_cents  INTEGER NOT NULL,
    status        VARCHAR(20) NOT NULL,
    created_at    TIMESTAMPTZ NOT NULL,
    deleted_at    TIMESTAMPTZ
);

CREATE INDEX IF NOT EXISTS idx_orders_created_at ON raw.orders (created_at);
CREATE INDEX IF NOT EXISTS idx_orders_customer_id ON raw.orders (customer_id);
