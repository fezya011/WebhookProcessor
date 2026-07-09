using Bifrost.Core;
using WebhookProcessor.Core.Enums;
using WebhookProcessor.Core.Jobs;
using WebhookProcessor.Core.Models;

namespace WebhookProcessor.Api.Services;

public class WebhookService
{
    private readonly IWorkOrchestrator<WebhookJob> _orchestrator;
    private readonly ILogger<WebhookService> _logger;

    public WebhookService(
        IWorkOrchestrator<WebhookJob> orchestrator,
        ILogger<WebhookService> logger)
    {
        _orchestrator = orchestrator;
        _logger = logger;
    }

    public async Task<WebhookResult> SubmitWebhookAsync(
        string clientId,
        WebhookRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.Payload))
        {
            return new WebhookResult
            {
                IsSuccess = false,
                ErrorMessage = "Payload is required"
            };
        }

        var priority = request.Priority?.ToLower() switch
        {
            "high" => Priority.High,
            "low" => Priority.Low,
            _ => Priority.Normal,
        };

        var job = new WebhookJob(
            ClientId: clientId,
            Payload: request.Payload,
            Metadata: request.IdempotencyKey,
            Priority: priority
        );

        var result = await _orchestrator.EnqueueAsync(job, WorkClass.Default, cancellationToken);

        if (!result.IsAccepted)
        {
            _logger.LogWarning(
                $"The queue is full: ClientId={clientId}, " +
                $"Pending={_orchestrator.PendingCount}, " +
                $"Capacity={_orchestrator.Capacity}"
            );

            return new WebhookResult
            {
                IsSuccess = false,
                ErrorMessage = "Queue is full",
                QueueFull = true
            };
        }

        _logger.LogInformation(
            $"Webhook accepted: ClientId={clientId}, " +
            $"Priority={priority}, Key={request.IdempotencyKey}"
        );

        return new WebhookResult
        {
            IsSuccess = true,
            ClientId = clientId,
            Priority = priority,
            IdempotencyKey = request.IdempotencyKey,
            QueuePosition = _orchestrator.PendingCount + 1,
            EstimatedWait = priority == Priority.High ? "< 1 sec" : "< 5 sec",
            PendingCount = _orchestrator.PendingCount,
            ActiveWorkers = _orchestrator.ActiveWorkers,
            Capacity = _orchestrator.Capacity
        };
    }

    public WebhookStatus GetStatus(string clientId)
    {
        return new WebhookStatus
        {
            ClientId = clientId,
            Status = "Processing",
            QueuePosition = _orchestrator.PendingCount > 0 ? $"~{_orchestrator.PendingCount}" : "Processing now",
            PendingCount = _orchestrator.PendingCount,
            ActiveWorkers = _orchestrator.ActiveWorkers,
            Capacity = _orchestrator.Capacity
        };
    }
}

public class WebhookResult
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public bool QueueFull { get; set; }
    public string? ClientId { get; set; }
    public Priority? Priority { get; set; }
    public string? IdempotencyKey { get; set; }
    public int QueuePosition { get; set; }
    public string? EstimatedWait { get; set; }
    public int PendingCount { get; set; }
    public int ActiveWorkers { get; set; }
    public int Capacity { get; set; }
}

public class WebhookStatus
{
    public string ClientId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string QueuePosition { get; set; } = string.Empty;
    public int PendingCount { get; set; }
    public int ActiveWorkers { get; set; }
    public int Capacity { get; set; }
}