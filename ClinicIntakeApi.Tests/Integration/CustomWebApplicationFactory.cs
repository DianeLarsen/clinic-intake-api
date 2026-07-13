using ClinicIntakeApi.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ClinicIntakeApi.Tests.Integration;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Find the normal DbContext registration.
            ServiceDescriptor? descriptor = services.SingleOrDefault(service =>
                service.ServiceType == typeof(DbContextOptions<ClinicIntakeDbContext>)
            );

            // Remove the normal database registration.
            if (descriptor is not null)
            {
                services.Remove(descriptor);
            }

            // Register a separate SQLite database for tests.
            services.AddDbContext<ClinicIntakeDbContext>(options =>
                options.UseSqlite("Data Source=clinic-intake-test.db")
            );
        });
    }
}
