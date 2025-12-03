using System.Net;
using DotnetAiCloudPlayground.Core.Configurations;
using DotnetAiCloudPlayground.Core.Domain;
using DotnetAiCloudPlayground.Infrastructure.Adapters;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Xunit;

namespace DotnetAiCloudPlayground.Tests.Infrastructure;

public class OpenAiChatAdapterTests
{
    private readonly Mock<ILogger<OpenAiChatAdapter>> _mockLogger;
    private readonly OpenAISettings _settings;
    private readonly IOptions<OpenAISettings> _options;

    public OpenAiChatAdapterTests()
    {
        _mockLogger = new Mock<ILogger<OpenAiChatAdapter>>();
        _settings = new OpenAISettings
        {
            BaseUrl = "https://api.openai.com/v1",
            Model = "gpt-4o-mini",
            MaxTokens = 256,
            Temperature = 0.2
        };
        _options = Options.Create(_settings);
    }

    [Fact]
    public async Task CompleteAsync_WithSuccessfulResponse_ShouldReturnChatOutput()
    {
        // Arrange
        var responseJson = """
        {
            "id": "chatcmpl-123",
            "object": "chat.completion",
            "created": 1677652288,
            "model": "gpt-4o-mini",
            "choices": [{
                "index": 0,
                "message": {
                    "role": "assistant",
                    "content": "Paris is the capital of France."
                },
                "finish_reason": "stop"
            }],
            "usage": {
                "prompt_tokens": 10,
                "completion_tokens": 15,
                "total_tokens": 25
            }
        }
        """;

        var mockHandler = CreateMockHttpMessageHandler(HttpStatusCode.OK, responseJson);
        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri(_settings.BaseUrl)
        };

        var adapter = new OpenAiChatAdapter(httpClient, _options, _mockLogger.Object);
        var prompt = Prompt.Create("What is the capital of France?");

        // Act
        var result = await adapter.CompleteAsync(prompt);

        // Assert
        result.Should().NotBeNull();
        result.Content.Should().Be("Paris is the capital of France.");
        result.Model.Should().Be("gpt-4o-mini");
        result.TokensUsed.Should().Be(25);

        mockHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task CompleteAsync_With401Unauthorized_ShouldThrowHttpRequestException()
    {
        // Arrange
        var errorJson = """
        {
            "error": {
                "message": "Invalid API key",
                "type": "invalid_request_error",
                "code": "invalid_api_key"
            }
        }
        """;

        var mockHandler = CreateMockHttpMessageHandler(HttpStatusCode.Unauthorized, errorJson);
        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri(_settings.BaseUrl)
        };

        var adapter = new OpenAiChatAdapter(httpClient, _options, _mockLogger.Object);
        var prompt = Prompt.Create("Test prompt");

        // Act
        var act = async () => await adapter.CompleteAsync(prompt);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task CompleteAsync_With429TooManyRequests_ShouldThrowHttpRequestException()
    {
        // Arrange
        var errorJson = """
        {
            "error": {
                "message": "Rate limit exceeded",
                "type": "rate_limit_error"
            }
        }
        """;

        var mockHandler = CreateMockHttpMessageHandler(HttpStatusCode.TooManyRequests, errorJson);
        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri(_settings.BaseUrl)
        };

        var adapter = new OpenAiChatAdapter(httpClient, _options, _mockLogger.Object);
        var prompt = Prompt.Create("Test prompt");

        // Act
        var act = async () => await adapter.CompleteAsync(prompt);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task CompleteAsync_With500InternalServerError_ShouldThrowHttpRequestException()
    {
        // Arrange
        var errorJson = """
        {
            "error": {
                "message": "Internal server error",
                "type": "server_error"
            }
        }
        """;

        var mockHandler = CreateMockHttpMessageHandler(HttpStatusCode.InternalServerError, errorJson);
        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri(_settings.BaseUrl)
        };

        var adapter = new OpenAiChatAdapter(httpClient, _options, _mockLogger.Object);
        var prompt = Prompt.Create("Test prompt");

        // Act
        var act = async () => await adapter.CompleteAsync(prompt);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task CompleteAsync_WithEmptyChoicesArray_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var responseJson = """
        {
            "id": "chatcmpl-123",
            "model": "gpt-4o-mini",
            "choices": []
        }
        """;

