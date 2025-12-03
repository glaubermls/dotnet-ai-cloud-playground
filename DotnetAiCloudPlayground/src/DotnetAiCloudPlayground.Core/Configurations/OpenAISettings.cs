namespace DotnetAiCloudPlayground.Core.Configurations;

public sealed class OpenAISettings
{
    public const string SectionName = "OpenAI";

    public string BaseUrl { get; set; } = "https://api.openai.com/v1/";
    public string Model { get; set; } = "gpt-4o-mini";
    public int MaxTokens { get; set; } = 256;
    public double Temperature { get; set; } = 0.2;
    public string? ApiKey { get; set; }
}
