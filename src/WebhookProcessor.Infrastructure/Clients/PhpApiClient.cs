using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;
using System.Text;
using System.Text.Json;
using WebhookProcessor.Core.Interfaces;

namespace WebhookProcessor.Infrastructure.Clients;

public class PhpApiClient : IPhpApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PhpApiClient> _logger;

    public PhpApiClient(HttpClient httpClient, ILogger<PhpApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<HttpResponseMessage> SendWebhookAsync(string clientId, string payload, CancellationToken ct)
    {
        try
        {
            _logger.LogInformation("Sending in PHP: ClientId={ClientId}", clientId);

            var content = new StringContent(
                JsonSerializer.Serialize(new { ClientId = clientId, Payload = payload }),
                Encoding.UTF8,
                "application/json"
            );

            content.Headers.Add("X-Client-Id", clientId);
            content.Headers.Add("X-Request-Id", Guid.NewGuid().ToString());

            var response = await _httpClient.PostAsync("/api/webhook", content, ct);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully sent to PHP: ClientId={ClientId}", clientId);
            }
            else
            {
                _logger.LogWarning("PHP returned an error {StatusCode}: {ClientId}", response.StatusCode, clientId);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PHP sending error: {ClientId}", clientId);
            throw new InvalidOperationException($"PHP service call failed for client {clientId}", ex);
        }
    }
}
