````markdown
# API Responses

## What Problem Does This Solve?

When a client calls an API, it needs more than just data.

The client also needs to know:

- Did the request succeed?
- Was the resource created?
- Was something not found?
- Was the request invalid?
- Is there additional information, such as pagination?

Without consistent responses, every endpoint would behave differently, making the API difficult to use.

## Solution

An API response combines:

- An HTTP status code
- An optional response body

The status code tells the client **what happened**.

The response body provides **the data or additional information**.

Example:

```http
GET /requests/5
```

Response:

```http
200 OK
```

```json
{
    "id": 5,
    "patientName": "Diane",
    "status": "Submitted"
}
```

## Where It Fits

API responses are created by the endpoint.

```text
Client
    ↓
Endpoint
    ↓
Service
    ↓
Repository
    ↓
Database
         ↑
Repository
    ↑
Service
    ↑
Endpoint
    ↓
HTTP Response
```

The service returns data.

The endpoint decides which HTTP response to send.

## Why This Matters

- Gives clients consistent behavior.
- Clearly communicates success or failure.
- Makes APIs easier to consume.
- Supports proper HTTP standards.
- Makes debugging much easier.

## Mental Model

Think of ordering a package online.

There are two pieces of information.

The shipping status:

```text
Delivered
```

and the package itself.

The status tells you what happened.

The package contains the information you wanted.

An API response works the same way.

```text
Status Code
+
Response Body
```

## Real-World Example

Suppose a client requests:

```http
GET /requests/999
```

Request 999 doesn't exist.

The endpoint returns:

```http
404 Not Found
```

No data is returned because the resource does not exist.

If the request exists:

```http
200 OK
```

```json
{
    "id": 5,
    "patientName": "Diane",
    "status": "Submitted"
}
```

The status tells the client the request succeeded.

The body contains the data.

## Code Example

Successful request:

```csharp
return Results.Ok(request);
```

Created resource:

```csharp
return Results.Created(
    $"/api/v1/requests/{request.Id}",
    request);
```

Missing resource:

```csharp
return Results.NotFound();
```

Invalid input:

```csharp
return Results.BadRequest(
    "Patient name is required.");
```

No content:

```csharp
return Results.NoContent();
```

Each helper creates the correct HTTP response.

## Response Bodies

Not every response returns data.

### 200 OK

Usually returns data.

```json
{
    "id": 5,
    "patientName": "Diane"
}
```

### 201 Created

Usually returns the newly created resource.

```json
{
    "id": 21,
    "patientName": "Emma"
}
```

### 204 No Content

Returns no response body.

The operation succeeded, but there is nothing to send back.

### 404 Not Found

May or may not include an error message.

### 400 Bad Request

Often includes a message explaining what was wrong.

## Response DTOs

The response body should not always contain the full database model.

Instead, APIs often return Response DTOs.

Example:

```json
{
    "id": 5,
    "displayText": "Diane - Submitted"
}
```

instead of

```json
{
    "id": 5,
    "patientName": "Diane",
    "status": "Submitted"
}
```

Response DTOs expose only the information the client needs.

## Collection Responses

Collections often return arrays.

Example:

```json
[
    {
        "id": 1,
        "displayText": "Alice - Submitted"
    },
    {
        "id": 2,
        "displayText": "Bob - Completed"
    }
]
```

As applications grow, collections are often wrapped in a response object.

Example:

```json
{
    "page": 2,
    "pageSize": 10,
    "totalCount": 57,
    "totalPages": 6,
    "items": [
        ...
    ]
}
```

This provides metadata that helps clients build pagination controls.

## Common Beginner Questions

### Does every response return JSON?

No.

Some responses only return a status code.

Example:

```http
204 No Content
```

No response body is sent.

### Why not always return 200?

Because HTTP status codes communicate what happened.

Returning:

```http
404 Not Found
```

is much more useful than returning:

```http
200 OK
```

with an error message inside the JSON.

### Why use `Results.*`?

`Results.Ok()`, `Results.NotFound()`, and similar methods make it easy to create standard HTTP responses in Minimal APIs.

## Common Mistakes

- Returning `200 OK` for every request.
- Returning database models instead of DTOs.
- Returning inconsistent response shapes.
- Putting HTTP response logic inside the service layer.
- Forgetting that the endpoint is responsible for creating the HTTP response.

## Interview Answer

An API response combines an HTTP status code with an optional response body. The status code communicates whether the request succeeded or failed, while the response body contains the requested data or additional information such as error messages or pagination metadata. Consistent API responses make applications easier to consume and maintain.

## One-Sentence Summary

API responses communicate the outcome of a request through HTTP status codes and, when appropriate, return data in a consistent response body.

## What Finally Made It Click

- An API response has two parts:
  - The HTTP status code.
  - The response body.
- The service returns data.
- The endpoint decides the HTTP response.
- Status codes explain **what happened**.
- The response body contains **what the client needs**.
- Response DTOs keep APIs focused on exposing only the necessary data.
- As APIs grow, response bodies often include metadata such as pagination information alongside the returned items.
````
