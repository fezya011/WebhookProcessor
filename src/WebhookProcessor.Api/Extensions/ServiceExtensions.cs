using Bifrost;
using Bifrost.Autoscaling;
using Bifrost.Core;
using Bifrost.DependencyInjection;
using Bifrost.OpenTelemetry;
using Bifrost.Resilience;
using Microsoft.Extensions.Options;
using WebhookProcessor.Api.Services;
using WebhookProcessor.Core.Handlers;
using WebhookProcessor.Core.Interfaces;
using WebhookProcessor.Core.Jobs;
using WebhookProcessor.Infrastructure.Clients;

namespace WebhookProcessor.Api.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddWebhookService(
    this IServiceCollection services,
    IConfiguration configuration)
    {
        services.AddHttpClient<PhpApiClient>(client =>
        {
            var baseUrl = configuration["PhpApi:BaseUrl"] ?? "https://your-php.com/api/webhooks";
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddSingleton<IPhpApiClient, PhpApiClient>();

        // Настройка оркестратора
        var orchestratorBuilder = services.AddWorkOrchestrator<WebhookJob>(options =>
        {
            options.Capacity = 256;
            options.WorkerCount = 10;
        })
        .WithHandler<WebhookJob, WebhookHandler>()
        .WithResilience(r =>
        {
            r.RetryCount = 3;
            r.UseExponentialBackoff = true;
        })
        .WithDeadLetterQueue()
        .WithOpenTelemetry();

        // Регистрация оркестратора в DI (Build() возвращает void, но регистрирует сервис)
        orchestratorBuilder.Build();  // или .Register(), если метод называется иначе

        // Регистрация WebhookService – оркестратор уже доступен в контейнере
        services.AddScoped<WebhookService>();

        services.AddHealthChecks();

        return services;
    }
}