namespace WebhookProcessor.Core.Models;

public record WebhookRequest
{
    public string Payload { get; set; } = string.Empty;
    public string? Priority { get; init; }
    public string? IdempotencyKey { get; init; }
}
