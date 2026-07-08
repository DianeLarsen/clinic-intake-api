# Layers

## What Problem Does This Solve?

As applications grow, putting every responsibility into one class quickly becomes difficult to maintain.

Instead, applications are divided into layers.

Each layer has one primary responsibility and communicates only with the layer directly above or below it.

This separation makes the application easier to understand, test, and modify.

## Why This Matters

Layered architecture provides:

- Clear separation of responsibilities
- Easier testing
- Better maintainability
- Reduced coupling
- Easier replacement of implementations
- More readable code

A layer should not know how every other layer works.

It should only know about the layer immediately below it.

## Mental Model

Think of the application as a chain of responsibility.

```text
HTTP Request
      │
      ▼
Controller
      ▼
Service
      ▼
Repository
      ▼
Entity Framework
      ▼
Database
```

Each layer has a specific job.

## The Layers

### Controller Layer

Responsible for HTTP.

Responsibilities:

- Receive HTTP requests
- Validate input
- Call the Service Layer
- Return HTTP responses

Controllers should not contain business logic.

---

### Service Layer

Responsible for business rules.

Responsibilities:

- Make business decisions
- Coordinate work
- Call repositories
- Validate business rules

Services should not know HTTP details.

---

### Repository Layer

Responsible for data access.

Responsibilities:

- Read data
- Save data
- Delete data
- Hide database details

Repositories should not contain business logic.

---

### Entity Framework

Responsible for translating C# objects into SQL.

Responsibilities:

- Generate SQL
- Track entities
- Execute queries
- Save changes

---

### Database

Responsible for storing data.

Responsibilities:

- Store records
- Enforce constraints
- Execute SQL
- Return results

## Request Flow

```text
Client
   │
   ▼
Controller
   ▼
Service
   ▼
Repository
   ▼
DbContext
   ▼
SQLite
```

Response:

```text
SQLite
   ▲
DbContext
   ▲
Repository
   ▲
Service
   ▲
Controller
   ▲
Client
```

## Example

When a request is made to:

```http
GET /requests/5
```

The application follows this path:

```text
RequestsController
      ▼
IntakeService
      ▼
EfIntakeRepository
      ▼
ClinicIntakeDbContext
      ▼
SQLite
```

The response then returns through the same layers.

## Common Mistakes

- Putting business logic inside controllers.
- Accessing DbContext directly from controllers.
- Having repositories call HTTP APIs.
- Mixing responsibilities between layers.
- Skipping layers because "it's only one query."

## Interview Answer

Layered architecture separates an application into components with different responsibilities. Controllers handle HTTP, services contain business logic, repositories access data, and Entity Framework communicates with the database. This separation improves maintainability, testing, and flexibility.

## One-Sentence Summary

Layered architecture separates responsibilities so each part of the application has one primary job.

## What Finally Made It Click

Every layer answers a different question:

- Controller: "What did the client ask?"
- Service: "What should happen?"
- Repository: "How do I get the data?"
- Entity Framework: "What SQL should I generate?"
- Database: "What data exists?"

Each layer focuses on one responsibility instead of trying to solve every problem.

## Related Topics

### Previous

- Service Layer
- Repository Pattern

### Next

- Dependency Flow
- Controllers