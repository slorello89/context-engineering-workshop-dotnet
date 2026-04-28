namespace BackendDotnetLayer.Configuration;

public sealed class SemanticRouterOptions
{
    public const string SectionName = "SemanticRouter";

    public string RedisUrl { get; init; } = "localhost:6379";

    public string Name { get; init; } = "retrieval-router";

    public string KeyNamespace { get; init; } = "context-engineering";

    public double DistanceThreshold { get; init; } = 0.35d;

    public string EmbeddingModel { get; init; } = "text-embedding-3-small";

    public int EmbeddingDimensions { get; init; } = 1536;
}
