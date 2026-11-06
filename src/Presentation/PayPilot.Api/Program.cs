using PayPilot.Infrastructure.Ai;


var builder = WebApplication.CreateBuilder(args);

// Aspire shared defaults: OpenTelemetry, health checks, service discovery, resilience
builder.AddServiceDefaults();
builder.AddPayPilotAi();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// /health and /alive endpoints (from ServiceDefaults)
app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
