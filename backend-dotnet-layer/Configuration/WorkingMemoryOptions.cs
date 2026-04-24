namespace BackendDotnetLayer.Configuration;

public sealed class WorkingMemoryOptions
{
    public const string SectionName = "WorkingMemory";

    public string? AgentMemoryServerUrl { get; init; }

    public string DefaultSessionId { get; init; } = "user-2bfc7e6e-452f-40d6-b7e7-29855518B052";

    public string Namespace { get; init; } = "short-term-memory";

    public long TimeToLiveInSeconds { get; init; } = 300;

    public bool StoreSystemMessages { get; init; }

    public bool StoreAssistantMessages { get; init; } = true;

    public bool StoreToolMessages { get; init; }
}
