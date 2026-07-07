# Nullable Reference Types

## What Problem Does This Solve?

One of the most common causes of application crashes is attempting to use an object that doesn't exist.

For example:

```csharp
IntakeRequest request = FindRequestById(id);

request.UpdateStatus(status);
```

What happens if `FindRequestById()` doesn't find a matching request?

It returns `null`.

Calling:

```csharp
request.UpdateStatus(status);
```

would throw a **NullReferenceException** because there is no object to update.

Historically, C# allowed any reference type to be `null`, making these bugs common and difficult to detect.

## Solution

Nullable Reference Types allow the compiler to distinguish between:

- References that should never be `null`
- References that may legitimately be `null`

A question mark (`?`) indicates that a reference may contain `null`.

Example:

```csharp
IntakeRequest? request;
```

Without the question mark:

```csharp
IntakeRequest request;
```

the compiler assumes the variable should always contain an object.

## Why This Matters

Nullable Reference Types:

- Prevent many runtime errors.
- Help the compiler detect possible bugs.
- Make code easier to understand.
- Clearly communicate intent.
- Encourage developers to think about missing data.

Instead of discovering null problems while the application is running, many are caught during compilation.

## Mental Model

Think of the question mark as a warning label.

Without it:

```text
This box always contains something.
```

With it:

```text
This box might be empty.
```

When the compiler sees:

```csharp
IntakeRequest?
```

it reminds you to check before using it.

## Nullable vs Non-Nullable

### Non-nullable

```csharp
IntakeRequest request;
```

The compiler expects this variable to always reference an object.

---

### Nullable

```csharp
IntakeRequest? request;
```

The variable may contain either:

- an `IntakeRequest`
- `null`

The compiler expects you to handle both possibilities.

## Real-World Example

In the Clinic Intake API:

```csharp
IntakeRequest? request =
    await FindRequestByIdAsync(id);
```

The method might not find a request with the supplied ID.

Instead of pretending one always exists, the return type communicates that the result may be `null`.

The caller must handle both cases.

```csharp
return request is not null
    ? Results.Ok(request)
    : Results.NotFound();
```

## Common Nullable Types

### Nullable Reference

```csharp
string? patientName;
```

The string may be `null`.

---

### Nullable Object

```csharp
IntakeRequest? request;
```

The object may not exist.

---

### Nullable Parameters

```csharp
RequestStatus? status

string? patient

string? sort
```

These query parameters are optional.

If the client does not provide them, they become `null`.

## Checking for Null

### Pattern Matching

```csharp
if (request is not null)
{
    request.UpdateStatus(status);
}
```

---

### Equality Check

```csharp
if (request != null)
{
}
```

Both are valid.

Many developers prefer pattern matching because it reads naturally.

## Null Conditional Operator

Sometimes an operation should only occur if an object exists.

```csharp
request?.UpdateStatus(status);
```

This means:

> If `request` exists, call `UpdateStatus()`.

Otherwise, do nothing.

It is equivalent to:

```csharp
if (request is not null)
{
    request.UpdateStatus(status);
}
```

## Null-Coalescing Operator

Provide a default value when something is `null`.

```csharp
string name =
    patientName ?? "Unknown";
```

Read as:

> If `patientName` is `null`, use `"Unknown"`.

## Code Example

Returning a single request:

```csharp
IntakeRequest? request =
    await intakeService.FindRequestByIdAsync(id);

return request is not null
    ? Results.Ok(request)
    : Results.NotFound();
```

The nullable return type forces the endpoint to handle the possibility that no request exists.

## Common Beginner Questions

### Why not just ignore null?

Because attempting to use a `null` reference causes a runtime exception.

Nullable Reference Types help prevent those errors before the application runs.

---

### Does `?` create a different type?

Not really.

It tells the compiler:

> This reference may be `null`.

The runtime object is still the same type.

---

### Why do some variables have `?` and others don't?

Only variables that may legitimately be missing should be nullable.

For example:

```csharp
IntakeRequest request;
```

should always contain an object.

But:

```csharp
IntakeRequest? request;
```

represents the result of a search that may fail.

## Common Mistakes

- Ignoring compiler warnings about possible null references.
- Making every reference nullable.
- Forgetting to check nullable values before using them.
- Assuming `?` changes the runtime type.
- Returning `null` when an object should always exist.

## Interview Answer

Nullable Reference Types allow developers to explicitly indicate whether a reference may contain `null`. This helps the compiler detect potential null reference errors during compilation, improving code safety and making intent clearer.

## One-Sentence Summary

Nullable Reference Types allow C# to distinguish between references that should always contain an object and references that may legitimately be `null`.

## What Finally Made It Click

- `?` doesn't change the object.
- It changes what the compiler knows about the object.
- Nullable Reference Types are really a communication tool between me and the compiler.
- When I see `IntakeRequest?`, I immediately know the object might not exist.
- The compiler isn't being annoying when it warns about possible null values. It's pointing out places where the application could otherwise crash at runtime.
- Using nullable types makes the code's intent much clearer because the possibility of missing data is visible directly in the method signature.

## Related Topics

### Previous

- Classes
- Collections

### Next

- Exceptions
- Async/Await

### See Also

- Interfaces
- Service Layer
- API Responses
- Entity Framework