# Clinic Intake API Cheat Sheet

## Goal

Learn:

-   C#
-   ASP.NET Core
-   REST APIs
-   Dependency Injection
-   Repository Pattern
-   Entity Framework Core
-   Azure

by building a real backend API.

------------------------------------------------------------------------

# Overall Architecture

``` text
HTTP Request
      │
      ▼
Program.cs
(Route)
      │
      ▼
Service
(Business Logic)
      │
      ▼
Repository
(Data Access)
      │
      ▼
Database
(SQLite now → Azure SQL later)
```

Every layer has one responsibility.

------------------------------------------------------------------------

# Folder Structure

``` text
ClinicIntakeApi
│
├── Data
│   └── ClinicIntakeDbContext.cs
│
├── Dtos
│   ├── CreateRequestDto.cs
│   └── UpdateRequestStatusDto.cs
│
├── Models
│   ├── IntakeRequest.cs
│   └── RequestStatus.cs
│
├── Repositories
│   ├── IIntakeRepository.cs
│   └── InMemoryIntakeRepository.cs
│
├── Services
│   ├── IIntakeService.cs
│   └── IntakeService.cs
│
└── Program.cs
```

------------------------------------------------------------------------

# What Each Folder Does

## Models

Represents your application's data.

-   `IntakeRequest`
-   `RequestStatus`

Think: **What information exists?**

## DTOs

Represents data moving into or out of the API.

Examples:

-   `CreateRequestDto`
-   `UpdateRequestStatusDto`

Think: **What information is the client allowed to send or receive?**

## Repositories

Handles data storage.

Current:

``` text
List<IntakeRequest>
```

Future:

``` text
SQLite
↓
Azure SQL
```

Think: **Where is my data stored?**

## Services

Contains business rules.

Examples:

-   `AddRequest()`
-   `UpdateStatus()`
-   `DeleteRequest()`

Think: **What can my application do?**

## Program.cs

Starts the application.

-   Registers services
-   Configures middleware
-   Maps API endpoints

Think: **How does the outside world talk to my application?**

------------------------------------------------------------------------

# Dependency Injection (DI)

Instead of creating objects yourself:

``` csharp
new IntakeService()
```

ASP.NET creates them for you:

``` csharp
builder.Services.AddSingleton<IIntakeService, IntakeService>();
```

You ask for an interface.

ASP.NET provides the implementation.

------------------------------------------------------------------------

# Repository Pattern

Current:

``` text
Service
↓
Repository
↓
List<IntakeRequest>
```

Later:

``` text
Service
↓
Repository
↓
DbContext
↓
SQLite
↓
Azure SQL
```

The Service doesn't care where the data comes from.

------------------------------------------------------------------------

# HTTP Verbs

  Verb     Purpose   Example
  -------- --------- -------------------------
  GET      Read      `/requests`
  POST     Create    `/requests`
  PUT      Update    `/requests/{id}/status`
  DELETE   Remove    `/requests/{id}`

------------------------------------------------------------------------

# HTTP Status Codes

  Code   Meaning
  ------ -----------------------
  200    OK
  201    Created
  204    Success (No Content)
  400    Bad Request
  404    Not Found
  500    Internal Server Error

------------------------------------------------------------------------

# Request Flow

``` text
HTTP Request
    ↓
Program.cs Endpoint
    ↓
Service
    ↓
Repository
    ↓
Database
    ↓
JSON Response
```

------------------------------------------------------------------------

# DTO vs Model

## DTO

Carries data between client and server.

Example:

``` json
{
  "patientName": "Diane"
}
```

## Model

Represents the application's internal object.

``` text
Id
PatientName
Status
```

The server owns IDs and business state.

------------------------------------------------------------------------

# Validation

**Endpoint**

-   Validates HTTP requests
-   Returns `400 Bad Request`

**Service**

-   Validates business rules
-   Protects the application regardless of caller

------------------------------------------------------------------------

# LINQ

Instead of loops:

``` csharp
foreach (...)
```

Use:

``` csharp
.Where(...)
.FirstOrDefault(...)
.Count()
```

Think of LINQ as SQL for C# collections.

------------------------------------------------------------------------

# EF Core

Current:

``` text
Repository → List
```

Next:

``` text
Repository → DbContext → SQLite
```

Later:

``` text
Repository → DbContext → Azure SQL
```

------------------------------------------------------------------------

# Mental Checklist

Whenever you're lost, ask:

1.  What data am I working with? → **Model**
2.  Who is sending the data? → **DTO**
3.  Where is the business logic? → **Service**
4.  Who stores the data? → **Repository**

------------------------------------------------------------------------

# Progress

-   ✅ C# Fundamentals
-   ✅ Interfaces
-   ✅ Dependency Injection
-   ✅ Repository Pattern
-   ✅ REST APIs
-   ✅ Swagger
-   ✅ CRUD Endpoints
-   ✅ Git & GitHub
-   ✅ DTOs
-   ✅ Validation
-   ✅ JSON Serialization
-   🚧 Entity Framework Core
-   ⬜ SQLite
-   ⬜ Migrations
-   ⬜ Azure SQL
-   ⬜ Azure App Service
-   ⬜ Authentication
-   ⬜ Production Deployment
