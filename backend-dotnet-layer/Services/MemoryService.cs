using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;
using BackendDotnetLayer.Configuration;

namespace BackendDotnetLayer.Services;

public sealed class MemoryService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly ILogger<MemoryService> _logger;
    private readonly MemoryOptions _memoryOptions;
    private readonly WorkingMemoryOptions _workingMemoryOptions;

    public MemoryService(
        HttpClient httpClient,
        IOptions<MemoryOptions> memoryOptions,
        IOptions<WorkingMemoryOptions> workingMemoryOptions,
        ILogger<MemoryService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _memoryOptions = memoryOptions.Value;
        _workingMemoryOptions = workingMemoryOptions.Value;
    }

    public async Task<IReadOnlyList<string>> SearchUserMemoriesAsync(string userId, string memory, CancellationToken cancellationToken, int limit = 5)
    {
        var request = new
        {
            session_id = new { eq = userId },
            @namespace = new { any = new[] { _memoryOptions.ShortTermNamespace, _memoryOptions.LongTermNamespace } },
            text = memory,
            limit
        };

        return await ExecuteSearchAsync(request, cancellationToken);
    }

    public async Task<bool> CreateUserMemoryAsync(string sessionId, string userId, string memory, CancellationToken cancellationToken)
    {
        var payload = new
        {
            memories = new[]
            {
                new
                {
                    id = sessionId,
                    session_id = userId,
                    @namespace = _memoryOptions.LongTermNamespace,
                    text = memory,
                    memory_type = _memoryOptions.MemoryType
                }
            }
        };

        try
        {
            using var request = BuildJsonRequest(HttpMethod.Post, "/v1/long-term-memory/", payload);
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            var root = JsonNode.Parse(body);
            return string.Equals(root?["status"]?.GetValue<string>(), "ok", StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error saving long-term memory for user {UserId}", userId);
            return false;
        }
    }

    public async Task CreateKnowledgeBaseEntryAsync(string memory, CancellationToken cancellationToken)
    {
        var sanitizedMemory = string.IsNullOrWhiteSpace(memory)
            ? string.Empty
            : memory.Replace("\r", " ").Replace("\n", " ").Trim();

        if (string.IsNullOrWhiteSpace(sanitizedMemory))
        {
            return;
        }

        var payload = new
        {
            memories = new[]
            {
                new
                {
                    id = $"knowledge.entry.{Guid.NewGuid()}",
                    @namespace = _memoryOptions.KnowledgeNamespace,
                    text = sanitizedMemory,
                    memory_type = _memoryOptions.MemoryType
                }
            }
        };

        try
        {
            using var request = BuildJsonRequest(HttpMethod.Post, "/v1/long-term-memory/", payload);
            await _httpClient.SendAsync(request, cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error creating knowledge-base entry");
        }
    }

    public async Task<IReadOnlyList<string>> SearchKnowledgeBaseAsync(string memory, CancellationToken cancellationToken, int limit = 1)
    {
        var request = new
        {
            @namespace = new { eq = _memoryOptions.KnowledgeNamespace },
            text = memory,
            limit
        };

        return await ExecuteSearchAsync(request, cancellationToken);
    }

    private async Task<IReadOnlyList<string>> ExecuteSearchAsync(object searchRequest, CancellationToken cancellationToken)
    {
        try
        {
            using var request = BuildJsonRequest(HttpMethod.Post, "/v1/long-term-memory/search?optimize_query=false", searchRequest);
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return [];
            }

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            var root = JsonNode.Parse(body);
            var memories = root?["memories"]?.AsArray();
            if (memories is null)
            {
                return [];
            }

            return memories
                .Select(node => node?["text"]?.GetValue<string>())
                .Where(text => !string.IsNullOrWhiteSpace(text))
                .Cast<string>()
                .ToList();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error during long-term memory search");
            return [];
        }
    }

    private HttpRequestMessage BuildJsonRequest(HttpMethod method, string relativePath, object? payload)
    {
        var baseUrl = GetAgentMemoryServerUrl();
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new InvalidOperationException("Agent memory server URL is missing. Set AGENT_MEMORY_SERVER_URL before using MemoryService.");
        }

        var request = new HttpRequestMessage(method, new Uri(new Uri(AppendTrailingSlash(baseUrl)), relativePath));
        if (payload is not null)
        {
            request.Content = new StringContent(JsonSerializer.Serialize(payload, JsonOptions), Encoding.UTF8, "application/json");
        }

        return request;
    }

    private string? GetAgentMemoryServerUrl()
    {
        return string.IsNullOrWhiteSpace(_workingMemoryOptions.AgentMemoryServerUrl)
            ? Environment.GetEnvironmentVariable("AGENT_MEMORY_SERVER_URL")
            : _workingMemoryOptions.AgentMemoryServerUrl;
    }

    private static string AppendTrailingSlash(string value)
    {
        return value.EndsWith('/') ? value : $"{value}/";
    }
}
