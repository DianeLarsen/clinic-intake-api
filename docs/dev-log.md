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

* Refactored the API to use asynchronous endpoint handlers throughout the application.
* Implemented centralized request validation using a reusable generic `ValidationFilter<T>`.
* Removed manual validation logic from endpoints.
* Learned Entity Framework Core Change Tracking.
* Documented the Entity Framework entity lifecycle and tracking states.
* Expanded the Backend Engineering Handbook with chapters covering:
  * Validation
  * Generics
  * Collections
  * Lambdas
  * LINQ
  * Nullable Reference Types
  * Program.cs
  * Configuration
  * Swagger
  * Change Tracking
* Reorganized the Backend Engineering Handbook into a long-term reference guide with section README files and improved navigation.

### Learned

* Entity Framework Change Tracking
* Entity States
  * Added
  * Modified
  * Deleted
  * Unchanged
* Detached vs Tracked entities
* `AsNoTracking()`
* Endpoint Filters
* Data Annotations
* Generic validation
* Generic response objects

New EF Core methods learned:

* `AsNoTracking()`
* `Update()`
* `Attach()`

### Issues

* Initially assumed changing a tracked object immediately updated the database.
* Needed to understand that `SaveChangesAsync()` is the point where EF Core synchronizes in-memory objects with the database.
* Clarified when `Update()` should and should not be used.
* Learned that using `Update()` with partial DTOs can accidentally overwrite existing database values.

### Biggest Insights

* EF Core is not watching the database; it is watching the C# objects it loaded.
* `SaveChangesAsync()` is a synchronization operation, not simply an update method.
* Multiple changes to the same entity are combined into a single SQL statement.
* Tracked entities automatically detect property changes, while detached entities must first be attached to the `DbContext`.
* `AsNoTracking()` improves performance for read-only queries because EF Core skips building change-tracking information.
* The safest update pattern for APIs is to load the existing entity, modify only the intended properties, and then call `SaveChangesAsync()`.

### Confidence

⭐⭐⭐⭐☆ (4/5)

Still want more practice with:

* Entity relationships
* Navigation properties
* Lazy vs Eager Loading
* More advanced LINQ queries


---

# 2026-07-01

## Completed

* Completed the Entity Framework Core learning section.
* Added one-to-many relationships between:

  * Clinics
  * Patients
  * Intake Requests
* Added navigation properties and foreign keys to the domain models.
* Refactored development seed data to create related entities in the correct order.
* Learned how to safely evolve an existing database schema using EF Core migrations.
* Converted `PatientId` from nullable to required after backfilling existing data.
* Updated request queries to include related patient and clinic information.
* Refactored DTO projection to use navigation properties instead of duplicated data.
* Expanded the Backend Engineering Handbook with chapters covering:

  * Relationships
  * Lazy vs Eager Loading
  * EF Core Performance
* Improved project documentation including the GitHub README and architecture overview.

## Learned

* One-to-many relationships
* Foreign keys
* Navigation properties
* Required vs optional relationships
* Eager Loading
* Lazy Loading
* Explicit Loading
* `Include()`
* `ThenInclude()`
* Object graphs
* N+1 query problem
* Query performance
* Database normalization
* Relationship migrations
* Backfilling existing data before enforcing constraints

New EF Core methods learned:

* `Include()`
* `ThenInclude()`

## Issues

* Initially attempted to make `PatientId` required before existing records had been backfilled.
* Learned that adding required relationships to existing databases requires a staged migration:

  * Add nullable relationship
  * Populate existing data
  * Change to required
* Accidentally created foreign key violations while refactoring the seed data.
* Realized that navigation properties may still be `null` even when the database relationship exists because related entities are not automatically loaded.

## Biggest Insights

* Foreign keys represent relationships in the database while navigation properties represent relationships in C# objects.
* The database stores IDs; Entity Framework turns those IDs into connected objects.
* Good relational design removes duplicated information instead of copying data across tables.
* Loading related data is a separate decision from defining relationships.
* Query performance depends more on retrieving only the data needed than on writing clever code.
* Production schema changes should preserve existing data instead of rebuilding the database.

## Confidence

⭐⭐⭐⭐☆ (4/5)

Still want more practice with:

* Complex LINQ queries
* Many-to-many relationships
* Performance optimization
* More advanced Entity Framework querying

---

# 2026-07-07

## Completed

* Began converting the Clinic Intake API from Minimal APIs to ASP.NET Controllers using a dedicated Git feature branch.
* Created the first `RequestsController`.
* Migrated the initial GET and POST endpoints from `Program.cs` into controller actions.
* Registered controllers within the ASP.NET application.
* Learned professional Git feature branch workflows including pushing branches with upstream tracking.
* Refactored the domain model to better reflect a real healthcare intake workflow.
* Replaced `Patient.FullName` with:

  * FirstName
  * LastName
  * DateOfBirth
* Changed intake creation to reference existing patients using `PatientId` instead of creating requests from free-form patient names.
* Updated development seed data to support the new patient model.
* Refactored DTOs, services, repositories, and queries to use normalized patient information.

## Learned

* ASP.NET Controllers
* Controller routing
* `[ApiController]`
* `[Route]`
* `[HttpGet]`
* `[HttpPost]`
* `ControllerBase`
* `IActionResult`
* Constructor Dependency Injection in Controllers
* Feature branch Git workflow
* Domain-driven modeling
* Database normalization

New ASP.NET concepts learned:

* `AddControllers()`
* `MapControllers()`
* `Ok()`
* `Created()`
* `NotFound()`

## Issues

* Initially duplicated endpoints by leaving Minimal API routes active while adding controller actions.
* Encountered foreign key violations after making `PatientId` required before updating the intake creation workflow.
* Realized the original API design allowed intake requests to be created from only a patient name, which does not accurately model a healthcare intake process.

## Biggest Insights

* Controllers replace the HTTP entry point, not the application's architecture. The Service Layer, Repository Pattern, and Entity Framework remain unchanged.
* Good software models the business domain instead of taking shortcuts that are merely convenient to code.
* An intake request should reference an existing patient rather than creating a relationship from a name string.
* Professional Git workflows use feature branches to isolate work and keep the main branch stable.
* A well-designed architecture allows major framework changes, such as moving from Minimal APIs to Controllers, with very little impact on the business logic.

## Confidence

⭐⭐⭐⭐☆ (4/5)

Still want more practice with:

* Controller routing
* Model binding
* Middleware
* Request lifecycle
* Controller validation
* Converting the remaining endpoints to Controllers


