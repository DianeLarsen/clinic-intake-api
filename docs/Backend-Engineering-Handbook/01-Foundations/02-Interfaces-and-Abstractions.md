# Interfaces

## What Problem Does This Solve?

Without interfaces, services depend directly on specific implementations. If the implementation changes (for example, from an in-memory repository to SQLite or Azure SQL), the service must also change, making the code tightly coupled and harder to maintain.

## Solution

An interface defines **what** a class must do, but not **how** it does it.

Instead of depending on a concrete class, the application depends on an interface (an abstraction). Different implementations can then be swapped without changing the code that uses them.

## Why This Matters

* Reduces coupling between layers.
* Makes implementations easy to swap.
* Makes testing much easier.
* Allows business logic to stay the same even when storage changes.
* Lets the compiler verify every implementation is complete.

## Mental Model

Think of an interface as a **plug or socket**.

The socket never changes.

Only what gets plugged into it changes.

```
🔌 IIntakeRepository

    ↓

InMemoryIntakeRepository

or

SqliteIntakeRepository

or

AzureSqlRepository
```

## Real-World Example

A computer doesn't care whether a USB keyboard is made by Logitech, Dell, or Microsoft.

It only cares that the keyboard follows the USB standard.

The USB specification is the interface.

Each manufacturer provides a different implementation.

## Code Example

Instead of:

```text
Service
    ↓
InMemoryIntakeRepository
```

Use:

```text
Service
    ↓
IIntakeRepository
    ↑
InMemoryIntakeRepository
```

The service depends on the interface rather than the implementation.

## Common Beginner Questions

### Why create another class if I still have to write it?

The interface usually stays the same.

Only the implementation changes.

Today you may use an in-memory repository.

Tomorrow you may use SQLite.

Later you may use Azure SQL.

The service never has to change.

### Doesn't this just create more code?

Sometimes.

On very small projects, interfaces can feel unnecessary.

As projects grow, they make code easier to change, test, and maintain.

### Isn't an interface basically a checklist?

Yes.

The compiler ensures every implementation provides all of the required methods.

## Common Mistakes

* Creating interfaces when there will only ever be one implementation.
* Confusing an interface with a class.
* Thinking interfaces exist only for testing.
* Forgetting that interfaces describe capabilities, not implementations.

## Interview Answer

Interfaces define a contract that describes what a class can do without specifying how it does it. They reduce coupling, allow implementations to be swapped without changing business logic, and help the compiler ensure every implementation fulfills the required contract.

## One-Sentence Summary

An interface is a contract that defines capabilities instead of implementations, making software easier to change, test, and maintain.

## What Finally Made It Click

* The interface almost never changes.
* The implementations change.
* The compiler acts as my checklist.
