namespace BackendDotnetLayer.Configuration;

public sealed class OpenAiOptions
{
    public const string SectionName = "OpenAi";

    public string? ApiKey { get; init; }

    public string Model { get; init; } = "gpt-4o-mini";

    public double Temperature { get; init; } = 0.7;

    public int MaxCompletionTokens { get; init; } = 300;
}
