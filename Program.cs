using ClinicIntakeApi.Repositories;
using ClinicIntakeApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<
    IIntakeRepository,
    InMemoryIntakeRepository>();

builder.Services.AddSingleton<
    IIntakeService,
    IntakeService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet(
    "/requests",
    (IIntakeService intakeService) =>
    {
        return intakeService.GetAllRequests();
    });

using (var scope = app.Services.CreateScope())
{
    IIntakeService intakeService =
        scope.ServiceProvider
            .GetRequiredService<IIntakeService>();

    intakeService.AddRequest("Diane");
    intakeService.AddRequest("Bob");
    intakeService.AddRequest("Alice");
}

app.Run();