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

## 2026-06-29

### Completed

* Refactored the entire application from synchronous to asynchronous operations.
* Updated the repository layer to use Entity Framework Core async methods.
* Updated the service layer to return `Task<T>` and use `await`.
* Updated Minimal API endpoints to support asynchronous handlers.
* Introduced a generic `PagedResponse<T>` wrapper for paginated endpoints.
* Improved API responses to return pagination metadata alongside results.
* Refactored pagination logic to correctly calculate total record count before applying `Skip()` and `Take()`.
* Expanded the Backend Engineering Handbook with:
  * Async/Await
  * Program.cs
  * Configuration
  * Swagger
* Added new Cheat Sheets for:
  * EF Core
  * HTTP
  * Git

### Learned

* Async programming with `Task` and `await`
* Entity Framework Core async methods
* Generic response wrappers (`PagedResponse<T>`)
* API response design
* ASP.NET Core startup (`Program.cs`)
* Application configuration
* Swagger/OpenAPI

New EF Core async methods learned:

* `SaveChangesAsync()`
* `ToListAsync()`
* `FirstOrDefaultAsync()`

### Issues

* Initially returned a `Task` object instead of awaiting it, causing Swagger to serialize the task metadata instead of the actual data.
* Accidentally calculated pagination totals after applying `Skip()` and `Take()`, resulting in incorrect page counts.
* Needed to understand where `async` belongs in Minimal API endpoint lambdas.

### Biggest Insights

* Async/await isn't about making code execute faster. It's about allowing server threads to do other work while waiting on slow operations like database queries.
* Every async method forms a chain. If one layer becomes async, every caller above it generally becomes async as well.
* Generic types allow one response model (`PagedResponse<T>`) to work with any DTO.
* A well-designed API returns both the requested data and useful metadata such as page number, total pages, and total record count.

### Confidence

⭐⭐⭐⭐☆ (4/5)

Still want more practice with:

* Async call chains
* Generic response models
* Designing REST API responses

## 2026-06-30

### Completed

* Learned ASP.NET Core validation using Data Annotations.
* Added validation attributes to DTOs.
* Learned the differences between validation in Controllers and Minimal APIs.
* Built a reusable generic `ValidationFilter<T>` for Minimal APIs.
* Removed endpoint-specific validation logic in favor of centralized validation.
* Reorganized the Backend Engineering Handbook into a structured learning reference.
* Added and expanded handbook chapters for:
  * Repository Pattern
  * Service Layer
  * Dependency Flow
  * Minimal APIs
  * Filtering
  * Searching
  * Sorting
  * Pagination
  * API Responses
  * Program.cs
  * Configuration
  * Swagger
  * Validation
  * Collections
  * Lambdas
  * LINQ
  * Nullable Reference Types
  * Generics
* Created README files for every handbook section and reorganized the project into a long-term reference guide.

### Learned

* Data Annotations
* Validation pipeline
* Endpoint Filters
* Separation of concerns
* Nullable Reference Types
* Lambda expressions
* Collections
* API architecture
* Request processing pipeline

Validation attributes learned:

* `[Required]`
* `[StringLength]`
* `[MinLength]`
* `[MaxLength]`
* `[Range]`
* `[EmailAddress]`
* `[Phone]`

### Issues

* Initially assumed Data Annotation attributes automatically validated requests in Minimal APIs.
* Learned that Controllers automatically validate models, while Minimal APIs require explicit validation through Endpoint Filters or custom logic.
* Needed to better understand where validation belongs within the application architecture.

### Biggest Insights

* Validation is not business logic. It belongs at the edge of the application before the service layer executes.
* Data Annotations describe validation rules, but something else must enforce them.
* Endpoint Filters in Minimal APIs serve a similar purpose to automatic model validation in Controllers.
* DTOs define what valid input looks like, while services should assume they receive already validated data.
* The more the project grows, the more valuable architectural separation becomes. Each layer should have one clear responsibility.

### Confidence

⭐⭐⭐⭐☆ (4/5)

Still want more practice with:

* Endpoint Filters
* Validation strategies
* Entity Framework Core Change Tracking
* Relationships between entities