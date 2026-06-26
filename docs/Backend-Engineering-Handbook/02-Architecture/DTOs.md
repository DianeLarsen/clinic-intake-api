DTOs are one of those concepts that seem unnecessary until you've built a few APIs. Then you start wondering how you ever lived without them.

The key idea is this:

> **A DTO controls what crosses the boundary of your API.**

Not what your application stores.
Not what your database looks like.

Just what is sent **into** or **out of** your API.

---

# Data Transfer Objects (DTOs)

## What Problem Does This Solve?

Without DTOs, the API exposes its internal models directly.

This causes several problems:

* Clients can send data they shouldn't be allowed to modify.
* Internal implementation details become public.
* Changes to database models can break API clients.
* Different endpoints often need different pieces of data.

## Solution

A **DTO (Data Transfer Object)** is a class used only for transferring data between systems.

Instead of sending your model directly, the API sends or receives a DTO.

## Why This Matters

* Protects internal models.
* Controls exactly what data is exposed.
* Prevents clients from changing fields they shouldn't.
* Makes APIs easier to evolve over time.
* Allows different endpoints to return different data.

---

## Mental Model

Think of a DTO as a **shipping box**.

The object inside your application is the actual product.

The DTO is the box you ship to someone else.

You choose what goes inside the box.

```text
Database Model

↓

Service

↓

DTO

↓

JSON

↓

Client
```

The client never sees the internal model directly.

---

## Real-World Example

Imagine a hospital employee record.

Internally it contains:

```text
Employee

Id
Name
Salary
Social Security Number
Date of Birth
Password Hash
```

If your API returned that object directly...

😬 Oops.

Instead you create:

```text
EmployeeDto

Id
Name
Department
```

The sensitive information never leaves the server.

---

## Code Example

Model:

```csharp
public class IntakeRequest
{
    public int Id { get; set; }

    public string PatientName { get; set; } = "";

    public RequestStatus Status { get; set; }
}
```

Request DTO:

```csharp
public class CreateIntakeRequestDto
{
    public string PatientName { get; set; } = "";
}
```

Response DTO:

```csharp
public class IntakeRequestDto
{
    public int Id { get; set; }

    public string PatientName { get; set; } = "";

    public string Status { get; set; } = "";
}
```

Notice the client never sends an Id.

The server creates it.

---

## Request vs Response DTOs

Most applications have both.

### Request DTO

Data coming **into** the API.

```json
{
    "patientName": "Diane"
}
```

---

### Response DTO

Data sent **back** to the client.

```json
{
    "id": 1,
    "patientName": "Diane",
    "status": "Submitted"
}
```

Different endpoints often use different DTOs.

---

## Common Beginner Questions

### Why not just use my model?

Because your model represents your application's internal state.

Your API shouldn't expose or trust that directly.

---

### Why make another class?

Because the client almost never needs every property.

DTOs let you expose only what is appropriate.

---

### Can a DTO be different from the model?

Absolutely.

In fact, that's one of the biggest reasons they exist.

For example:

Model:

```text
FirstName
LastName
```

DTO:

```text
FullName
```

The DTO doesn't have to match the model exactly.

---

## Common Mistakes

* Returning entity models directly from the API.
* Reusing one DTO for every endpoint.
* Putting business logic inside DTOs.
* Treating DTOs as database models.

---

## Connections

Related Concepts:

* REST APIs
* Entity Framework Core
* Models
* JSON Serialization

Used Later In:

* AutoMapper (optional)
* Validation
* API Versioning
* Authentication

---

## Interview Answer

A DTO (Data Transfer Object) is an object used to transfer data between layers or systems. It helps control what data is exposed, prevents clients from modifying internal models, and allows APIs to evolve independently of the database schema.

---

## One-Sentence Summary

A DTO is a lightweight object that controls the data sent into and out of an API, protecting internal models and simplifying communication between systems.

---

## What Finally Made It Click

* Models represent the application's data.
* DTOs represent the API's data.
* They are **not** the same thing.
* A DTO is the package you hand to the outside world.

---

## One thing I'd add because of your background

I think this analogy will stick with you.

Imagine you're working on a defibrillator at Stryker.

Inside the device there are hundreds of internal values:

* Battery voltage
* Capacitor charge
* Calibration values
* Manufacturing settings
* Error logs
* Internal serial numbers

When the defibrillator sends information to LIFENET, it **doesn't send everything**.

It sends only the information that system needs.

That's essentially a DTO.

The internal device state is the **model**.

The carefully selected data sent outside the device is the **DTO**.

That distinction is why DTOs exist. They aren't just "go-betweens." They're a deliberate boundary that protects your application's internals while exposing exactly the information another system needs.
