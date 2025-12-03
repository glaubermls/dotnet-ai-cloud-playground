namespace DotnetAiCloudPlayground.Core.Domain;

public sealed class ChatOutput
{
    public string Content { get; }
    public string Model { get; }
    public int TokensUsed { get; }
    public DateTime CreatedAt { get; }

    public ChatOutput(string content, string model, int tokensUsed)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException("Chat output content cannot be empty.", nameof(content));
        }

        if (string.IsNullOrWhiteSpace(model))
        {
            throw new ArgumentException("Model name cannot be empty.", nameof(model));
        }

        if (tokensUsed < 0)
        {
            throw new ArgumentException("Tokens used cannot be negative.", nameof(tokensUsed));
        }

        Content = content;
        Model = model;
        TokensUsed = tokensUsed;
        CreatedAt = DateTime.UtcNow;
    }
}
