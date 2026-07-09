using WebhookProcessor.Api.Services;
using WebhookProcessor.Core.Enums;
using WebhookProcessor.Core.Models;

namespace WebhookProcessor.Api.Endpoints;

public static class WebhookEndpoints
{
    public static void MapWebhookEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/webhooks").WithTags("Webhooks");

        group.MapPost("/{clientId}", SubmitWebhookAsync)
             .WithName("SubmitWebhook")
             .Produces(202)
             .Produces(400)
             .Produces(503);

        group.MapGet("/status/{clientId}", GetWebhookStatusAsync)
             .WithName("GetWebhookStatus")
             .Produces(200);
    }

    private static async Task<IResult> SubmitWebhookAsync(
        string clientId,
        WebhookRequest request,
        WebhookService webhookService,
        CancellationToken cancellationToken)
    {
        var result = await webhookService.SubmitWebhookAsync(clientId, request, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.QueueFull)
            {
                return Results.StatusCode(503);
            }

            return Results.BadRequest(new
            {
                Error = result.ErrorMessage,
                Detail = "The Payload field cannot be empty"
            });
        }

        return Results.Accepted(
            $"/api/webhooks/status/{clientId}",
            new
            {
                Status = "Accepted",
                Message = "Webhook accepted for processing",
                result.ClientId,
                result.Priority,
                result.IdempotencyKey,
                result.QueuePosition,
                result.EstimatedWait
            }
        );
    }

    private static IResult GetWebhookStatusAsync(
        string clientId,
        WebhookService webhookService)
    {
        var status = webhookService.GetStatus(clientId);

        return Results.Ok(status);
    }
}
