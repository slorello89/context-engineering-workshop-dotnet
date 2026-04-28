using Microsoft.Extensions.Options;
using BackendDotnetLayer.Configuration;

namespace BackendDotnetLayer.Services;

public sealed class RerankingService
{
    private readonly RerankingOptions _options;

    public RerankingService(
        IOptions<RerankingOptions> options,
        IWebHostEnvironment environment,
        ILogger<RerankingService> logger)
    {
        _options = options.Value;
        var modelPath = ResolvePath(environment.ContentRootPath, _options.ModelPath);
        var tokenizerPath = ResolvePath(environment.ContentRootPath, _options.TokenizerPath);
        logger.LogInformation(
            "Lab 6 starter reranking is running in pass-through mode. ONNX assets are available at {ModelPath} and {TokenizerPath}.",
            modelPath,
            tokenizerPath);
    }

    public int CandidatesPerSource => _options.CandidatesPerSource;

    public async Task<IReadOnlyList<RerankedContextItem>> RerankAsync(
        string query,
        IReadOnlyList<string> candidates,
        CancellationToken cancellationToken)
    {
        await Task.CompletedTask;

        // TODO: Replace this pass-through implementation with ONNX reranking.
        return candidates
            .Take(_options.TopN)
            .Select(text => new RerankedContextItem(text, null))
            .ToList();
    }

    private static string ResolvePath(string contentRootPath, string configuredPath)
    {
        return Path.GetFullPath(Path.Combine(contentRootPath, configuredPath));
    }
}

public sealed record RerankedContextItem(string Text, double? Score);
