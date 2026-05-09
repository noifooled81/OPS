# DATA CONTRACT

## Order Service Schema

```yaml
entity: Order
columns:
  - name: id
    type: UUID (PK)
    description: The unique order identifier.
  - name: customer_id
    type: UUID
    description: Reference to the user (owned by User Service).
  - name: status
    type: ENUM [CREATED, INVENTORY_PENDING, INVENTORY_RESERVED, PAYMENT_PENDING, PAYMENT_VALIDATED, RELEASE_STOCK_PENDING, APPROVED, COMPLETED, CANCELLED, REFUNDED]
    description: "Order state machine"
  - name: total_amount
    type: BIGINT
    description: Price in cents (to avoid floating-point errors).
  - name: currency
    type: VARCHAR(3)
    description: "e.g., 'USD', 'EUR'."
  - name: idempotency_key
    type: UUID
    description: Client-side key to prevent double-ordering on retry.
  - name: version
    type: INT
    description: For Optimistic Locking (preventing race conditions).
  - name: payment_id
    type: UUID (Nullable)
    description: Link to Payment Service record after success
  - name: snapshot_address
    type: JSONB
    description: The shipping address at the time of purchase.
  - name: created_at
    type: TIMESTAMP
    description: Audit trail.

```

## Inventory Service Schema

```yaml
entity: Inventory
columns:
  - name: sku_id
    type: VARCHAR (PK)
    description: The unique product identifier.
  - name: available_stock
    type: INT
    description: Stock ready for sale.
  - name: reserved_stock
    type: INT
    description: Stock locked by INVENTORY_RESERVED orders.

```

```yaml
entity: Reservation
columns:
  - name: id
    type: UUID (PK)
    description: Reservation ID.
  - name: order_id
    type: UUID
    description: The link back to the Order Service.
  - name: sku_id
    type: VARCHAR
    description: What is being held.
  - name: quantity
    type: INT
    description: How many.
  - name: status
    type: ENUM [ACTIVE, CONSUMED, RELEASED]
    description: ACTIVE (held), CONSUMED (sold), RELEASED (back to available)
  - name: expires_at
    type: TIMESTAMP
    description: The TTL (Time-to-Live) for the lock.

```

## Payment Service Schema

```yaml
entity: Payment
columns:
  - name: id
    type: UUID (PK)
    description: Internal transaction ID.
  - name: order_id
    type: UUID
    description: Reference to the Order.
  - name: external_ref
    type: VARCHAR
    description: The Stripe/PayPal ID.
  - name: status
    type: ENUM
    description: "PENDING, COMPLETED, REFUNDED, FAILED."
  - name: amount
    type: BIGINT
    description: Amount charged in cents.
  - name: idempotency_key
    type: VARCHAR
    description: Prevents double charge on MQ retries
  - name: failure_reason
    type: TEXT
    description: Log for debugging rejected cards.

```

## Shared Schema
```yaml
entity: MessageRelay
columns:
  - name: id
    type: UUID
    description: Primary Key.
  - name: aggregate_id
    type: VARCHAR
    description: The OrderID or PaymentID (helps for tracing).
  - name: event_type
    type: VARCHAR
    description: ORDER_RESERVED, PAYMENT_FAILED, etc.
  - name: payload
    type: JSONB
    description: The full message data.
  - name: status
    type: SMALLINT
    description: 0 (Pending), 1 (Sent), 2 (Failed).
  - name: attempts
    type: INT
    description: Track retries for the Relay worker.
  - name: created_at
    type: TIMESTAMP
    description: For ordering and cleanup.

```

## Data Ownership

| Service | Owns | Can Read | Cannot Modify |
|:---:|:---:|:---:|:---:|
| Order | Orders | Payment Status | Inventory Stock |
| Inventory | Stock, Reservations | Order Ids | Order Status |
| Payment | Transactions | Order State | Inventory |

## Events
- OrderInventoryReserved

Published by: Inventory Service -> Consumed by: Order Service & Payment Service

```json
{
  "event_id": "evt_abc123",
  "event_version": 1,
  "event_type": "INVENTORY_RESERVED",
  "timestamp": "2026-05-08T09:00:00Z",
  "data": {
    "order_id": "ord_550e8400",
    "reservation_id": "res_998877",
    "expires_at": "2026-05-08T09:15:00Z" 
  }
}
```

- OrderInventoryFailed

Published by: Inventory Service -> Consumed by: Order

```json
{
  "event_id": "evt_abc123",
  "event_version": 1,
  "event_type": "INVENTORY_RESERVATION_FAILED",
  "timestamp": "2026-05-08T09:00:00Z",
  "data": {
    "order_id": "ord_550e8400",
    "reason": "INSUFFICIENT_STOCK"
  }
}
```

- PaymentFinalized

Published by: Payment Service -> Consumed by: Order Service & Inventory Service

```json
{
  "event_id": "evt_def456",
  "event_version": 1,
  "event_type": "PAYMENT_SUCCESS",
  "timestamp": "2026-05-08T09:00:00Z",
  "data": {
    "order_id": "ord_550e8400",
    "transaction_id": "txn_stripe_001",
    "amount_captured": 5000,
    "currency": "USD"
  }
}
```

- PaymentFailed

Published by: Payment Service -> Consumed by: Order Service & Inventory Service

```json
{
  "event_id": "evt_def456",
  "event_version": 1,
  "event_type": "PAYMENT_FAILED",
  "timestamp": "2026-05-08T09:00:00Z",
  "data": {
    "order_id": "ord_550e8400",
    "reason": "CARD_DECLINED"
  }
}
```
