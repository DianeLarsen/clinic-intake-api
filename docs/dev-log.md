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