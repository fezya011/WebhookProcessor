using Bifrost.Core;
using WebhookProcessor.Core.Enums;

namespace WebhookProcessor.Core.Jobs;

public record WebhookJob(
    string ClientId,
    string Payload,
    string? Metadata = null,
    Priority Priority = Priority.Normal 
);