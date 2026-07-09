using Bifrost.Core;
using System.ComponentModel.DataAnnotations;
using WebhookProcessor.Api.Endpoints;
using WebhookProcessor.Api.Extensions;
using WebhookProcessor.Core.Jobs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddWebhookService(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapWebhookEndpoints();

app.MapHealthChecks("/health");

await app.RunAsync();
