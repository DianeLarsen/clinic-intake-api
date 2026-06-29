# Backend Engineering Glossary

## Purpose

This glossary contains concise definitions of technical terms encountered throughout the Backend Engineering Handbook.

Unlike the handbook chapters, which explain concepts in depth, the glossary is designed for quick reference.

Whenever a new technical term appears, it should be added here.

---

# A

## API

**Application Programming Interface.**

A set of rules that allows different software systems to communicate with one another. In this handbook, APIs are typically HTTP web APIs.

---

## API Endpoint

A specific URL that performs one operation within an API.

Example:

```http
GET /requests
```

---

## Asynchronous Programming

A programming model that allows work to continue while waiting for long-running operations, such as database queries or web requests, to complete.

---

# B

## Backend

The server-side portion of an application responsible for business logic, data storage, authentication, and communication with databases and external services.

---

## Business Logic

The rules that determine how an application behaves.

Example:

- Patient names cannot be blank.
- Completed requests cannot return to Submitted.

Business logic belongs in the Service Layer.

---

# C

## Class

A blueprint used to create objects.

A class defines the properties and behaviors that its objects contain.

---

## Constructor

A special method that runs when an object is created.

Constructors are commonly used to receive dependencies through Dependency Injection.

---

## CRUD

The four basic operations performed on data:

- Create
- Read
- Update
- Delete

---

# D

## Database

A system used to permanently store and retrieve information.

Examples include:

- SQLite
- SQL Server
- PostgreSQL
- Azure SQL

---

## DbContext

Entity Framework Core's primary class for communicating with a database.

It manages:

- Database connections
- Queries
- Change tracking
- Saving changes

---

## DbSet

Represents a database table within Entity Framework Core.

A `DbSet<T>` allows LINQ queries and CRUD operations against a specific entity.

---

## Dependency

An object another class requires to perform its work.

Example:

`IntakeService` depends on `IIntakeRepository`.

---

## Dependency Injection (DI)

A design technique where required objects are provided to a class instead of being created inside it.

---

## DTO

**Data Transfer Object.**

An object used to transfer data between layers or systems.

DTOs often prevent exposing database models directly to clients.

---

# E

## Endpoint

A specific HTTP route exposed by an API.

Example:

```http
GET /requests/{id}
```

---

## Entity

A class that represents data stored in the database.

Example:

```csharp
IntakeRequest
```

---

## Entity Framework Core (EF Core)

Microsoft's Object-Relational Mapper (ORM) for .NET.

It allows developers to work with C# objects while EF Core generates SQL automatically.

---

# H

## HTTP

**Hypertext Transfer Protocol.**

The communication protocol used by web browsers and web APIs.

---

## HTTP Status Code

A numeric code returned with every HTTP response indicating the outcome of the request.

Examples:

- 200 OK
- 201 Created
- 204 No Content
- 400 Bad Request
- 404 Not Found

---

# I

## Interface

A contract that defines what a class must implement.

Interfaces allow multiple implementations to share the same public behavior.

---

# L

## Layer

A section of an application's architecture with a specific responsibility.

Examples include:

- Endpoint
- Service
- Repository
- Database

---

## LINQ

**Language Integrated Query.**

C#'s standard query language for working with collections and databases.

---

# M

## Migration

A version-controlled set of database changes created by Entity Framework Core.

Migrations keep the database schema synchronized with the application's models.

---

## Minimal API

A lightweight way to build HTTP endpoints in ASP.NET Core without using controllers.

---

# O

## ORM

**Object-Relational Mapper.**

Software that maps objects in code to tables in a relational database.

Examples:

- Entity Framework Core
- Prisma
- Drizzle

---

# P

## Pagination

The process of dividing a large collection into smaller pages of results.

Usually implemented using:

- `Skip()`
- `Take()`

---

# R

## Repository

A class responsible for retrieving and storing data.

Repositories hide the details of the underlying data source.

---

## REST

**Representational State Transfer.**

An architectural style commonly used when designing HTTP APIs.

---

# S

## Service Layer

The layer responsible for application and business logic.

Services coordinate work between endpoints and repositories.

---

## Scoped

A Dependency Injection lifetime where one instance is created per HTTP request.

---

## Singleton

A Dependency Injection lifetime where one instance is shared for the lifetime of the application.

---

# T

## Task

Represents asynchronous work that will complete in the future.

`Task<T>` eventually returns a value of type `T`.

---

# W

## Where()

A LINQ method used to filter a collection.

Returns only the items that satisfy a condition.