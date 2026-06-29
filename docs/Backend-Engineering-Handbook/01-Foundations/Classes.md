# Classes

## What Problem Does This Solve?

Related information used to be stored in separate variables and functions, making it difficult to keep data together and increasing the chance of bugs.

## Solution

A class groups related data and behavior into a single object.

## Why This Matters

- Keeps related data together
- Models real-world concepts
- Reduces bugs
- Makes code easier to maintain
- Provides type safety

## Mental Model

A class is a **blueprint**.

An object is the **house built from the blueprint**.

## Real-World Example

A defibrillator has:

- Serial Number
- Battery Level
- Firmware Version

It also knows how to:

- Charge
- Self-Test
- Deliver Shock

The data and behavior belong together.

## Code Example

Without:

ids[]
names[]
statuses[]

With:

IntakeRequest

## Common Beginner Questions

### Why not just use variables?

Variables don't express that the data belongs together.

### Why not use a Dictionary?

You lose type safety and IntelliSense.

## Common Mistakes

- Creating classes that only hold unrelated data.
- Putting unrelated responsibilities into one class.
- Thinking classes exist only for organization.

## Interview Answer

Classes model real-world entities by grouping related data and behavior together, improving readability, maintainability, and type safety.

## One-Sentence Summary

A class is a blueprint that combines related data and behavior into a reusable type.

## What Finally Made It Click

- A class isn't just organization.
- It models a real-world thing.
- Behavior belongs with the data.