# Repository Pattern

## What Problem Does This Solve?

The application needs to get data from somewhere.

At first, the data came from an in-memory `List<IntakeRequest>`. Later, the data came from SQLite through Entity Framework Core. Eventually, it may come from Azure SQL.

Without the Repository Pattern, the service layer would need to know exactly how data is stored and retrieved. That would tightly couple business logic to a specific storage technology, which is how applications become sad little dependency swamp creatures.

## Solution

A repository acts as a dedicated data access layer.

The service asks the repository for data.

The repository handles the details of where the data comes from.

```text
Service
    ↓
Repository
    ↓
Data Source
````

The data source can change without forcing the service to change.

## Where It Fits

In this project, the repository sits between the service layer and the database.

```text
Endpoint
    ↓
Service
    ↓
Repository
    ↓
DbContext
    ↓
SQLite Database
```

The endpoint does not talk directly to the database.

The service does not talk directly to Entity Framework.

The repository handles data access.

## Why This Matters

* Keeps database logic out of the service layer.
* Makes storage implementations easier to swap.
* Supports Dependency Inversion.
* Makes testing easier.
* Keeps business logic focused on business rules.
* Makes future database changes less painful.

## Mental Model

Think of a repository as a storage clerk.

The service says:

```text
Get request 5.
```

The repository figures out how to get it.

Today, the repository might look in a list.

Tomorrow, it might query SQLite.

Later, it might query Azure SQL.

The service does not care. It just asks for data.

## Real-World Example

Imagine a hospital front desk needs a patient chart.

The nurse does not need to know whether the chart is:

* In a paper filing cabinet
* In Epic
* In Cerner
* In an archive system

The nurse asks for the chart.

The records system retrieves it.

The repository is like the records system.

## Code Example

The interface defines what the repository must do:

```csharp
public interface IIntakeRepository
{
    Task<IntakeRequest> AddAsync(IntakeRequest request);

    Task<IEnumerable<IntakeRequest>> GetAllAsync();

    Task<IntakeRequest?> GetByIdAsync(int id);

    Task<bool> DeleteAsync(int id);
}
```

The in-memory implementation stores data in a list:

```csharp
public class InMemoryIntakeRepository : IIntakeRepository
{
    private readonly List<IntakeRequest> _requests = [];

    public Task<IEnumerable<IntakeRequest>> GetAllAsync()
    {
        return Task.FromResult<IEnumerable<IntakeRequest>>(_requests);
    }
}
```

The Entity Framework implementation stores data in SQLite:

```csharp
public class EfIntakeRepository : IIntakeRepository
{
    private readonly ClinicIntakeDbContext _db;

    public EfIntakeRepository(ClinicIntakeDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<IntakeRequest>> GetAllAsync()
    {
        return await _db.IntakeRequests.ToListAsync();
    }
}
```

Both implementations follow the same interface.

The service depends on `IIntakeRepository`, not a specific repository class.

## Common Beginner Questions

### Is a repository the same as a database?

No.

A repository is not the database.

A repository is a class that hides the details of how the application accesses data.

### Why not use Entity Framework directly in the service?

You can, especially in small applications.

But using a repository keeps data access separate from business logic. This makes the service easier to test and makes storage changes easier later.

### Is the repository pattern always necessary?

No.

For very small apps, it can feel like extra ceremony.

For larger applications, or applications where the data source might change, it helps keep the architecture cleaner.

### Why use an interface with the repository?

The interface allows the service to depend on the repository contract instead of one specific implementation.

That makes this possible:

```text
IIntakeRepository
    ↑
InMemoryIntakeRepository

IIntakeRepository
    ↑
EfIntakeRepository
```

The service does not need to change when the implementation changes.

## Common Mistakes

* Putting business logic inside the repository.
* Returning database-specific details from the repository.
* Making the repository responsible for HTTP responses.
* Creating too many repository methods for every tiny query variation.
* Forgetting that the repository should hide storage details.

## Interview Answer

The Repository Pattern separates data access logic from business logic. A repository provides methods for retrieving and saving data while hiding the details of the underlying storage system, such as an in-memory list, SQLite, or Azure SQL. This improves maintainability, testability, and flexibility.

## One-Sentence Summary

The Repository Pattern creates a dedicated data access layer so the rest of the application does not need to know how or where data is stored.

## What Finally Made It Click

* The service asks for data.
* The repository knows how to get the data.
* The service should not care whether data comes from a list, SQLite, or Azure SQL.
* The repository is where storage details belong.
* Interfaces make it possible to swap repository implementations without changing the service.


