# 05 - ASP.NET

## Overview

ASP.NET Core is Microsoft's modern framework for building web applications and APIs.

While C# provides the language and .NET provides the runtime, ASP.NET Core provides the web framework that accepts HTTP requests, executes application code, and returns HTTP responses.

This section focuses on how an ASP.NET Core application starts, how requests move through the application, and how the framework connects all of the pieces together.

---

## Learning Goals

After completing this section, you should understand:

- How an ASP.NET Core application starts
- What `Program.cs` is responsible for
- How configuration settings are loaded
- How dependency injection is configured and used
- How middleware processes every request
- How controllers receive HTTP requests and return responses
- How an HTTP request travels through the application

---

## Topics

### Configuration

Learn how ASP.NET Core loads application settings from files, environment variables, and other configuration providers.

Topics include:

- `appsettings.json`
- Environment variables
- Configuration providers
- Reading configuration values

---

### Controllers

Controllers define API endpoints that receive HTTP requests and return HTTP responses.

Topics include:

- Routing
- Action methods
- Model binding
- Returning responses

---

### Dependency Injection

ASP.NET Core includes a built-in dependency injection container that manages object creation and object lifetimes throughout the application.

Topics include:

- Service registration
- Constructor injection
- Service lifetimes
- Why dependency injection is useful

---

### Middleware

Middleware components form the request processing pipeline.

Each middleware can inspect, modify, or stop a request before it reaches your application code.

Examples include:

- Authentication
- Authorization
- Logging
- Exception handling
- Static files

---

### Logging

Logging records what the API does while it runs and provides evidence when something fails.

Topics include:

- [Logging Fundamentals](Logging.md)
- [Structured Logging](Structured-Logging.md)
- Log levels and filtering
- Request duration and trace IDs
- Exception logging

---

### Program.cs

`Program.cs` is the application's entry point.

It creates the web application, registers services, configures middleware, and starts the web server.

Topics include:

- `WebApplication.CreateBuilder()`
- Registering services
- Building the application
- Mapping endpoints
- Starting the application

---

### Request Lifecycle

Every HTTP request follows the same general path through an ASP.NET Core application.

Understanding this lifecycle makes debugging and designing APIs much easier.

Topics include:

- Request arrival
- Middleware pipeline
- Routing
- Controllers
- Services
- Database access
- Response generation

---

## How These Topics Connect

A typical request follows this sequence:

```
Client
   │
   ▼
ASP.NET Core
   │
   ▼
Middleware Pipeline
   │
   ▼
Routing
   │
   ▼
Controller
   │
   ▼
Service
   │
   ▼
Repository
   │
   ▼
Database
   │
   ▲
Response
```

This section explains the ASP.NET Core pieces of that process before later sections explore databases, Azure deployment, testing, and design patterns.

---

## Next Section

Continue to **06 - Entity Framework**, where you'll learn how ASP.NET Core applications communicate with relational databases using Entity Framework Core.
