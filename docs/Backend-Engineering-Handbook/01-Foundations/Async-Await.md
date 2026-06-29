# Async / Await

## What Problem Does This Solve?

Applications often need to wait for slow operations such as:

- Database queries
- File reads and writes
- Web API calls
- Network requests

While waiting, the CPU usually has nothing to do.

If every request blocks a thread while waiting, the server can quickly run out of available threads under heavy load.

## Solution

Asynchronous programming allows a method to start a slow operation and then give the thread back to the runtime while waiting for the operation to finish.

When the operation completes, execution resumes where it left off.

This allows the server to handle many more requests without needing more threads.

## Why This Matters

- Improves server scalability.
- Prevents threads from sitting idle.
- Allows many requests to be processed concurrently.
- Essential for database, file, and network operations.
- Standard practice in modern ASP.NET applications.

## Mental Model

Imagine a waiter taking your order.

### Synchronous

```text
Take Order
      │
      ▼
Stand beside the grill...
      │
      ▼
Food is ready
      │
      ▼
Serve customer
```

The waiter cannot help anyone else.

### Asynchronous

```text
Take Order
      │
      ▼
Kitchen starts cooking
      │
      ▼
Waiter serves other tables
      │
      ▼
Kitchen rings bell
      │
      ▼
Serve customer
```

The waiter stays productive while the kitchen works.

The database is the kitchen.

The thread is the waiter.

## What is a Task?

A `Task` represents work that will finish in the future.

Think of it as a promise that eventually produces a result.

Instead of returning:

```csharp
IntakeRequest
```

an asynchronous method returns:

```csharp
Task<IntakeRequest>
```

The task says:

> "I don't have the result yet, but I will."

## What Does async Do?

The `async` keyword allows a method to use `await`.

Example:

```csharp
public async Task<IntakeRequest?> FindRequestByIdAsync(int id)
{
    ...
}
```

## What Does await Do?

`await` pauses the current method until the asynchronous operation finishes.

Example:

```csharp
IntakeRequest? request =
    await _repository.GetByIdAsync(id);
```

Think of it as:

> Wait until the work is finished, then continue with the result.

## JavaScript Comparison

| JavaScript | C# |
|------------|----|
| Promise | `Task` |
| `async function` | `async` method |
| `await fetch()` | `await FirstOrDefaultAsync()` |
| `await db.query()` | `await SaveChangesAsync()` |

The concepts are nearly identical.

The biggest difference is why they're used.

JavaScript uses asynchronous programming because many APIs naturally return promises.

C# uses asynchronous programming to avoid blocking server threads while waiting for I/O operations.

## Thread Example

Imagine two users call the API at the same time.

### Without async

```text
Person A
      │
      ▼
Thread waits for database...
      │
      ▼
Person B waits for a free thread
```

### With async

```text
Person A
      │
      ▼
Database starts working
      │
      ▼
Thread returns to the pool
      │
      ▼
Person B can begin immediately
```

The users still wait for the database.

The thread does not.

This allows the server to process many more requests simultaneously.

## Common Beginner Questions

### Does async make my code faster?

Usually no.

The database still takes the same amount of time.

Async improves **throughput**, allowing the server to handle more requests at the same time.

### Do I always use async?

No.

Use async when performing I/O operations such as:

- Database access
- HTTP requests
- File operations

Simple calculations generally do not need async.

### Why does async spread through the application?

If one method returns a `Task`, the calling method must also await it.

That method usually becomes async as well.

This continues upward through the service layer and eventually to the API endpoint.

## Common Mistakes

- Forgetting to use `await`.
- Mixing synchronous and asynchronous database methods.
- Assuming async makes calculations faster.
- Blocking asynchronous code by using `.Result` or `.Wait()`.

## Interview Answer

Asynchronous programming allows an application to perform slow I/O operations without blocking server threads. Instead of waiting idly for databases, files, or web services, the runtime can use those threads to process other requests, improving scalability and throughput.

## One-Sentence Summary

Async and await allow applications to wait for slow operations without blocking threads, making web applications more scalable while keeping code readable.

## What Finally Made It Click

- `Task<T>` is C#'s version of a JavaScript `Promise<T>`.
- `await` doesn't make an operation asynchronous; it waits for an operation that is **already asynchronous** to finish.
- The user still waits for the database query to complete.
- The database still takes the same amount of time to execute the query.
- The thread does **not** wait idle while the database works. It returns to the thread pool to help process other requests.
- Async improves **server scalability**, not the speed of individual database queries.
- There are three independent parts involved in a request:
  - The client waits for a response.
  - The database performs the work.
  - The ASP.NET thread coordinates between them.
- Async frees the coordinator (the thread) while the worker (the database) is busy.
- There are two different bottlenecks:
  - **Server threads** → improved by async.
  - **Database performance** → improved by better queries, indexing, caching, or faster hardware.
- The goal of async is to increase **throughput** (how many requests the server can handle), not necessarily reduce **response time** for a single request.