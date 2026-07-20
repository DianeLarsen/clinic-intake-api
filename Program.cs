using System.Text.Json.Serialization;
using Asp.Versioning;
using ClinicIntakeApi.Authorization;
using ClinicIntakeApi.Configuration;
using ClinicIntakeApi.Data;
using ClinicIntakeApi.Filters;
using ClinicIntakeApi.HealthChecks;
using ClinicIntakeApi.Middleware;
using ClinicIntakeApi.Repositories;
using ClinicIntakeApi.Services;
using ClinicIntakeApi.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;

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
builder.Services.AddSwaggerGen(options =>
{
    //
    // Tell Swagger how JWT authentication works.
    //
    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            // JWTs are sent through the Authorization header.
            Name = "Authorization",

            // HTTP means this uses an HTTP authentication scheme.
            Type = SecuritySchemeType.Http,

            // The HTTP authentication scheme is Bearer.
            Scheme = "bearer",

            // Helps Swagger describe the token format.
            BearerFormat = "JWT",

            In = ParameterLocation.Header,

            Description = "Paste the JWT only. Swagger adds the 'Bearer' prefix.",
        }
    );

    // Examine [Authorize] and [AllowAnonymous] attributes
    // and mark the correct Swagger operations as protected.
    options.OperationFilter<AuthorizeCheckOperationFilter>();
});
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
// Register JWT bearer authentication.
//
// ASP.NET will look for a JWT in this header:
//
// Authorization: Bearer <token>
//
// The JWT handler validates the token and creates
// HttpContext.User from the token's claims.
//
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();

//
// Register authorization.
//
// Authentication determines who the user is.
// Authorization determines what the user may access.
//
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(
        AuthorizationPolicies.ClinicMember,
        policy =>
        {
            // The request must contain a valid authenticated user.
            policy.RequireAuthenticatedUser();

            // The user must have a ClinicId claim containing
            // a positive integer.
            policy.RequireAssertion(context =>
                int.TryParse(
                    context.User.FindFirst(CustomClaimTypes.ClinicId)?.Value,
                    out int clinicId
                )
                && clinicId > 0
            );
        }
    );
});

//
// Load the "Pagination" section from configuration
// into a PaginationOptions object.
//
builder
    .Services.AddOptions<PaginationOptions>()
    .Bind(builder.Configuration.GetSection(PaginationOptions.SectionName))
    // The default must be at least 1.
    .Validate(
        options => options.DefaultPageSize > 0,
        "Pagination:DefaultPageSize must be greater than 0."
    )
    // The maximum cannot be smaller than the default.
    .Validate(
        options => options.MaximumPageSize >= options.DefaultPageSize,
        "Pagination:MaximumPageSize must be greater than or equal to DefaultPageSize."
    )
    // Stop startup immediately if configuration is invalid.
    .ValidateOnStart();

//
// Register Entity Framework Core.
//
// AddDbContext() creates one DbContext per HTTP request (Scoped).
// EF will use SQLite as the database.
//
string connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

//
// Select the Entity Framework database provider from configuration.
//
// Local development uses SQLite by default.
// Azure will later set Database:Provider to SqlServer and provide
// an Azure SQL connection string through environment configuration.
//
string databaseProvider = builder.Configuration["Database:Provider"] ?? "Sqlite";

builder.Services.AddDbContext<ClinicIntakeDbContext>(options =>
{
    if (databaseProvider.Equals("Sqlite", StringComparison.OrdinalIgnoreCase))
    {
        options.UseSqlite(connectionString);
    }
    else if (databaseProvider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
    {
        options.UseSqlServer(
            connectionString,
            sqlServerOptions => sqlServerOptions.EnableRetryOnFailure()
        );
    }
    else
    {
        throw new InvalidOperationException(
            $"Database provider '{databaseProvider}' is not supported."
        );
    }
});

//
// Register application health checks.
//
// This check uses ClinicIntakeDbContext.CanConnectAsync()
// to verify that the API can communicate with its database.
//
builder
    .Services.AddHealthChecks()
    .AddDbContextCheck<ClinicIntakeDbContext>(
        name: "database",
        // Report Unhealthy when the database cannot be reached.
        failureStatus: HealthStatus.Unhealthy,
        // The "ready" tag lets the readiness endpoint
        // select this check.
        tags: ["ready"]
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

// Wrap the remaining pipeline and record the final response,
// including responses produced by exception handling.
app.UseRequestLogging();

// Convert unhandled downstream exceptions into safe
// 500 Internal Server Error responses.
app.UseExceptionHandling();

//
// Identify the user from the request's authentication token.
//
app.UseAuthentication();

//
// Check whether the identified user is allowed
// to access the requested endpoint.
//
app.UseAuthorization();

//
// Liveness check.
//
// Runs no dependency checks. If the API can return this
// response, its process is alive.
//
app.MapHealthChecks(
        "/health/live",
        new HealthCheckOptions
        {
            Predicate = _ => false,

            ResponseWriter = HealthCheckResponseWriter.WriteResponseAsync,
        }
    )
    .AllowAnonymous();

//
// Readiness check.
//
// Runs checks tagged "ready", including the database check.
// A Healthy result means the API is ready to handle work.
//
app.MapHealthChecks(
        "/health/ready",
        new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready"),

            ResponseWriter = HealthCheckResponseWriter.WriteResponseAsync,
        }
    )
    .AllowAnonymous();

app.MapControllers();

//
// Start listening for HTTP requests.
//
// Nothing happens until app.Run() is called.
//
app.Run();

public partial class Program { }
