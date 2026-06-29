# REST APIs

## What Problem Does This Solve?

Applications need a standard way for clients (websites, mobile apps, and other services) to communicate with a server.

Without a common standard, every API would work differently, making them difficult to learn, use, and integrate with.

## Solution

REST (Representational State Transfer) is a set of conventions for designing web APIs.

It uses HTTP methods to describe the action being performed on a resource.

Resources are represented by nouns:

```text
/requests
/patients
/appointments
```

The HTTP method tells the server what action to perform.

## Why This Matters

* Provides a standard way for applications to communicate.
* Makes APIs predictable and easy to understand.
* Separates the client from the server.
* Allows different applications to use the same API.
* Uses standard HTTP methods and status codes understood by developers worldwide.

## Mental Model

Think of a REST API like ordering food at a restaurant.

* The **menu** is the API.
* The **waiter** is HTTP.
* You place an order using a standard format.
* The kitchen performs the work.
* The waiter returns your food (or tells you something went wrong).

Everyone follows the same process, making it easy to communicate.

## Real-World Example

Your Clinic Intake API exposes resources such as:

```text
/requests
```

Clients can interact with those resources using different HTTP methods:

```http
GET    /requests
POST   /requests
PUT    /requests/5/status
DELETE /requests/5
```

The URL identifies the resource.

The HTTP method identifies the action.

## Code Example

### GET

Retrieve data.

```http
GET /requests
GET /requests/5
```

---

### POST

Create a new resource.

```http
POST /requests
```

```json
{
    "patientName": "Diane"
}
```

---

### PUT

Update an existing resource.

```http
PUT /requests/5/status
```

```json
{
    "status": "Completed"
}
```

---

### DELETE

Delete an existing resource.

```http
DELETE /requests/5
```

## Common Beginner Questions

### Why are resources nouns instead of verbs?

The HTTP method already describes the action.

Good:

```text
GET /requests
POST /requests
DELETE /requests/5
```

Avoid:

```text
GET /getRequests
POST /createRequest
POST /deleteRequest
```

### Why do we need different HTTP methods?

Using standard methods makes every REST API behave consistently.

A developer can usually guess how an API works without reading extensive documentation.

### What is the difference between a URL and an HTTP method?

The URL identifies **what** you are working with.

The HTTP method identifies **what action** you want to perform.

## Common Mistakes

* Using verbs in endpoint names.
* Returning the wrong HTTP status code.
* Using POST for every operation.
* Forgetting to return `404 Not Found` when a resource doesn't exist.

## Common HTTP Status Codes

|    Code | Meaning                                                      |
| ------: | ------------------------------------------------------------ |
| **200** | OK - Request succeeded and returned data.                    |
| **201** | Created - A new resource was successfully created.           |
| **204** | No Content - Request succeeded but nothing is returned.      |
| **400** | Bad Request - The client sent invalid data.                  |
| **404** | Not Found - The requested resource doesn't exist.            |
| **500** | Internal Server Error - An unexpected server error occurred. |

## REST Request Flow

```text
Client

↓

HTTP Request

(GET, POST, PUT, DELETE)

↓

ASP.NET Endpoint

↓

Service

↓

Repository

↓

Database

↓

HTTP Response

(Status Code + JSON)
```

## Interview Answer

REST is a standard architectural style for designing web APIs. It uses HTTP methods such as GET, POST, PUT, and DELETE to perform operations on resources while using HTTP status codes to communicate the result of each request.

## One-Sentence Summary

REST is a standardized way for clients and servers to communicate using HTTP methods and status codes to perform operations on resources.

## What Finally Made It Click

* The URL tells you **what** you're working with.
* The HTTP method tells you **what action** to perform.
* The status code tells the client **what happened**.
