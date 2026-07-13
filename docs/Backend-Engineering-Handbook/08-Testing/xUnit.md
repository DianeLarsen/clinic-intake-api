# xUnit

## What is xUnit?

xUnit is a testing framework for .NET applications.

It allows developers to write small programs called **tests** that automatically verify whether application code behaves correctly.

Instead of manually opening Swagger or clicking buttons every time code changes, xUnit can run hundreds or thousands of tests in seconds.

---

## Why use xUnit?

Testing helps answer questions like:

- Does this method return the correct result?
- Does the code handle invalid data?
- Did a recent change break existing functionality?
- Does the application still behave as expected?

---

## The `[Fact]` Attribute

A method becomes a test when it is marked with the `[Fact]` attribute.

Example:

```csharp
[Fact]
public void Addition_Works()
{
    Assert.Equal(5, 2 + 3);
}
```

Without `[Fact]`, xUnit ignores the method.

---

## Running Tests

Run all tests from the project root:

```bash
dotnet test
```

Example output:

```text
Passed! - Failed: 0
```

---

## Creating a Test Project

### Create a solution

```bash
dotnet new sln -n ClinicIntakeApi
```

### Add the API project

```bash
dotnet sln add ClinicIntakeApi.csproj
```

### Create the test project

```bash
dotnet new xunit -n ClinicIntakeApi.Tests
```

### Add the test project to the solution

```bash
dotnet sln add ClinicIntakeApi.Tests/ClinicIntakeApi.Tests.csproj
```

### Reference the API project

```bash
dotnet add ClinicIntakeApi.Tests/ClinicIntakeApi.Tests.csproj reference ClinicIntakeApi.csproj
```

### Run the tests

```bash
dotnet test
```

---

## Project Structure

```text
ClinicIntakeApi.sln
│
├── ClinicIntakeApi
│
└── ClinicIntakeApi.Tests
```

The API project contains the application code.

The test project contains code that verifies the application behaves correctly.