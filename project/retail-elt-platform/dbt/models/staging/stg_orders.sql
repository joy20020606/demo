{{ config(materialized='view') }}

-- Staging layer:只負責清洗 + 改名 + type cast,不做 join、不做業務邏輯。
SELECT
    order_id,
    customer_id,
    product_id,
    LOWER(status)              AS order_status,
    amount_cents / 100.0       AS amount_dollars,
    created_at                 AS order_at,
    DATE(created_at)           AS order_date
FROM {{ source('raw', 'orders') }}
WHERE deleted_at IS NULL
