using ClinicIntakeApi.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ClinicIntakeApi.Tests.Integration;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    //
    // Each test factory receives its own SQLite database file.
    //
    // Guid.NewGuid() prevents separate test runs from choosing
    // the same file name.
    //
    private readonly string _databasePath = Path.Combine(
        Path.GetTempPath(),
        $"clinic-intake-tests-{Guid.NewGuid():N}.db"
    );

    //
    // Exposes the test database location so integration tests
    // can verify the factory created an isolated database.
    //
    public string DatabasePath => _databasePath;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Make the application use its Testing environment.
        builder.UseEnvironment("Testing");

        // Configure services only inside the test server.
        builder.ConfigureTestServices(services =>
        {
            //
            // Remove the DbContext registration created in Program.cs.
            //
            // The application normally uses clinic-intake.db.
            // Tests must use the temporary file created above instead.
            //
            services.RemoveAll<DbContextOptions<ClinicIntakeDbContext>>();

            //
            // Register ClinicIntakeDbContext again, but point it at this
            // factory's unique temporary SQLite database.
            //
            services.AddDbContext<ClinicIntakeDbContext>(options =>
                options.UseSqlite($"Data Source={_databasePath}")
            );

            // Replace the application's default JWT scheme
            // with our predictable test authentication scheme.
            services
                .AddAuthentication(TestAuthenticationHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(
                    TestAuthenticationHandler.SchemeName,
                    options => { }
                );
        });
    }

    protected override void Dispose(bool disposing)
    {
        //
        // Dispose the test server first so it releases SQLite connections.
        //
        base.Dispose(disposing);

        //
        // Delete the temporary test database after the factory is finished.
        //
        if (disposing && File.Exists(_databasePath))
        {
            File.Delete(_databasePath);
        }
    }

    //
    // Restores this factory's test database to its original
    // empty-and-seeded state.
    //
    // Integration tests can call this before each test method
    // so one test's database changes do not affect another test.
    //
    public async Task ResetDatabaseAsync()
    {
        //
        // Delete all tables and data from this factory's
        // temporary SQLite database.
        //
        using (IServiceScope scope = Services.CreateScope())
        {
            ClinicIntakeDbContext db =
                scope.ServiceProvider.GetRequiredService<ClinicIntakeDbContext>();

            await db.Database.EnsureDeletedAsync();
        }

        //
        // Run the normal application seeder again.
        //
        // DbSeeder creates the database, its tables, and the
        // predictable sample clinics, patients, and requests
        // used by integration tests.
        //
        await DbSeeder.SeedAsync(Services);
    }
}
