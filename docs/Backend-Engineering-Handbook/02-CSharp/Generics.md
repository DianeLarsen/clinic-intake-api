# Generics

## What Problem Does This Solve?

Many classes perform the same job regardless of the type of data they contain.

For example:

- A list of patients.
- A list of appointments.
- A list of clinics.

Without generics, you would need to create separate versions of the same class for every type.

Example:

```csharp
public class PatientList
{
    public List<Patient> Items { get; set; }
}

public class AppointmentList
{
    public List<Appointment> Items { get; set; }
}

public class ClinicList
{
    public List<Clinic> Items { get; set; }
}
```

The only thing changing is the type.

This quickly leads to duplicated code.

## Solution

Generics allow a class, method, or interface to work with any type while remaining type-safe.

Instead of writing many versions of the same class, you write one generic version.

Example:

```csharp
public class Box<T>
{
    public T Value { get; set; } = default!;
}
```

Later, the type is supplied:

```csharp
Box<int>

Box<string>

Box<IntakeRequest>
```

The same class works with many different types.

## Why This Matters

Generics make code:

- Reusable
- Type-safe
- Easier to maintain
- Easier to read
- Less repetitive

Most of the .NET libraries rely heavily on generics.

## Mental Model

Think of a generic as a reusable container.

The container stays the same.

Only the contents change.

```text
List<T>

┌──────────────┐
│   Container  │
└──────────────┘
        ▲
        │
        │
   Any Type
```

For example:

```text
List<string>

List<int>

List<IntakeRequest>

List<RequestSummaryDto>
```

The list behaves the same regardless of what it contains.

## Real-World Example

Suppose every API returns paginated data.

Without generics you would need:

```csharp
PatientPagedResponse

AppointmentPagedResponse

ClinicPagedResponse

RequestPagedResponse
```

Instead, one generic class handles every case.

```csharp
public class PagedResponse<T>
{
    public int Page { get; set; }

    public int PageSize { get; set; }

    public int TotalCount { get; set; }

    public int TotalPages { get; set; }

    public IEnumerable<T> Items { get; set; } = [];
}
```

Now the API can return:

```csharp
PagedResponse<RequestSummaryDto>
```

or

```csharp
PagedResponse<AppointmentDto>
```

or

```csharp
PagedResponse<PatientDto>
```

The response structure never changes.

Only the type inside changes.

## Generics You've Already Used

Long before writing your own generic class, you've been using generic classes throughout C#.

### List<T>

```csharp
List<IntakeRequest>
```

A list containing `IntakeRequest` objects.

---

### IEnumerable<T>

```csharp
IEnumerable<RequestSummaryDto>
```

A sequence of `RequestSummaryDto` objects.

---

### Task<T>

```csharp
Task<IntakeRequest>
```

An asynchronous operation that eventually returns an `IntakeRequest`.

---

### DbSet<T>

```csharp
DbSet<IntakeRequest>
```

Represents a database table containing `IntakeRequest` entities.

---

### PagedResponse<T>

```csharp
PagedResponse<RequestSummaryDto>
```

Represents a paginated response containing `RequestSummaryDto` objects.

## Reading Generic Types

Generics become much easier when read like English.

```csharp
List<IntakeRequest>
```

"A list of intake requests."

---

```csharp
Task<IntakeRequest>
```

"An asynchronous operation that returns an intake request."

---

```csharp
IEnumerable<RequestSummaryDto>
```

"A sequence of request summaries."

---

```csharp
PagedResponse<RequestSummaryDto>
```

"A paged response containing request summaries."

Instead of focusing on the angle brackets, read the entire type as one sentence.

## Code Example

Creating a paged response:

```csharp
return new PagedResponse<RequestSummaryDto>
{
    Page = page,
    PageSize = pageSize,
    TotalCount = totalCount,
    TotalPages = totalPages,
    Items = items
};
```

The compiler replaces `T` with `RequestSummaryDto`.

Conceptually, it becomes:

```csharp
PagedResponse<RequestSummaryDto>
{
    ...
    IEnumerable<RequestSummaryDto> Items
}
```

## Common Beginner Questions

### What does `<T>` mean?

`T` stands for **Type**.

It is simply a placeholder that will be replaced later with a real type.

---

### Why use `T` instead of writing `RequestSummaryDto`?

Because tomorrow the same class might hold:

- `PatientDto`
- `AppointmentDto`
- `ClinicDto`

The class doesn't care.

Only the caller decides the type.

---

### Are generics slower?

No.

Generics are compiled into strongly typed code.

They provide flexibility without sacrificing performance.

## Common Mistakes

- Thinking `<T>` is a special data type.
- Creating multiple classes that only differ by type.
- Using `object` when a generic should be used.
- Focusing on the angle brackets instead of the container.

## Interview Answer

Generics allow classes, methods, and interfaces to work with different data types while maintaining compile-time type safety. They eliminate duplicate code by allowing one implementation to work with many types.

## One-Sentence Summary

Generics allow one class, method, or interface to work with many different data types without sacrificing type safety.

## What Finally Made It Click

- Every generic follows the same pattern:

```text
Container<Thing>
```

Examples:

```text
List<IntakeRequest>

Task<IntakeRequest>

IEnumerable<RequestSummaryDto>

DbSet<IntakeRequest>

PagedResponse<RequestSummaryDto>
```

- The container defines **what it does**.
- The type inside defines **what it contains**.
- `<T>` is simply a placeholder for a type that will be supplied later.
- Generics aren't a special feature used occasionally. They're one of the core building blocks of modern C# and appear throughout the .NET libraries.
- Creating `PagedResponse<T>` was the first time I built my own generic class instead of just using someone else's.