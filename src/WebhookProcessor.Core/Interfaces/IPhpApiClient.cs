namespace WebhookProcessor.Core.Interfaces;

public interface IPhpApiClient
{
    Task<HttpResponseMessage> SendWebhookAsync(
        string clientId,
        string payload,
        CancellationToken ct
    );
}