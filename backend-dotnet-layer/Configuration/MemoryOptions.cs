namespace BackendDotnetLayer.Configuration;

public sealed class MemoryOptions
{
    public const string SectionName = "Memory";

    public string LongTermNamespace { get; init; } = "long-term-memory";

    public string KnowledgeNamespace { get; init; } = "knowledge-base";

    public string ShortTermNamespace { get; init; } = "short-term-memory";

    public string MemoryType { get; init; } = "semantic";
}
