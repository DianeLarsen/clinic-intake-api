using System.Text.Json.Serialization;
using Asp.Versioning;
using ClinicIntakeApi.Data;
using ClinicIntakeApi.Filters;
using ClinicIntakeApi.Middleware;
using ClinicIntakeApi.Repositories;
using ClinicIntakeApi.Services;
using ClinicIntakeApi.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
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
