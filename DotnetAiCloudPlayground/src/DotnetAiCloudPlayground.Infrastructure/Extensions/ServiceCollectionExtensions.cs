using DotnetAiCloudPlayground.Core.Configurations;
using DotnetAiCloudPlayground.Core.Ports;
using DotnetAiCloudPlayground.Infrastructure.Adapters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;

namespace DotnetAiCloudPlayground.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOpenAIClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<OpenAISettings>(configuration.GetSection(OpenAISettings.SectionName));

        services.AddHttpClient<IChatModelPort, OpenAiChatAdapter>((serviceProvider, client) =>
        {
            var settings = configuration
                .GetSection(OpenAISettings.SectionName)
                .Get<OpenAISettings>() ?? new OpenAISettings();

            var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") 
                         ?? settings.ApiKey;

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException(
                    "OpenAI API key is not configured. Set OPENAI_API_KEY environment variable or configure it in settings.");
            }

            var baseUrl = settings.BaseUrl.TrimEnd('/');
            client.BaseAddress = new Uri($"{baseUrl}/");
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            client.Timeout = TimeSpan.FromSeconds(30);
            
            Console.WriteLine($"[OpenAI Config] BaseUrl: {client.BaseAddress}");
            Console.WriteLine($"[OpenAI Config] Model: {settings.Model}");
            Console.WriteLine($"[OpenAI Config] API Key configured: {!string.IsNullOrWhiteSpace(apiKey)}");
        })
        .AddPolicyHandler(GetRetryPolicy())
        .AddPolicyHandler(GetTimeoutPolicy());

        return services;
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryAttempt, context) =>
                {
                    Console.WriteLine(
                        $"Retry {retryAttempt} for OpenAI request after {timespan.TotalSeconds}s due to {outcome.Result?.StatusCode}");
                });
    }

    private static IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy()
    {
        return Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(30));
    }
}