        var mockHandler = CreateMockHttpMessageHandler(HttpStatusCode.OK, responseJson);
        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri(_settings.BaseUrl)
        };

        var adapter = new OpenAiChatAdapter(httpClient, _options, _mockLogger.Object);
        var prompt = Prompt.Create("Test prompt");

        // Act
        var act = async () => await adapter.CompleteAsync(prompt);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*does not contain any choices*");
    }

    [Fact]
    public async Task CompleteAsync_WithMissingMessageContent_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var responseJson = """
        {
            "id": "chatcmpl-123",
            "model": "gpt-4o-mini",
            "choices": [{
                "index": 0,
                "message": {
                    "role": "assistant"
                }
            }]
        }
        """;

        var mockHandler = CreateMockHttpMessageHandler(HttpStatusCode.OK, responseJson);
        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri(_settings.BaseUrl)
        };

        var adapter = new OpenAiChatAdapter(httpClient, _options, _mockLogger.Object);
        var prompt = Prompt.Create("Test prompt");

        // Act
        var act = async () => await adapter.CompleteAsync(prompt);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*does not contain message content*");
    }

    [Fact]
    public async Task CompleteAsync_WithInvalidJson_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var invalidJson = "{ invalid json }";

        var mockHandler = CreateMockHttpMessageHandler(HttpStatusCode.OK, invalidJson);
        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri(_settings.BaseUrl)
        };

        var adapter = new OpenAiChatAdapter(httpClient, _options, _mockLogger.Object);
        var prompt = Prompt.Create("Test prompt");

        // Act
        var act = async () => await adapter.CompleteAsync(prompt);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Failed to parse OpenAI response*");
    }

    [Fact]
    public async Task CompleteAsync_WithoutUsageInformation_ShouldReturnZeroTokens()
    {
        // Arrange
        var responseJson = """
        {
            "id": "chatcmpl-123",
            "model": "gpt-4o-mini",
            "choices": [{
                "message": {
                    "content": "Response without usage"
                }
            }]
        }
        """;

        var mockHandler = CreateMockHttpMessageHandler(HttpStatusCode.OK, responseJson);
        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri(_settings.BaseUrl)
        };

        var adapter = new OpenAiChatAdapter(httpClient, _options, _mockLogger.Object);
        var prompt = Prompt.Create("Test prompt");

        // Act
        var result = await adapter.CompleteAsync(prompt);

        // Assert
        result.Should().NotBeNull();
        result.TokensUsed.Should().Be(0);
    }

    [Fact]
    public async Task CompleteAsync_ShouldSendCorrectRequestBody()
    {
        // Arrange
        var responseJson = """
        {
            "choices": [{
                "message": {
                    "content": "Test response"
                }
            }],
            "model": "gpt-4o-mini"
        }
        """;

        HttpRequestMessage? capturedRequest = null;
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, ct) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseJson)
            });

        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri(_settings.BaseUrl)
        };

        var adapter = new OpenAiChatAdapter(httpClient, _options, _mockLogger.Object);
        var prompt = Prompt.Create("Test prompt");

        // Act
        await adapter.CompleteAsync(prompt);

        // Assert
        capturedRequest.Should().NotBeNull();
        capturedRequest!.Method.Should().Be(HttpMethod.Post);
        capturedRequest.RequestUri.Should().NotBeNull();
        capturedRequest.RequestUri!.ToString().Should().Contain("chat/completions");

        var requestBody = await capturedRequest.Content!.ReadAsStringAsync();
        requestBody.Should().Contain("gpt-4o-mini");
        requestBody.Should().Contain("Test prompt");
        requestBody.Should().Contain("256");
        requestBody.Should().Contain("0.2");
    }

    [Fact]
    public async Task CompleteAsync_WithNullPrompt_ShouldThrowArgumentNullException()
    {
        // Arrange
        var mockHandler = CreateMockHttpMessageHandler(HttpStatusCode.OK, "{}");
        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri(_settings.BaseUrl)
        };

        var adapter = new OpenAiChatAdapter(httpClient, _options, _mockLogger.Object);

        // Act
        var act = async () => await adapter.CompleteAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    private static Mock<HttpMessageHandler> CreateMockHttpMessageHandler(
        HttpStatusCode statusCode,
        string responseContent)
    {
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(responseContent)
            });

        return mockHandler;
    }
}
