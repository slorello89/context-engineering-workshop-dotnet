using Microsoft.Extensions.Options;
using RedisVL.Schema;
using RedisVL.Workflows;
using StackExchange.Redis;
using BackendDotnetLayer.Configuration;
using AppSemanticRouterOptions = BackendDotnetLayer.Configuration.SemanticRouterOptions;

namespace BackendDotnetLayer.Services;

public sealed class SemanticRoutingService : IAsyncDisposable
{
    private static readonly IReadOnlyDictionary<string, string[]> RouteReferences = new Dictionary<string, string[]>
    {
        ["user-memory"] =
        [
            "what do I like",
            "what is my favorite",
            "what did I tell you about myself",
            "remember my preferences",
            "use my personal memory",
            "what do you know about me"
        ],
        ["knowledge-base"] =
        [
            "what does the document say",
            "search the knowledge base",
            "use the uploaded pdf",
            "find this in the docs",
            "look in the knowledge base",
            "answer from the document"
        ],
        ["hybrid"] =
        [
            "use my memory and the knowledge base",
            "combine what you know about me with the documents",
            "use both memory and docs",
            "personal context and knowledge base",
            "my preferences plus the document",
            "combine all available context"
        ]
    };

    private readonly ILogger<SemanticRoutingService> _logger;
    private readonly AppSemanticRouterOptions _options;
    private readonly OpenAiEmbeddingVectorizer _vectorizer;
    private readonly SemaphoreSlim _initializationLock = new(1, 1);

    private ConnectionMultiplexer? _connection;
    private SemanticRouter? _router;
    private bool _initialized;

    public SemanticRoutingService(
        IOptions<AppSemanticRouterOptions> options,
        OpenAiEmbeddingVectorizer vectorizer,
        ILogger<SemanticRoutingService> logger)
    {
        _options = options.Value;
        _vectorizer = vectorizer;
        _logger = logger;
    }

    public async Task<string?> RouteAsync(string input, CancellationToken cancellationToken)
    {
        await EnsureInitializedAsync(cancellationToken);
        if (_router is null)
        {
            return null;
        }

        try
        {
            var match = await _router.RouteAsync(input, _vectorizer, cancellationToken);
            return match?.RouteName;
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Semantic router failed; falling back to querying all retrievers");
            return null;
        }
    }

    public async Task EnsureInitializedAsync(CancellationToken cancellationToken)
    {
        if (_initialized)
        {
            return;
        }

        await _initializationLock.WaitAsync(cancellationToken);
        try
        {
            if (_initialized)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("OPENAI_API_KEY")))
            {
                _logger.LogInformation("Semantic router is disabled because OpenAI API key is missing.");
                _initialized = true;
                return;
            }

            var redisUrl = Environment.GetEnvironmentVariable("REDIS_VL_REDIS_URL")
                ?? Environment.GetEnvironmentVariable("REDIS_URL")
                ?? _options.RedisUrl;

            _connection = await ConnectionMultiplexer.ConnectAsync(redisUrl);
            var database = _connection.GetDatabase();

            _router = new SemanticRouter(
                database,
                new RedisVL.Workflows.SemanticRouterOptions(
                    _options.Name,
                    new VectorFieldAttributes(
                        VectorAlgorithm.Flat,
                        VectorDataType.Float32,
                        VectorDistanceMetric.Cosine,
                        _options.EmbeddingDimensions),
                    _options.DistanceThreshold,
                    keyNamespace: _options.KeyNamespace));

            await _router.CreateAsync();

            foreach (var route in RouteReferences)
            {
                foreach (var reference in route.Value)
                {
                    await _router.AddRouteAsync(route.Key, reference, _vectorizer, cancellationToken);
                }
            }

            _initialized = true;
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Semantic router initialization failed; retrieval will fall back to querying all retrievers.");
            _initialized = true;
            _router = null;
        }
        finally
        {
            _initializationLock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        _initializationLock.Dispose();

        if (_connection is not null)
        {
            await _connection.DisposeAsync();
        }
    }
}
