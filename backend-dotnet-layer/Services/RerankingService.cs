using Microsoft.Extensions.Options;
using RedisVL.Rerankers;
using RedisVL.Rerankers.Onnx;
using BackendDotnetLayer.Configuration;

namespace BackendDotnetLayer.Services;

public sealed class RerankingService : IDisposable
{
    private readonly ILogger<RerankingService> _logger;
    private readonly RerankingOptions _options;
    private readonly OnnxTextReranker? _reranker;

    public RerankingService(
        IOptions<RerankingOptions> options,
        IWebHostEnvironment environment,
        ILogger<RerankingService> logger)
    {
        _logger = logger;
        _options = options.Value;

        var modelPath = ResolvePath(environment.ContentRootPath, _options.ModelPath);
        var tokenizerPath = ResolvePath(environment.ContentRootPath, _options.TokenizerPath);

        if (!File.Exists(modelPath) || !File.Exists(tokenizerPath))
        {
            _logger.LogWarning(
                "ONNX reranker disabled because model assets were not found. Model: {ModelPath}, Tokenizer: {TokenizerPath}",
                modelPath,
                tokenizerPath);
            return;
        }

        _reranker = new OnnxTextReranker(
            new OnnxRerankerOptions
            {
                ModelPath = modelPath,
                TokenizerPath = tokenizerPath,
                MaxSequenceLength = _options.MaxSequenceLength,
                ScoreThreshold = _options.ScoreThreshold
            });
    }

    public int CandidatesPerSource => _options.CandidatesPerSource;

    public async Task<IReadOnlyList<RerankedContextItem>> RerankAsync(
        string query,
        IReadOnlyList<string> candidates,
        CancellationToken cancellationToken)
    {
        if (_reranker is null || candidates.Count == 0)
        {
            return candidates
                .Take(_options.TopN)
                .Select(text => new RerankedContextItem(text, null))
                .ToList();
        }

        try
        {
            var request = new RerankRequest(
                query,
                    candidates.Select((text, index) => new RerankDocument(text, index.ToString())).ToArray(),
                topN: _options.TopN);

            var results = await _reranker.RerankAsync(request, cancellationToken);
            return results
                .Select(result => new RerankedContextItem(result.Document.Text, result.Score))
                .ToList();
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "ONNX reranker failed; falling back to unreranked candidates");
            return candidates
                .Take(_options.TopN)
                .Select(text => new RerankedContextItem(text, null))
                .ToList();
        }
    }

    public void Dispose()
    {
        _reranker?.Dispose();
    }

    private static string ResolvePath(string contentRootPath, string configuredPath)
    {
        return Path.GetFullPath(Path.Combine(contentRootPath, configuredPath));
    }
}

public sealed record RerankedContextItem(string Text, double? Score);
