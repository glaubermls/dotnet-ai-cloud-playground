using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using DotnetAiCloudPlayground.Core.Configurations;
using DotnetAiCloudPlayground.Core.Domain;
using DotnetAiCloudPlayground.Core.Ports;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotnetAiCloudPlayground.Infrastructure.Adapters;

public sealed class OpenAiChatAdapter : IChatModelPort
{
    private readonly HttpClient _httpClient;
    private readonly OpenAISettings _settings;
    private readonly ILogger<OpenAiChatAdapter> _logger;

    public OpenAiChatAdapter(
        HttpClient httpClient,
        IOptions<OpenAISettings> settings,
        ILogger<OpenAiChatAdapter> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ChatOutput> CompleteAsync(Prompt prompt, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(prompt);

        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            var requestBody = new
            {
                model = _settings.Model,
                messages = new[]
                {
                    new { role = "user", content = prompt.Content }
                },
                max_tokens = _settings.MaxTokens,
                temperature = _settings.Temperature
            };

            var endpoint = "chat/completions";
            var fullUrl = new Uri(_httpClient.BaseAddress!, endpoint);

            _logger.LogDebug(
                "Sending request to OpenAI - URL: {Url}, Model: {Model}, MaxTokens: {MaxTokens}, Temperature: {Temperature}",
                fullUrl, _settings.Model, _settings.MaxTokens, _settings.Temperature);

            var response = await _httpClient.PostAsJsonAsync(
                endpoint,
                requestBody,
                cancellationToken);

            stopwatch.Stop();

            _logger.LogInformation(
                "OpenAI API response - Status: {StatusCode}, Latency: {Latency}ms",
                (int)response.StatusCode, stopwatch.ElapsedMilliseconds);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError(
                    "OpenAI API error - Status: {StatusCode}, URL: {Url}, Error: {Error}",
                    (int)response.StatusCode, fullUrl, errorContent);
                
                response.EnsureSuccessStatusCode();
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            return ParseResponse(responseContent);
        }
        catch (HttpRequestException ex)
        {
            stopwatch.Stop();
            _logger.LogError(
                ex,
                "HTTP request to OpenAI failed after {Latency}ms",
                stopwatch.ElapsedMilliseconds);
            throw;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(
                ex,
                "Unexpected error calling OpenAI API after {Latency}ms",
                stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    private ChatOutput ParseResponse(string jsonResponse)
    {
        try
        {
            using var document = JsonDocument.Parse(jsonResponse);
            var root = document.RootElement;

            if (!root.TryGetProperty("choices", out var choices) || choices.GetArrayLength() == 0)
            {
                throw new InvalidOperationException("OpenAI response does not contain any choices.");
            }

            var firstChoice = choices[0];
            if (!firstChoice.TryGetProperty("message", out var message) ||
                !message.TryGetProperty("content", out var content))
            {
                throw new InvalidOperationException("OpenAI response choice does not contain message content.");
            }

            var contentText = content.GetString() ?? string.Empty;

            var model = root.TryGetProperty("model", out var modelProp)
                ? modelProp.GetString() ?? _settings.Model
                : _settings.Model;

            var tokensUsed = 0;
            if (root.TryGetProperty("usage", out var usage) &&
                usage.TryGetProperty("total_tokens", out var totalTokens))
            {
                tokensUsed = totalTokens.GetInt32();
            }

            return new ChatOutput(contentText, model, tokensUsed);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse OpenAI response JSON");
            throw new InvalidOperationException("Failed to parse OpenAI response.", ex);
        }
    }
}
