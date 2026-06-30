# HTTP Cheat Sheet

## Common Methods

| Method | Purpose |
|---------|---------|
| GET | Read data |
| POST | Create data |
| PUT | Replace or update |
| PATCH | Partial update |
| DELETE | Delete data |

---

## Status Codes

| Code | Meaning |
|------|---------|
| 200 | OK |
| 201 | Created |
| 204 | No Content |
| 400 | Bad Request |
| 401 | Unauthorized |
| 403 | Forbidden |
| 404 | Not Found |
| 409 | Conflict |
| 500 | Internal Server Error |

---

## Query Parameters

```http
GET /requests?page=2&pageSize=10
```

Multiple parameters

```http
GET /requests?status=Completed&sort=name
```

---

## Route Parameters

```http
GET /requests/5
```

---

## Request Body

```json
{
  "patientName": "Diane"
}
```

---

## Response Body

```json
{
  "id": 5,
  "displayText": "Diane - Submitted"
}
```

---

## Common Results Helpers

```csharp
Results.Ok()

Results.Created()

Results.NoContent()

Results.BadRequest()

Results.NotFound()
```

---

## REST Mapping

| Operation | HTTP |
|-----------|------|
| Read All | GET |
| Read One | GET |
| Create | POST |
| Update | PUT |
| Delete | DELETE |

---

## Mental Model

```text
Client

↓

HTTP Request

↓

API

↓

HTTP Response

↓

Client
```