namespace BackendDotnetLayer.Configuration;

public sealed class ResponseSemanticCacheOptions
{
    public const string SectionName = "ResponseSemanticCache";

    public bool Enabled { get; set; } = true;

    public string RedisUrl { get; set; } = "localhost:6379";

    public string Name { get; set; } = "chat-responses";

    public string KeyNamespace { get; set; } = "context-engineering";

    public double DistanceThreshold { get; set; } = 0.35d;

    public int TimeToLiveSeconds { get; set; } = 60;

    public int EmbeddingDimensions { get; set; } = 1536;

    public bool FilterBySession { get; set; } = true;
}
