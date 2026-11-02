var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var app = builder.Build();

var instanceId = Guid.NewGuid();

app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    instanceId
}));

app.Run();
