# C4 Architecture Documentation — Inventory Service

## 1. System Context

```mermaid
graph LR
    Client[React Frontend] -->|REST/gRPC| Inventory[Inventory Service]
    POS[POS Service] -->|REST/Events| Inventory
    Inventory -->|Queries| PostgreSQL[(PostgreSQL inventory_db)]
    Inventory -->|Cache| Redis[(Redis)]
    Inventory -->|Events| RabbitMQ[RabbitMQ]
    Admin[Admin Panel] -->|REST| Inventory
    Seq -->|Logs| Inventory
    Prometheus -->|Metrics| Inventory
```

**Description:**
- React frontend consumes Inventory APIs for product management
- POS Service queries products and receives stock update events
- PostgreSQL is the primary datastore
- Redis caches frequently accessed products
- RabbitMQ enables async communication with POS
- Seq collects structured logs
- Prometheus scrapes metrics

---

## 2. Container Diagram

```mermaid
graph LR
    subgraph "Inventory Service"
        API[API Layer<br/>Controllers, Middleware]
        App[Application Layer<br/>Commands, Queries, Validators]
        Domain[Domain Layer<br/>Entities, Value Objects]
        Infra[Infrastructure Layer<br/>EF Core, Repositories, Event Bus]
    end

    API --> App
    App --> Domain
    Infra --> Domain
    Infra --> App
    
    API --> PostgreSQL[(PostgreSQL)]
    Infra --> PostgreSQL
    Infra --> Redis
    Infra --> RabbitMQ
```

---

## 3. Component Diagram

```mermaid
graph LR
    subgraph "API Layer"
        Controller[ProductsController]
        Middleware[GlobalExceptionHandler]
        Health[Health Checks]
        Release[Release Info]
    end

    subgraph "Application Layer"
        Mediator[MediatR]
        Validators[FluentValidation]
        Handlers[Command/Query Handlers]
        DTOs[DTOs]
    end

    subgraph "Domain Layer"
        Entities[Entities: Product, Category, Brand, Unit, Supplier, Warehouse]
        ValueObjects[Value Objects]
        Events[Domain Events]
    end

    subgraph "Infrastructure Layer"
        DbContext[InventoryDbContext]
        Configs[Entity Configurations]
        Repositories[Repositories]
        Logging[Serilog]
        Cache[Redis Cache]
        Events[Event Bus]
    end

    Controller --> Mediator
    Mediator --> Validators
    Mediator --> Handlers
    Handlers --> Entities
    Handlers --> DbContext
    DbContext --> Configs
    Configs --> Entities
    Handlers --> Cache
    Handlers --> Events
```

---

## 4. Deployment Diagram

```mermaid
graph LR
    subgraph "Docker Container"
        subgraph "inventory-api"
            App[InventoryService.API]
            Serilog[Serilog]
            MediatR[MediatR]
        end
        PostgreSQL[(PostgreSQL 16<br/>inventory_db)]
        Redis[(Redis 7<br/>Cache)]
        RabbitMQ[RabbitMQ<br/>Events]
    end

    Internet[Internet / Load Balancer] -->|HTTPS 5002| App
    App -->|TCP 5432| PostgreSQL
    App -->|TCP 6379| Redis
    App -->|AMQP 5672| RabbitMQ
```

---

## 5. Key Design Decisions

1. **Clean Architecture:** Domain has no infrastructure dependencies
2. **CQRS:** Commands and Queries are separate, routed via MediatR
3. **Vertical Slice:** Each feature is self-contained in Application layer
4. **Provider Abstraction:** Database provider is configuration-driven
5. **Soft Delete:** All entities support soft delete via query filter
6. **Audit Trail:** Automatic population of audit fields
7. **Multi-tenancy:** tenant_id column on all tables
