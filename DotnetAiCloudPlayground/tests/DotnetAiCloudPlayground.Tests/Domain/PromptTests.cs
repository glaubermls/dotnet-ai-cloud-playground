using DotnetAiCloudPlayground.Core.Domain;
using FluentAssertions;
using Xunit;

namespace DotnetAiCloudPlayground.Tests.Domain;

public class PromptTests
{
    [Fact]
    public void Create_WithValidContent_ShouldReturnPrompt()
    {
        // Arrange
        var content = "What is the capital of France?";

        // Act
        var prompt = Prompt.Create(content);

        // Assert
        prompt.Should().NotBeNull();
        prompt.Content.Should().Be(content);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void Create_WithEmptyOrWhitespaceContent_ShouldThrowArgumentException(string? content)
    {
        // Act
        var act = () => Prompt.Create(content!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*cannot be empty or whitespace*")
            .And.ParamName.Should().Be("content");
    }

    [Fact]
    public void Create_WithContentExceedingMaxLength_ShouldThrowArgumentException()
    {
        // Arrange
        var content = new string('a', 4001);

        // Act
        var act = () => Prompt.Create(content);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*cannot exceed 4000 characters*")
            .And.ParamName.Should().Be("content");
    }

    [Fact]
    public void Create_WithContentAtMaxLength_ShouldReturnPrompt()
    {
        // Arrange
        var content = new string('a', 4000);

        // Act
        var prompt = Prompt.Create(content);

        // Assert
        prompt.Should().NotBeNull();
        prompt.Content.Should().HaveLength(4000);
    }

    [Fact]
    public void ToString_ShouldReturnContent()
    {
        // Arrange
        var content = "Test prompt";
        var prompt = Prompt.Create(content);

        // Act
        var result = prompt.ToString();

        // Assert
        result.Should().Be(content);
    }

    [Fact]
    public void Create_WithMultilineContent_ShouldReturnPrompt()
    {
        // Arrange
        var content = "Line 1\nLine 2\nLine 3";

        // Act
        var prompt = Prompt.Create(content);

        // Assert
        prompt.Should().NotBeNull();
        prompt.Content.Should().Be(content);
    }
}
