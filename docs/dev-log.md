## 2026-06-04

Completed:
- Created IntakeService
- Added repository layer
- Added dependency injection

Learned:
- Constructor injection
- Interfaces
- LINQ FirstOrDefault()

Issues:
- Confused IEnumerable vs List

## 2026-06-05

Completed:

* Refactored `IntakeService` to generate IDs internally.
* Updated `AddRequest()` to return the created `IntakeRequest`.
* Added request validation for updating status.
* Improved console application flow and error handling.

Learned:

* Encapsulation
* Service responsibilities
* Returning objects from service methods
* Input validation

Issues:

* Initially validated the new status before confirming the request ID existed.
* Needed to move ID generation into the service instead of `Program.cs`.

---

## 2026-06-06

Completed:

* Introduced the Repository pattern.
* Created `IIntakeRepository`.
* Created `InMemoryIntakeRepository`.
* Refactored `IntakeService` to depend on the repository instead of storing data directly.

Learned:

* Repository Pattern
* Separation of concerns
* Dependency Inversion
* Interfaces as contracts

Issues:

* Initially questioned why interfaces were necessary if every implementation still had to be written.
* Realized the interface rarely changes while implementations do.

---

## 2026-06-24

Completed:

* Created the ASP.NET Core Web API project.
* Configured Swagger/OpenAPI.
* Registered services using Dependency Injection.
* Created Minimal API endpoints.
* Added GET endpoints for all requests and individual requests.
* Added POST endpoint for creating requests.
* Added PUT endpoint for updating request status.
* Introduced DTOs for API requests.

Learned:

* ASP.NET Minimal APIs
* Dependency Injection container
* REST API design
* HTTP methods (GET, POST, PUT)
* HTTP status codes
* DTOs
* Model binding

Issues:

* Forgot to import the Models namespace when adding new endpoints.
* Received HTTP 200 instead of 404 until returning `Results.NotFound()`.
* Swagger required restarting after DTO changes.
* Learned that enum values are serialized as integers by default.

---

## 2026-06-25

Completed:

* Added Entity Framework Core packages.
* Learned the role of `DbContext` and `DbSet`.
* Compared EF Core to Prisma and Drizzle.
* Began planning the database migration from the in-memory repository.
* Started creating the Backend Engineering Handbook to document concepts.

Learned:

* Entity Framework Core
* Object Relational Mapper (ORM)
* DbContext
* DbSet
* Change Tracking
* Service lifetimes (Singleton, Scoped, Transient)

Issues:

* Attempted to install EF Core 10 on a .NET 8 project, causing package compatibility errors.
* Learned to use package version `8.*` to match the target framework.
* Initially viewed EF Core as completely new before realizing it fills the same role as Prisma/Drizzle in the .NET ecosystem.

Biggest Insight:
- Dependency Injection doesn't just "inject dependencies." It centralizes who creates and wires objects.
- EF Core is basically Prisma/Drizzle for C#.

## 2026-06-26

### Completed

* Replaced the in-memory repository with `EfIntakeRepository`.
* Connected the application to SQLite using Entity Framework Core.
* Created the first EF Core migration.
* Created and initialized the SQLite database.
* Added automatic development seed data with first-run detection.
* Implemented advanced LINQ querying:

  * Status filtering
  * Patient name searching
  * Sorting
  * Pagination
* Added DTO projection using `Select()`.
* Refactored DTO mapping from the endpoint into the service layer.
* Expanded the Backend Engineering Handbook with:

  * Entity Framework Core
  * DbContext
  * DbSet
  * Migrations
  * LINQ
  * DTOs + `Select()`
* Created a LINQ Cheatsheet to track commonly used operators.

### Learned

* EF Core Migrations
* SQLite database workflow
* LINQ query composition
* DTO projection
* Query pipelines
* Pagination
* Seed data initialization

LINQ methods learned:

* `Where()`
* `Contains()`
* `FirstOrDefault()`
* `Count()`
* `Any()`
* `OrderBy()`
* `OrderByDescending()`
* `Skip()`
* `Take()`
* `Select()`

### Issues

* Initially installed EF Core 10 instead of EF Core 8, causing package compatibility errors.
* Seed data was inserted every application startup until an `Any()` check was added.
* DTO mapping was originally placed inside the endpoint before being refactored into the service layer.
* Needed to clarify the relationship between LINQ and Entity Framework. LINQ is part of C#, while EF Core translates LINQ queries into SQL.

### Biggest Insights

* LINQ is C#'s built-in query language, not a database library.
* Entity Framework Core plays the same role in C# that Drizzle and Prisma play in JavaScript.
* A single endpoint can support filtering, searching, sorting, pagination, and DTO projection without creating multiple specialized endpoints.
* Good endpoint design keeps HTTP endpoints thin while placing business logic and data shaping inside the service layer.
* LINQ methods form a query pipeline, with each method performing one operation before passing the results to the next step.

### Confidence

⭐⭐⭐⭐☆ (4/5)

Still want more practice with:

* Async/await
* Writing more complex LINQ queries
* Recognizing when DTOs should differ from domain models
