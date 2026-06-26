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