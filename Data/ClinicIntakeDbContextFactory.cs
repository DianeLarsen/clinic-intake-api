using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ClinicIntakeApi.Data;

//
// This factory is used only by Entity Framework tooling commands
// such as "dotnet ef migrations add".
//
// It lets EF create ClinicIntakeDbContext without starting the API,
// running database seeding, or connecting to Azure SQL.
//
public class ClinicIntakeDbContextFactory
    : IDesignTimeDbContextFactory<ClinicIntakeDbContext>
{
    public ClinicIntakeDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ClinicIntakeDbContext>();

        //
        // EF needs a SQL Server provider and syntactically valid
        // connection string to generate SQL Server migrations.
        //
        // The tooling command does not connect to this server while
        // creating a migration. Azure supplies the real connection
        // string later through environment configuration.
        //
        optionsBuilder.UseSqlServer(
            "Server=localhost;Database=ClinicIntakeApi;"
                + "User Id=sa;Password=NotARealPassword;"
                + "TrustServerCertificate=True"
        );

        return new ClinicIntakeDbContext(optionsBuilder.Options);
    }
}