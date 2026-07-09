using Bifrost.Core; // Пространство имен для IWorkHandler
using Microsoft.Extensions.Logging;
using WebhookProcessor.Core.Interfaces; // Интерфейс клиента для PHP
using WebhookProcessor.Core.Jobs;

namespace WebhookProcessor.Core.Handlers;

public class WebhookHandler(
    IPhpApiClient phpClient,
    ILogger<WebhookHandler> logger) : IWorkHandler<WebhookJob>
{
    public async ValueTask HandleAsync(WebhookJob job, CancellationToken ct)
    {
        // 1. Здесь мы можем обогатить данные (например, получить информацию о клиенте из БД)
        logger.LogInformation($"Processing webhook for Client: {job.ClientId}");

        // 2. Отправляем запрос в PHP
        var responce = await phpClient.SendWebhookAsync(job.ClientId, job.Payload, ct);

        if (!responce.IsSuccessStatusCode)
        {
            // Если PHP вернул ошибку, Bifrost сам сделает повтор по вашей политике
            throw new HttpRequestException($"PHP API returned {responce.StatusCode}");
        }

        logger.LogInformation($"Succesfully processed webhook for Client: {job.ClientId}");
    }
}