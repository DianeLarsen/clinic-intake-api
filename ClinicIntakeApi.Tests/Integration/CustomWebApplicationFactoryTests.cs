using System.Net;

namespace ClinicIntakeApi.Tests.Integration;

public class CustomWebApplicationFactoryTests
{
    [Fact]
    public async Task CreateClient_CreatesAnIsolatedTemporaryDatabase()
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();

        // The database should not exist until the test server starts
        // and Program.cs runs DbSeeder.SeedAsync().
        Assert.False(File.Exists(factory.DatabasePath));

        // Act
        using HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/health/ready");

        // Assert

        // The API started successfully and can connect to its database.
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // The database was created in the temporary folder rather
        // than using the application's normal clinic-intake.db file.
        Assert.True(File.Exists(factory.DatabasePath));
        Assert.Equal(
            Path.TrimEndingDirectorySeparator(Path.GetTempPath()),
            Path.GetDirectoryName(factory.DatabasePath)
        );
        Assert.NotEqual("clinic-intake.db", Path.GetFileName(factory.DatabasePath));
    }

    [Fact]
    public async Task Dispose_DeletesTheTemporaryDatabase()
    {
        // Arrange
        string databasePath;

        using (var factory = new CustomWebApplicationFactory())
        {
            databasePath = factory.DatabasePath;

            using HttpClient client = factory.CreateClient();

            // Start the test server so startup seeding creates the database.
            HttpResponseMessage response = await client.GetAsync("/health/ready");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(File.Exists(databasePath));
        }

        // The using block disposed the factory, which should remove
        // the temporary SQLite database file.
        Assert.False(File.Exists(databasePath));
    }
}
