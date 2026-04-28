namespace BackendDotnetLayer.Configuration;

public sealed class ChatMemoryOptions
{
    public const string SectionName = "ChatMemory";

    public int MaxPromptTokens { get; set; } = 2000;
}
