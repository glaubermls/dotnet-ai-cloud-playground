using DotnetAiCloudPlayground.Core.Application;
using DotnetAiCloudPlayground.Core.Domain;
using DotnetAiCloudPlayground.Core.Ports;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DotnetAiCloudPlayground.Tests.Application;

public class ChatUseCaseTests
{
    private readonly Mock<IChatModelPort> _mockChatModelPort;
    private readonly Mock<ILogger<ChatUseCase>> _mockLogger;
    private readonly ChatUseCase _sut;

    public ChatUseCaseTests()
    {
        _mockChatModelPort = new Mock<IChatModelPort>();
        _mockLogger = new Mock<ILogger<ChatUseCase>>();
        _sut = new ChatUseCase(_mockChatModelPort.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidPrompt_ShouldReturnChatOutput()
    {
        // Arrange
        var prompt = Prompt.Create("Test prompt");
        var expectedOutput = new ChatOutput("Test response", "gpt-4o-mini", 50);

        _mockChatModelPort
            .Setup(x => x.CompleteAsync(prompt, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedOutput);

        // Act
        var result = await _sut.ExecuteAsync(prompt);

        // Assert
        result.Should().Be(expectedOutput);
        result.Content.Should().Be("Test response");
        result.Model.Should().Be("gpt-4o-mini");
        result.TokensUsed.Should().Be(50);

        _mockChatModelPort.Verify(
            x => x.CompleteAsync(prompt, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithNullPrompt_ShouldThrowArgumentNullException()
    {
        // Act
        var act = async () => await _sut.ExecuteAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("prompt");

        _mockChatModelPort.Verify(
            x => x.CompleteAsync(It.IsAny<Prompt>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPortThrowsException_ShouldPropagateException()
    {
        // Arrange
        var prompt = Prompt.Create("Test prompt");
        var expectedException = new InvalidOperationException("API error");

        _mockChatModelPort
            .Setup(x => x.CompleteAsync(prompt, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act
        var act = async () => await _sut.ExecuteAsync(prompt);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("API error");

        _mockChatModelPort.Verify(
            x => x.CompleteAsync(prompt, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldLogInformation()
    {
        // Arrange
        var prompt = Prompt.Create("Test prompt");
        var output = new ChatOutput("Response", "gpt-4o-mini", 25);

        _mockChatModelPort
            .Setup(x => x.CompleteAsync(prompt, It.IsAny<CancellationToken>()))
            .ReturnsAsync(output);

        // Act
        await _sut.ExecuteAsync(prompt);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Executing chat use case")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("completed successfully")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPortThrowsException_ShouldLogError()
    {
        // Arrange
        var prompt = Prompt.Create("Test prompt");
        var expectedException = new HttpRequestException("Network error");

        _mockChatModelPort
            .Setup(x => x.CompleteAsync(prompt, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act
        try
        {
            await _sut.ExecuteAsync(prompt);
        }
        catch
        {
            // Expected
        }

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error executing chat use case")),
                It.Is<Exception>(ex => ex == expectedException),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithCancellationToken_ShouldPassThroughToPort()
    {
        // Arrange
        var prompt = Prompt.Create("Test prompt");
        var output = new ChatOutput("Response", "gpt-4o-mini", 30);
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        _mockChatModelPort
            .Setup(x => x.CompleteAsync(prompt, cancellationToken))
            .ReturnsAsync(output);

        // Act
        await _sut.ExecuteAsync(prompt, cancellationToken);

        // Assert
        _mockChatModelPort.Verify(
            x => x.CompleteAsync(prompt, cancellationToken),
            Times.Once);
    }
}
