Here’s a clean cheatsheet version.


# Set Up Testing in a .NET Project

## 1. Create a solution file

Run this from the main project folder:

```bash
dotnet new sln -n ClinicIntakeApi
```

## 2. Add the API project to the solution

```bash
dotnet sln add ClinicIntakeApi.csproj
```

## 3. Create an xUnit test project

```bash
dotnet new xunit -n ClinicIntakeApi.Tests
```

## 4. Add the test project to the solution

```bash
dotnet sln add ClinicIntakeApi.Tests/ClinicIntakeApi.Tests.csproj
```

## 5. Let the test project reference the API project

This lets the tests use classes from the API.

```bash
dotnet add ClinicIntakeApi.Tests/ClinicIntakeApi.Tests.csproj reference ClinicIntakeApi.csproj
```

## 6. If the test project is inside the API folder, exclude it from the API project

Add this to `ClinicIntakeApi.csproj`:

```xml
<ItemGroup>
  <Compile Remove="ClinicIntakeApi.Tests/**/*.cs" />
</ItemGroup>
```

This stops the API project from trying to compile the test files. Because naturally .NET sees a child folder and thinks, “mine.”

## 7. Clean and build

```bash
dotnet clean
dotnet build
```

## 8. Run tests

```bash
dotnet test
```

Expected result:

Passed!


One note for future projects: the cleaner layout is usually:

MySolution/
├── src/
│   └── ClinicIntakeApi/
└── tests/
    └── ClinicIntakeApi.Tests/


But for this project, your current setup works.
