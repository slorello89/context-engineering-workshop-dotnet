using BackendDotnetLayer.Configuration;
using Microsoft.Extensions.Options;
using RedisVL.Caches;
using RedisVL.Filters;
using RedisVL.Indexes;
using RedisVL.Schema;
using StackExchange.Redis;

namespace BackendDotnetLayer.Services;

public sealed class ResponseSemanticCacheService
{
    private const string SessionIdFieldName = "sessionId";

    private readonly ResponseSemanticCacheOptions _options;
    private readonly OpenAiEmbeddingVectorizer _vectorizer;
    private readonly ILogger<ResponseSemanticCacheService> _logger;
    private readonly Lazy<Task<SemanticCache?>> _cacheFactory;

    public ResponseSemanticCacheService(
        IOptions<ResponseSemanticCacheOptions> options,
        OpenAiEmbeddingVectorizer vectorizer,
        ILogger<ResponseSemanticCacheService> logger)
    {
        _options = options.Value;
        _vectorizer = vectorizer;
        _logger = logger;
        _cacheFactory = new Lazy<Task<SemanticCache?>>(CreateCacheAsync);
    }

    public async Task EnsureInitializedAsync(CancellationToken cancellationToken)
    {
        _ = await GetCacheAsync(cancellationToken);
    }

    public async Task<string?> TryGetResponseAsync(string prompt, string sessionId, CancellationToken cancellationToken)
    {
        var cache = await GetCacheAsync(cancellationToken);
        if (cache is null)
        {
            return null;
        }

        try
        {
            var hit = await cache.CheckAsync(
                prompt,
                _vectorizer,
                CreateSessionFilter(sessionId),
                cancellationToken);

            if (hit is null)
            {
                _logger.LogInformation("Semantic cache miss for session {SessionId}", sessionId);
                return null;
            }

            _logger.LogInformation(
                "Semantic cache hit for session {SessionId} with distance {Distance:F4}",
                sessionId,
                hit.Distance);

            return hit.Response;
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Semantic cache lookup failed for session {SessionId}", sessionId);
            return null;
        }
    }

    public async Task StoreResponseAsync(string prompt, string response, string sessionId, CancellationToken cancellationToken)
    {
        var cache = await GetCacheAsync(cancellationToken);
        if (cache is null)
        {
            return;
        }

        try
        {
            await cache.StoreAsync(
                prompt,
                response,
                _vectorizer,
                metadata: new
                {
                    sessionId,
                    cachedAtUtc = DateTimeOffset.UtcNow
                },
                filterValues: CreateSessionFilterValues(sessionId),
                cancellationToken: cancellationToken);

            _logger.LogInformation("Stored semantic cache entry for session {SessionId}", sessionId);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Semantic cache store failed for session {SessionId}", sessionId);
        }
    }

    private async Task<SemanticCache?> GetCacheAsync(CancellationToken cancellationToken)
    {
        if (!_options.Enabled)
        {
            return null;
        }

        return await _cacheFactory.Value.WaitAsync(cancellationToken);
    }

    private async Task<SemanticCache?> CreateCacheAsync()
    {
        try
        {
            var redisUrl = string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("REDIS_VL_REDIS_URL"))
                ? _options.RedisUrl
                : Environment.GetEnvironmentVariable("REDIS_VL_REDIS_URL")!;

            var connection = await ConnectionMultiplexer.ConnectAsync(redisUrl);
            var cache = new SemanticCache(
                connection.GetDatabase(),
                new RedisVL.Caches.SemanticCacheOptions(
                    _options.Name,
                    new VectorFieldAttributes(
                        VectorAlgorithm.Flat,
                        VectorDataType.Float32,
                        VectorDistanceMetric.Cosine,
                        _options.EmbeddingDimensions),
                    _options.DistanceThreshold,
                    _options.KeyNamespace,
                    _options.TimeToLiveSeconds > 0 ? TimeSpan.FromSeconds(_options.TimeToLiveSeconds) : null,
                    filterableFields: _options.FilterBySession
                        ? [new TagFieldDefinition(SessionIdFieldName)]
                        : null));

            await cache.CreateAsync(new CreateIndexOptions(skipIfExists: true));
            _logger.LogInformation("Semantic cache initialized");
            return cache;
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Semantic cache initialization failed; semantic caching will be disabled");
            return null;
        }
    }

    private FilterExpression? CreateSessionFilter(string sessionId)
    {
        return _options.FilterBySession
            ? Filter.Tag(SessionIdFieldName).Eq(sessionId)
            : null;
    }

    private IReadOnlyDictionary<string, object?>? CreateSessionFilterValues(string sessionId)
    {
        return _options.FilterBySession
            ? new Dictionary<string, object?> { [SessionIdFieldName] = sessionId }
            : null;
    }
}
