# Swagger

## What Problem Does This Solve?

As APIs grow, it becomes difficult to remember:

- Which endpoints exist.
- Which HTTP methods they use.
- What parameters they accept.
- What data they return.

Without documentation, developers often have to read the source code or maintain separate API documentation that quickly becomes outdated.

Swagger solves this by generating interactive API documentation directly from the application.

## Solution

Swagger automatically analyzes an ASP.NET Core application and generates documentation for every API endpoint.

Instead of reading source code, developers can open a web page and:

- View every endpoint.
- Read descriptions.
- See parameters.
- Execute requests.
- Inspect responses.

The documentation stays synchronized with the code because it is generated automatically.

## Why This Matters

Swagger makes developing and testing APIs much faster.

It provides:

- Interactive documentation
- Live endpoint testing
- Automatic request generation
- Automatic response examples

This reduces the need for external tools during development.

## Mental Model

Think of Swagger as a live instruction manual for your API.

```text
ASP.NET Core Application
            │
            ▼
        Swagger
            │
            ▼
Interactive Documentation
            │
            ▼
Try Endpoints
```

Instead of guessing how an endpoint works, Swagger already knows.

## How Swagger Works

Two services are registered during application startup.

```csharp
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();
```

Later, Swagger is enabled:

```csharp
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI();
}
```

The first line generates the API specification.

The second provides the web interface.

## Real-World Example

When the Clinic Intake API starts, Swagger displays endpoints such as:

```http
GET /requests

GET /requests/{id}

POST /requests

PUT /requests/{id}/status

DELETE /requests/{id}
```

Each endpoint includes:

- Parameters
- Request body
- Response body
- HTTP status codes

Developers can execute requests directly from the browser.

## Example Workflow

Suppose you want to create a request.

Instead of writing code or using Postman, Swagger allows you to:

1. Open the endpoint.
2. Click **Try it out**.
3. Enter JSON.

```json
{
  "patientName": "Diane"
}
```

4. Click **Execute**.

Swagger sends the HTTP request and displays:

- Request
- Response
- Status Code
- Response Body

Everything happens from the browser.

## OpenAPI

Swagger is the user interface.

OpenAPI is the specification that describes the API.

```text
ASP.NET Core

↓

OpenAPI Document

↓

Swagger UI
```

The application generates an OpenAPI document.

Swagger reads that document and builds the interface.

## Development vs Production

Most applications only expose Swagger during development.

Example:

```csharp
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI();
}
```

Production systems often disable Swagger for security and simplicity.

## Common Beginner Questions

### Is Swagger part of ASP.NET Core?

Not exactly.

Swagger is an external tool that integrates with ASP.NET Core.

ASP.NET Core generates the information.

Swagger displays it.

---

### Do I still need API documentation?

Often yes.

Swagger documents endpoints.

It does not explain business rules or architecture.

---

### Can Swagger send real requests?

Yes.

The requests execute against your running application.

Swagger is a client for your API.

---

### Is Swagger only for testing?

No.

Swagger is used for:

- Documentation
- Testing
- Learning APIs
- Client generation
- Team communication

## Common Mistakes

- Forgetting to register Swagger services.
- Forgetting to call `UseSwagger()`.
- Leaving Swagger enabled in production unnecessarily.
- Assuming Swagger replaces architectural documentation.
- Thinking Swagger creates the API.

## Interview Answer

Swagger is a tool that generates interactive documentation for REST APIs using the OpenAPI specification. It allows developers to view endpoints, test requests, inspect responses, and keep documentation synchronized with the code.

## One-Sentence Summary

Swagger provides automatically generated, interactive documentation for an API.

## What Finally Made It Click

- Swagger doesn't build the API.
- Swagger documents the API.
- The documentation stays synchronized because it is generated from the application.
- Swagger is essentially a browser-based API client.
- Instead of memorizing endpoints or constantly switching to Postman, I could explore and test the entire API directly from the browser.
- OpenAPI describes the API, and Swagger provides the interface for interacting with it.

## Related Topics

### Previous

- REST APIs
- Program.cs

### Next

- Validation
- Controllers

### See Also

- API Responses
- Minimal APIs
- HTTP
- Configuration