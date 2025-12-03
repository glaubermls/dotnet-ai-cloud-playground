namespace DotnetAiCloudPlayground.Core.Domain;

public sealed class Prompt
{
    private const int MaxLength = 4000;
    
    public string Content { get; }

    private Prompt(string content)
    {
        Content = content;
    }

    public static Prompt Create(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException("Prompt content cannot be empty or whitespace.", nameof(content));
        }

        if (content.Length > MaxLength)
        {
            throw new ArgumentException($"Prompt content cannot exceed {MaxLength} characters.", nameof(content));
        }

        return new Prompt(content);
    }

    public override string ToString() => Content;
}
