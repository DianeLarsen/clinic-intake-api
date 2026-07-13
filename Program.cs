using System.Text.Json.Serialization;
using ClinicIntakeApi.Data;
using ClinicIntakeApi.Middleware;
using ClinicIntakeApi.Repositories;
using ClinicIntakeApi.Services;
using Microsoft.EntityFrameworkCore;

// Creates the application builder.
// This is where services and application configuration are registered.
var builder = WebApplication.CreateBuilder(args);

//
// Configure JSON serialization
// Converts enums like RequestStatus.Submitted into
// "Submitted" instead of 0.
//
// Register Swagger/OpenAPI services.
// These generate interactive API documentation.
//
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder
    .Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

//
// Register Entity Framework Core.
//
// AddDbContext() creates one DbContext per HTTP request (Scoped).
// EF will use SQLite as the database.
//
builder.Services.AddDbContext<ClinicIntakeDbContext>(options =>
    options.UseSqlite("Data Source=clinic-intake.db")
);

//
// Dependency Injection registrations.
//
// Whenever something asks for IIntakeRepository,
// create an EfIntakeRepository.
//
// Whenever something asks for IIntakeService,
// create an IntakeService.
//
builder.Services.AddScoped<IIntakeRepository, EfIntakeRepository>();
builder.Services.AddScoped<IIntakeService, IntakeService>();

// Build the application after all services have been registered.
var app = builder.Build();

//
// Only enable Swagger while developing.
//
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//
// Seed development data.
//
await DbSeeder.SeedAsync(app.Services);

//
// Redirect HTTP requests to HTTPS.
//
if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseHttpsRedirection();
}

app.UseExceptionHandling();

//
// Log every incoming request and outgoing response.
//
app.UseRequestLogging();

// app.UseFirstMiddleware();
// app.UseSecondMiddleware();

app.MapControllers();

//
// Start listening for HTTP requests.
//
// Nothing happens until app.Run() is called.
//
app.Run();

public partial class Program { }
