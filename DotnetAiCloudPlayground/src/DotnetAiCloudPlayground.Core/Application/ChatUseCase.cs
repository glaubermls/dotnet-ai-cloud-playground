using DotnetAiCloudPlayground.Core.Domain;
using DotnetAiCloudPlayground.Core.Ports;
using Microsoft.Extensions.Logging;

namespace DotnetAiCloudPlayground.Core.Application;

public sealed class ChatUseCase
{
    private readonly IChatModelPort _chatModelPort;
    private readonly ILogger<ChatUseCase> _logger;

    public ChatUseCase(IChatModelPort chatModelPort, ILogger<ChatUseCase> logger)
    {
        _chatModelPort = chatModelPort ?? throw new ArgumentNullException(nameof(chatModelPort));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ChatOutput> ExecuteAsync(Prompt prompt, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(prompt);

        _logger.LogInformation("Executing chat use case with prompt length: {PromptLength}", prompt.Content.Length);

        try
        {
            var output = await _chatModelPort.CompleteAsync(prompt, cancellationToken);
            
            _logger.LogInformation(
                "Chat use case completed successfully. Model: {Model}, Tokens: {Tokens}", 
                output.Model, 
                output.TokensUsed);

            return output;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing chat use case");
            throw;
        }
    }
}
