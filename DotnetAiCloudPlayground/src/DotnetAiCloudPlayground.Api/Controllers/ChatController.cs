using DotnetAiCloudPlayground.Core.Application;
using DotnetAiCloudPlayground.Core.Domain;
using Microsoft.AspNetCore.Mvc;

namespace DotnetAiCloudPlayground.Api.Controllers;

[ApiController]
[Route("chat")]
public class ChatController : ControllerBase
{
    private readonly ChatUseCase _chatUseCase;
    private readonly ILogger<ChatController> _logger;

    public ChatController(ChatUseCase chatUseCase, ILogger<ChatController> logger)
    {
        _chatUseCase = chatUseCase ?? throw new ArgumentNullException(nameof(chatUseCase));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpPost("openai")]
    public async Task<IActionResult> ChatWithOpenAI(
        [FromBody] ChatRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request?.Prompt))
            {
                return BadRequest(new { error = "Prompt is required" });
            }

            var prompt = Prompt.Create(request.Prompt);
            var result = await _chatUseCase.ExecuteAsync(prompt, cancellationToken);

            return Ok(new ChatResponse
            {
                Content = result.Content,
                Model = result.Model,
                TokensUsed = result.TokensUsed,
                CreatedAt = result.CreatedAt
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid prompt received");
            return BadRequest(new { error = ex.Message });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "OpenAI API request failed");
            return StatusCode(503, new { error = "AI service temporarily unavailable" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in chat endpoint");
            return StatusCode(500, new { error = "An unexpected error occurred" });
        }
    }
}

public record ChatRequest
{
    public string Prompt { get; init; } = string.Empty;
}

public record ChatResponse
{
    public string Content { get; init; } = string.Empty;
    public string Model { get; init; } = string.Empty;
    public int TokensUsed { get; init; }
    public DateTime CreatedAt { get; init; }
}
