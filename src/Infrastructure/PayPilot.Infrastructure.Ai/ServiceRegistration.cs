using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace PayPilot.Infrastructure.Ai;

public static class ServiceRegistration
{
    public static IHostApplicationBuilder AddPayPilotAi(this IHostApplicationBuilder builder)
    {
        builder.AddOllamaApiClient("chat")
            .AddChatClient()
            .UseOpenTelemetry(configure: c =>
                c.EnableSensitiveData = builder.Environment.IsDevelopment());

        builder.Services.AddOllamaResilience();

        return builder;
    }

    // Local models on a CPU can take minutes to answer. The standard resilience handler
    // from ServiceDefaults cancels requests after 10 s per attempt and 30 s in total,
    // which aborts the Ollama stream mid-answer. Replace it with longer timeouts.
    // Must be called after AddServiceDefaults(), so this configuration runs last.
    private static IServiceCollection AddOllamaResilience(this IServiceCollection services)
    {
        services.ConfigureHttpClientDefaults(http =>
        {
#pragma warning disable EXTEXP0001 // RemoveAllResilienceHandlers is experimental
            http.RemoveAllResilienceHandlers();
#pragma warning restore EXTEXP0001

            http.AddStandardResilienceHandler(options =>
            {
                options.AttemptTimeout.Timeout = TimeSpan.FromMinutes(5);
                options.TotalRequestTimeout.Timeout = TimeSpan.FromMinutes(10);
                // Must be at least double the attempt timeout to pass options validation
                options.CircuitBreaker.SamplingDuration = TimeSpan.FromMinutes(10);
            });
        });

        return services;
    }
}
