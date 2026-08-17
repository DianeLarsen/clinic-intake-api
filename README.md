# Clinic Intake API

A production-deployed REST API for managing clinic intake requests. Built with ASP.NET Core and C# as a long-term backend engineering learning project focused on clean architecture, security, testing, and cloud deployment.

**Live API:** https://clinic-intake-api-dlarsen-2026-dhdmdmesgkgygpbz.westus-01.azurewebsites.net  
**Live health check:** https://clinic-intake-api-dlarsen-2026-dhdmdmesgkgygpbz.westus-01.azurewebsites.net/health/ready

---

## Features

- ASP.NET Core Web API with controllers
- Layered architecture: Controllers → Services → Repositories → EF Core
- JWT authentication and role-based authorization
- Clinic-based data isolation using `ClinicId` claims
- API versioning, validation, filtering, sorting, searching, and pagination
- Structured logging and custom middleware
- Health checks for application liveness and database readiness
- Swagger / OpenAPI in Development
- SQLite for local development and automated tests
- SQL Server / Azure SQL support for Production
- EF Core SQL Server migrations and database seeding
- 47 unit and integration tests
- GitHub Actions CI and deployment to Azure App Service

---

## Architecture

```text
HTTP Request
      ↓
Controllers
      ↓
Services
      ↓
Repositories
      ↓
Entity Framework Core
      ↓
SQLite (local/tests) or Azure SQL (production)
```

---

## Production Deployment

The API is deployed to Azure using:

* Azure App Service on Linux with .NET 8
* Azure SQL Database serverless
* GitHub Actions continuous deployment from `main`
* OpenID Connect (OIDC) authentication—no publish-profile secret stored in GitHub
* App-scoped `Website Contributor` permissions
* SQL Server connection retry handling for temporary Azure SQL availability events
* Health checks:

```text
GET /health/live
GET /health/ready
```

Swagger is intentionally disabled in Production. Anonymous requests to protected API endpoints return `401 Unauthorized`.

See [Azure deployment documentation](docs/Backend-Engineering-Handbook/07-Azure/Deployment.md) for configuration details and deployment notes.

---

## Technologies

* C# / .NET 8
* ASP.NET Core Web API
* Entity Framework Core
* SQLite
* SQL Server / Azure SQL
* JWT Bearer Authentication
* Swagger / OpenAPI
* xUnit integration and unit testing
* GitHub Actions
* Azure App Service

---

## Run locally

### Prerequisites

- .NET 8 SDK
- A JWT signing key configured through .NET user secrets

### Run the API

```bash
git clone https://github.com/DianeLarsen/clinic-intake-api.git
cd clinic-intake-api
dotnet restore
dotnet run --project ClinicIntakeApi
```

## Repository Structure

```text
ClinicIntakeApi
├── Controllers
├── Data
├── Dtos
├── Filters
├── Middleware
├── Models
├── Repositories
├── Services
├── Migrations
├── docs
│   └── Backend-Engineering-Handbook
└── Program.cs
```

---

## Backend Engineering Handbook

This repository includes a companion [Backend Engineering Handbook](docs/Backend-Engineering-Handbook/README.md).

It documents the concepts used to build the API, including C#, LINQ, Entity Framework Core, API design, architecture, ASP.NET Core, Azure, testing, design patterns, and interview preparation.

---

## Post-release backlog

Version 1.0 is complete. Future enhancements are tracked separately as
learning and production-readiness work.

- Filter requests by status and date
- Expand request-status history
- Add Application Insights monitoring and alerts
- Practice safe production schema migrations
- Containerize the application with Docker

---

## Purpose

This project is built incrementally to develop a practical understanding of professional backend engineering. Architectural decisions are documented, tested, and improved as the project evolves.

