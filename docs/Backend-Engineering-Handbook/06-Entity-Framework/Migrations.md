# Migrations

## What Problem Does This Solve?

Databases change over time.

At first, the application may only need an `IntakeRequests` table. Later, it may need new columns, new tables, indexes, relationships, or constraints.

Without migrations, database changes would have to be tracked manually, making it easy to forget what changed or accidentally update environments differently.

## Solution

Entity Framework Core migrations track database schema changes in code.

A migration describes how to move the database from one structure to another.

For example:

```text
No database

↓

InitialCreate migration

↓

Database with IntakeRequests table
```

Later:

```text
Database with IntakeRequests table

↓

AddEmail migration

↓

Database with new Email column
```

## Why This Matters

* Tracks database changes over time.
* Keeps schema changes version-controlled.
* Makes local, test, and production databases easier to keep in sync.
* Allows EF Core to create or update the database from code.
* Reduces the need to manually write SQL for basic schema changes.

## Mental Model

Think of migrations as **Git commits for your database schema**.

Git tracks how your code changes.

Migrations track how your database structure changes.

```text
Git commit

↓

Code changed
```

```text
EF migration

↓

Database schema changed
```

## Real-World Example

In the Clinic Intake API, the first migration created the initial database structure.

It created tables like:

```text
IntakeRequests
_EFMigrationsHistory
```

The `IntakeRequests` table came from the `IntakeRequest` model.

The `_EFMigrationsHistory` table is used by EF Core to track which migrations have already been applied.

## Code Example

Create a migration:

```bash
dotnet ef migrations add InitialCreate
```

Apply the migration to the database:

```bash
dotnet ef database update
```

This creates or updates the SQLite database based on the current migrations.

A migration file may contain code like:

```csharp
migrationBuilder.CreateTable(
    name: "IntakeRequests",
    columns: table => new
    {
        Id = table.Column<int>(nullable: false),
        PatientName = table.Column<string>(nullable: false),
        Status = table.Column<int>(nullable: false)
    },
    constraints: table =>
    {
        table.PrimaryKey("PK_IntakeRequests", x => x.Id);
    });
```

This C# code describes the database table EF Core should create.

## Common Beginner Questions

### Is a migration the database?

No.

A migration is not the database.

A migration is a set of instructions for creating or changing the database schema.

### What does `dotnet ef migrations add` do?

It creates a new migration file based on the difference between the current model and the last known database schema.

### What does `dotnet ef database update` do?

It applies pending migrations to the actual database.

In this project, it created or updated:

```text
clinic-intake.db
```

### What is `_EFMigrationsHistory`?

`_EFMigrationsHistory` is a table EF Core creates to remember which migrations have already been applied.

This prevents EF Core from running the same migration over and over.

### Do migrations store data?

No.

Migrations usually describe database structure, not application data.

They create or modify things like:

* Tables
* Columns
* Keys
* Relationships
* Indexes

### Should I edit migration files manually?

Usually no.

Most of the time, EF Core generates migrations and developers review them.

Manual edits are possible, but should be done carefully because migrations affect the database structure.

## Common Mistakes

* Forgetting to run `dotnet ef database update`.
* Thinking `migrations add` updates the database automatically.
* Deleting migration files without understanding the database state.
* Editing the model but forgetting to create a new migration.
* Confusing schema changes with data changes.

## Interview Answer

Entity Framework Core migrations are version-controlled database schema changes. They allow developers to define how the database should evolve over time and apply those changes consistently across environments.

## One-Sentence Summary

Migrations are version-controlled instructions that tell EF Core how to create or update the database schema.

## What Finally Made It Click

* Migrations are like Git commits for the database schema.
* `migrations add` creates the instructions.
* `database update` applies the instructions.
* `_EFMigrationsHistory` tracks what has already been applied.

## Development Notes

### Reset the Database (Development Only)

Sometimes it is easier to start with a clean database while developing.

1. Stop the application.
2. Delete the SQLite database file:

```bash
rm clinic-intake.db
```

3. Recreate the database from the existing migrations:

```bash
dotnet ef database update
```

This creates a fresh database using the current migration history.

> **Note:** Only do this during development. Never delete a production database to apply changes.

---

### Create a New Migration

After changing your models:

```bash
dotnet ef migrations add <MigrationName>
```

Example:

```bash
dotnet ef migrations add AddEmailToIntakeRequest
```

---

### Apply Pending Migrations

```bash
dotnet ef database update
```

This updates the database schema to match the latest migration.

---

### Useful Commands

List installed packages:

```bash
dotnet list package
```

Check EF CLI version:

```bash
dotnet ef --version
```

List migrations:

```bash
dotnet ef migrations list
```

Remove the most recent migration (if it has not been applied):

```bash
dotnet ef migrations remove
```


Create Migration
dotnet ef migrations add InitialCreate

Apply Migration
dotnet ef database update

List Migrations
dotnet ef migrations list

Remove Last Migration
dotnet ef migrations remove

Reset Database (Development)
rm clinic-intake.db
dotnet ef database update