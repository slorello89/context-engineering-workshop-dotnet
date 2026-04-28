namespace BackendDotnetLayer.Configuration;

public sealed class FilesProcessorOptions
{
    public const string SectionName = "FilesProcessor";

    public string InputDirectory { get; init; } = "/tmp";

    public int ScanIntervalSeconds { get; init; } = 5;

    public int MaxChunkLength { get; init; } = 1000;

    public int ChunkOverlapLength { get; init; } = 100;

    public int MinimumChunkLength { get; init; } = 50;
}
