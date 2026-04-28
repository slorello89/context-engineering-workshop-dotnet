using Microsoft.Extensions.Options;
using BackendDotnetLayer.Configuration;

namespace BackendDotnetLayer.Services;

public sealed class RetrievalAugmentorService
{
    private readonly ILogger<RetrievalAugmentorService> _logger;
    private readonly MemoryService _memoryService;
    private readonly RerankingService _rerankingService;
    private readonly SemanticRoutingService _semanticRoutingService;
    private readonly WorkingMemoryOptions _workingMemoryOptions;

    public RetrievalAugmentorService(
        ILogger<RetrievalAugmentorService> logger,
        MemoryService memoryService,
        RerankingService rerankingService,
        SemanticRoutingService semanticRoutingService,
        IOptions<WorkingMemoryOptions> workingMemoryOptions)
    {
        _logger = logger;
        _memoryService = memoryService;
        _rerankingService = rerankingService;
        _semanticRoutingService = semanticRoutingService;
        _workingMemoryOptions = workingMemoryOptions.Value;
    }

    public async Task<string> AugmentUserMessageAsync(string userMessage, string? sessionId, CancellationToken cancellationToken)
    {
        var resolvedSessionId = string.IsNullOrWhiteSpace(sessionId)
            ? _workingMemoryOptions.DefaultSessionId
            : sessionId;

        var retrievalQuery = await CompressQueryAsync(userMessage, cancellationToken);
        var route = await _semanticRoutingService.RouteAsync(retrievalQuery, cancellationToken);
        _logger.LogInformation(
            "Retrieval augmentor session {SessionId}: compressed query '{RetrievalQuery}', selected route '{Route}'",
            resolvedSessionId,
            retrievalQuery,
            route ?? "fallback-all");

        var rerankedContextItems = await RetrieveContextItemsAsync(route, resolvedSessionId, retrievalQuery, cancellationToken);
        var contextItems = rerankedContextItems.Select(item => item.Text).ToList();

        if (contextItems.Count == 0)
        {
            _logger.LogInformation(
                "Retrieval augmentor session {SessionId}: no context items selected after retrieval/reranking",
                resolvedSessionId);
            return userMessage;
        }

        _logger.LogInformation(
            "Retrieval augmentor session {SessionId}: injecting {ContextCount} context items. Top rerank scores: {TopScores}. Preview: {ContextPreview}",
            resolvedSessionId,
            contextItems.Count,
            string.Join(", ", rerankedContextItems.Take(3).Select(FormatScoreForLog)),
            string.Join(" | ", contextItems.Take(3).Select(TruncateForLog)));

        var contextBlock = string.Join(Environment.NewLine, contextItems.Select(item => $"- {item.Trim()}"));
        return $"{userMessage}{Environment.NewLine}{Environment.NewLine}[Context]{Environment.NewLine}{contextBlock}";
    }

    private Task<string> CompressQueryAsync(string userMessage, CancellationToken cancellationToken)
    {
        // TODO: Implement query compression for retrieval. For now we use the raw user message.
        return Task.FromResult(userMessage);
    }

    private async Task<List<RerankedContextItem>> RetrieveContextItemsAsync(
        string? route,
        string sessionId,
        string retrievalQuery,
        CancellationToken cancellationToken)
    {
        var shouldSearchUserMemory = route is null or "user-memory" or "hybrid";
        var shouldSearchKnowledgeBase = route is null or "knowledge-base" or "hybrid";

        Task<IReadOnlyList<string>>? userMemoriesTask = shouldSearchUserMemory
            ? _memoryService.SearchUserMemoriesAsync(sessionId, retrievalQuery, cancellationToken, _rerankingService.CandidatesPerSource)
            : null;

        Task<IReadOnlyList<string>>? knowledgeBaseTask = shouldSearchKnowledgeBase
            ? _memoryService.SearchKnowledgeBaseAsync(retrievalQuery, cancellationToken, _rerankingService.CandidatesPerSource)
            : null;

        await Task.WhenAll(
            userMemoriesTask ?? Task.FromResult<IReadOnlyList<string>>([]),
            knowledgeBaseTask ?? Task.FromResult<IReadOnlyList<string>>([]));

        var userMemoryResults = userMemoriesTask?.Result ?? [];
        var knowledgeBaseResults = knowledgeBaseTask?.Result ?? [];

        _logger.LogInformation(
            "Retrieval augmentor session {SessionId}: retrieved {UserMemoryCount} user-memory candidates and {KnowledgeBaseCount} knowledge-base candidates before reranking",
            sessionId,
            userMemoryResults.Count,
            knowledgeBaseResults.Count);

        var candidates = userMemoryResults
            .Concat(knowledgeBaseTask?.Result ?? [])
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.Ordinal)
            .ToList();

        var reranked = (await _rerankingService.RerankAsync(retrievalQuery, candidates, cancellationToken)).ToList();

        _logger.LogInformation(
            "Retrieval augmentor session {SessionId}: {CandidateCount} merged candidates reduced to {RerankedCount} reranked context items",
            sessionId,
            candidates.Count,
            reranked.Count);

        return reranked;
    }

    private static string TruncateForLog(string value)
    {
        const int maxLength = 120;
        var normalized = value.Replace(Environment.NewLine, " ").Trim();
        return normalized.Length <= maxLength
            ? normalized
            : normalized[..maxLength] + "...";
    }

    private static string FormatScoreForLog(RerankedContextItem item)
    {
        return item.Score is null
            ? "n/a"
            : item.Score.Value.ToString("F4", System.Globalization.CultureInfo.InvariantCulture);
    }
}
