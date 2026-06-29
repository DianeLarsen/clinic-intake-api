````markdown
# Searching

## What Problem Does This Solve?

Sometimes users don't know the exact record they're looking for.

For example, a nurse may remember that a patient's name contains "dia", but not know whether it is:

- Diane
- Diana
- Dianna

Without searching, the user would need to retrieve every record and manually look through them.

## Solution

Searching allows clients to find records that partially match a value.

Instead of requiring an exact match, the API searches for records containing the supplied text.

Example:

```http
GET /requests?patient=dia
```

The API returns any matching requests.

## Where It Fits

Searching is part of the API design.

The request flows through the application like this:

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
```

The endpoint receives the search term.

The service builds the query.

The repository retrieves the matching records.

## Why This Matters

- Helps users quickly find records.
- Reduces unnecessary data transfer.
- Improves the user experience.
- Allows one endpoint to support many search scenarios.
- Works well with filtering, sorting, and pagination.

## Mental Model

Think of searching like using the search box in your email.

Instead of reading every email, you type:

```text
invoice
```

The email program returns only messages containing that word.

Searching works the same way in an API.

## Real-World Example

A clinic receptionist remembers a patient named "Charlie" but only types:

```text
char
```

The dashboard requests:

```http
GET /requests?patient=char
```

The API returns:

- Charlie
- Charlotte

The receptionist doesn't need to know the exact spelling.

## Code Example

The endpoint accepts an optional search parameter:

```csharp
app.MapGet(
    "/requests",
    async (
        IIntakeService intakeService,
        string? patient
    ) =>
    {
        ...
    }
);
```

The service applies the search:

```csharp
if (!string.IsNullOrWhiteSpace(patient))
{
    requests = requests.Where(r =>
        r.PatientName.Contains(
            patient,
            StringComparison.OrdinalIgnoreCase));
}
```

The `Contains()` method checks whether the patient's name includes the search text.

`StringComparison.OrdinalIgnoreCase` makes the search case-insensitive.

So these searches all return the same results:

```text
char
CHAR
Char
```

## Searching vs Filtering

Searching and filtering are similar but solve different problems.

Filtering looks for an exact value.

Example:

```http
GET /requests?status=Completed
```

Searching looks for a partial match.

Example:

```http
GET /requests?patient=char
```

Think of it this way:

```text
Filtering:
Status equals Completed

Searching:
Patient name contains "char"
```

## Combining Searches

Searching can be combined with filters.

Example:

```http
GET /requests?status=Submitted&patient=dia
```

The service builds the query one step at a time:

```csharp
if (status is not null)
{
    requests = requests.Where(r =>
        r.Status == status.Value);
}

if (!string.IsNullOrWhiteSpace(patient))
{
    requests = requests.Where(r =>
        r.PatientName.Contains(
            patient,
            StringComparison.OrdinalIgnoreCase));
}
```

Each condition narrows the results further.

## Common Beginner Questions

### Why use `Contains()`?

`Contains()` performs a partial text search.

Example:

```text
Search: "ann"

Matches:
Anna
Annabelle
Joanne
```

### Why use `OrdinalIgnoreCase`?

Without it:

```text
char
```

would not match:

```text
Charlie
```

Using `StringComparison.OrdinalIgnoreCase` makes searches case-insensitive.

### Does searching replace filtering?

No.

Filtering and searching solve different problems and are commonly used together.

## Common Mistakes

- Using an exact comparison (`==`) when a partial search is needed.
- Making searches case-sensitive without a good reason.
- Putting search logic inside the endpoint.
- Creating separate endpoints for every searchable field.
- Searching after pagination instead of before building the full query.

## Interview Answer

Searching allows clients to retrieve records that partially match a value, typically using LINQ's `Contains()` method. It improves usability by allowing users to find records without knowing the exact value while keeping the API flexible and efficient.

## One-Sentence Summary

Searching finds records that partially match user input, making APIs easier and more efficient to use.

## What Finally Made It Click

- Searching answers the question, **"Can I find records containing this text?"**
- Searching usually uses `Contains()`.
- Searches are typically case-insensitive.
- Searching is different from filtering because it looks for partial matches instead of exact values.
- Searching works best when combined with filtering, sorting, and pagination.
- The service layer builds the search query before returning the results.
````
