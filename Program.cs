using System.Text.Json.Serialization;
using Asp.Versioning;
using ClinicIntakeApi.Authentication;
using ClinicIntakeApi.Data;
using ClinicIntakeApi.Middleware;
using ClinicIntakeApi.Repositories;
using ClinicIntakeApi.Services;
using ClinicIntakeApi.Versioning;
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

builder
    .Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(ApiVersions.V1, 0);

        options.AssumeDefaultVersionWhenUnspecified = true;

        options.ReportApiVersions = true;
    })
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";

        options.SubstituteApiVersionInUrl = true;
    });

//
// Register authentication.
//
// "Demo" is the name of our authentication scheme.
// When ASP.NET needs to identify a user, it will run
// DemoAuthenticationHandler.
//
builder
    .Services.AddAuthentication("Demo")
    .AddScheme<
        Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions,
        DemoAuthenticationHandler
    >("Demo", options => { });

//
// Register authorization.
//
// Authentication determines who the user is.
// Authorization determines what the user may access.
//
builder.Services.AddAuthorization();

//
// Register Entity Framework Core.
//
// AddDbContext() creates one DbContext per HTTP request (Scoped).
// EF will use SQLite as the database.
//
string connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

builder.Services.AddDbContext<ClinicIntakeDbContext>(options =>
    options.UseSqlite(connectionString)
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

//
// Identify the user from the request's authentication token.
//
app.UseAuthentication();

//
// Check whether the identified user is allowed
// to access the requested endpoint.
//
app.UseAuthorization();

app.MapControllers();

//
// Start listening for HTTP requests.
//
// Nothing happens until app.Run() is called.
//
app.Run();

public partial class Program { }
