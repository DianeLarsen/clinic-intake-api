# Backend Learning Notes

## Interfaces

-   Depend on interfaces instead of concrete implementations.
-   Interfaces are contracts describing **what** a class must do.
-   The interface usually stays the same while implementations
    (InMemory, SQLite, Azure SQL) change.
-   The compiler enforces the contract. If you add a method to an
    interface, every implementation must add it too.
-   Think of an interface as a plug/socket: the service plugs into the
    interface, not the implementation.

## Dependency Injection (DI)

### Problem

Without DI, every class creates its own dependencies with `new`, making
code tightly coupled.

### Solution

ASP.NET creates and connects objects for you.

`Program.cs` becomes the central wiring diagram:

``` csharp
builder.Services.AddSingleton<IIntakeRepository, InMemoryIntakeRepository>();
builder.Services.AddSingleton<IIntakeService, IntakeService>();
```

### Mental Model

Dependency Injection **centralizes the creation and wiring of objects**.

Think of ASP.NET as the electrician wiring a control cabinet. Your
classes simply expose connection points through constructor parameters.

### Lifetimes

-   **Singleton**: One instance for the application's lifetime.
-   **Scoped**: One instance per HTTP request (used by EF Core
    DbContext).
-   **Transient**: New instance every time requested.

## Entity Framework Core (EF Core)

### Problem

A `List<T>` loses all data when the application stops.

### Solution

Use EF Core (an ORM) to map C# objects to database tables.

### ORM

Object Relational Mapper

    C# Objects
        ↓
    Database Tables

### DbContext

Represents a connection/session with the database.

Responsibilities: - Knows what tables exist - Maps models to tables -
Tracks changes - Saves changes

Think: **Gateway to the database**

### DbSet

Represents a table.

``` csharp
public DbSet<IntakeRequest> IntakeRequests => Set<IntakeRequest>();
```

Examples:

``` csharp
_db.IntakeRequests.Add(request);
_db.IntakeRequests.FirstOrDefault(...);
_db.IntakeRequests.Where(...);
_db.SaveChanges();
```

### Why EF Core?

-   Work in C# instead of SQL
-   Automatic object mapping
-   Automatic change tracking
-   Database migrations
-   Easier database switching

## EF Core vs Prisma vs Drizzle

  TypeScript     C#
  -------------- -----------------------
  Prisma         Entity Framework Core
  Drizzle        Entity Framework Core
  PrismaClient   DbContext
  Prisma Model   C# Model

Prisma:

``` ts
await prisma.user.findMany();
```

EF Core:

``` csharp
_db.Users.ToList();
```

### EF Core's Special Feature

Change Tracking:

``` csharp
User user = _db.Users.First();

user.Name = "Diane";

_db.SaveChanges();
```

EF automatically generates the SQL UPDATE.

## Cloud Ecosystem

  Language     Framework           ORM
  ------------ ------------------- ------------------
  C#           ASP.NET Core        EF Core
  TypeScript   Next.js / Express   Prisma / Drizzle
  Python       FastAPI             SQLAlchemy
  Java         Spring Boot         Hibernate
  Go           Gin                 GORM

### Equivalent Concepts

  Already Know         Learning
  -------------------- ----------------------
  Next.js API Routes   ASP.NET Minimal APIs
  Prisma / Drizzle     EF Core
  PostgreSQL           SQLite → Azure SQL
  npm                  dotnet
  package.json         .csproj
  TypeScript Classes   C# Classes
  Drizzle Migrations   EF Migrations
  Vercel               Azure App Service

## Biggest Takeaways

-   **Classes** model real-world things.
-   **Interfaces** describe capabilities and enforce contracts.
-   **Dependency Injection** centralizes object creation and wiring.
-   **Repositories** isolate data access.
-   **EF Core** lets C# work with databases using objects instead of
    writing SQL everywhere.
