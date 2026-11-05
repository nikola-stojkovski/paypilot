var builder = WebApplication.CreateBuilder(args);

// Aspire shared defaults: OpenTelemetry, health checks, service discovery, resilience
builder.AddServiceDefaults();

builder.Services.AddOpenApi();

var app = builder.Build();

// /health and /alive endpoints (from ServiceDefaults)
app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/", () => "PayPilot API");

app.Run();
