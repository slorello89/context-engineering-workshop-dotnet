using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using BackendDotnetLayer.Configuration;

namespace BackendDotnetLayer.Services;

public sealed class WorkingMemoryStore
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly ILogger<WorkingMemoryStore> _logger;
    private readonly WorkingMemoryOptions _options;

    public WorkingMemoryStore(
        HttpClient httpClient,
        IOptions<WorkingMemoryOptions> options,
        ILogger<WorkingMemoryStore> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ChatMessageContent>> GetMessagesAsync(string sessionId, CancellationToken cancellationToken)
    {
        var baseUrl = GetAgentMemoryServerUrl();
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            return [];
        }

        var requestUri = $"{baseUrl}/v1/working-memory/{Uri.EscapeDataString(sessionId)}?namespace={Uri.EscapeDataString(_options.Namespace)}";

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return [];
            }

            var payload = await response.Content.ReadAsStringAsync(cancellationToken);
            var root = JsonNode.Parse(payload);
            var messages = root?["messages"]?.AsArray();
            if (messages is null)
            {
                return [];
            }

            var result = new List<ChatMessageContent>();
            foreach (var messageNode in messages)
            {
                var role = messageNode?["role"]?.GetValue<string>() ?? string.Empty;
                var content = messageNode?["content"]?.GetValue<string>() ?? string.Empty;
                var message = ToChatMessage(role, content);
                if (message is not null)
                {
                    result.Add(message);
                }
            }

            return result;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error loading working memory for session {SessionId}", sessionId);
            return [];
        }
    }

    public async Task UpdateMessagesAsync(string sessionId, IReadOnlyList<ChatMessageContent> messages, CancellationToken cancellationToken)
    {
        var baseUrl = GetAgentMemoryServerUrl();
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            return;
        }

        var messagesToStore = messages
            .Where(ShouldStore)
            .Select(message => new Dictionary<string, string>
            {
                ["role"] = DetermineRole(message),
                ["content"] = message.Content ?? string.Empty
            })
            .ToList();

        var requestBody = new
        {
            session_id = sessionId,
            messages = messagesToStore,
            @namespace = _options.Namespace,
            ttl_seconds = _options.TimeToLiveInSeconds,
            long_term_memory_strategy = new
            {
                strategy = "discrete",
                config = new { }
            }
        };

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Put, $"{baseUrl}/v1/working-memory/{Uri.EscapeDataString(sessionId)}")
            {
                Content = new StringContent(JsonSerializer.Serialize(requestBody, JsonOptions), Encoding.UTF8, "application/json")
            };

            await _httpClient.SendAsync(request, cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error updating working memory for session {SessionId}", sessionId);
        }
    }

    public async Task DeleteMessagesAsync(string sessionId, CancellationToken cancellationToken)
    {
        var baseUrl = GetAgentMemoryServerUrl();
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            return;
        }

        var requestUri = $"{baseUrl}/v1/working-memory/{Uri.EscapeDataString(sessionId)}?namespace={Uri.EscapeDataString(_options.Namespace)}";

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Delete, requestUri);
            await _httpClient.SendAsync(request, cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error deleting working memory for session {SessionId}", sessionId);
        }
    }

    private string? GetAgentMemoryServerUrl()
    {
        return string.IsNullOrWhiteSpace(_options.AgentMemoryServerUrl)
            ? Environment.GetEnvironmentVariable("AGENT_MEMORY_SERVER_URL")
            : _options.AgentMemoryServerUrl;
    }

    private bool ShouldStore(ChatMessageContent message)
    {
        if (message.Role == AuthorRole.System)
        {
            return _options.StoreSystemMessages;
        }

        if (message.Role == AuthorRole.Assistant)
        {
            return _options.StoreAssistantMessages;
        }

        if (message.Role.Label.Equals("tool", StringComparison.OrdinalIgnoreCase))
        {
            return _options.StoreToolMessages;
        }

        return true;
    }

    private static string DetermineRole(ChatMessageContent message)
    {
        if (message.Role == AuthorRole.User)
        {
            return "user";
        }

        if (message.Role == AuthorRole.Assistant)
        {
            return "assistant";
        }

        if (message.Role == AuthorRole.System)
        {
            return "system";
        }

        return message.Role.Label;
    }

    private ChatMessageContent? ToChatMessage(string role, string content)
    {
        if ((!_options.StoreSystemMessages && role.Equals("system", StringComparison.OrdinalIgnoreCase)) ||
            (!_options.StoreAssistantMessages && (role.Equals("assistant", StringComparison.OrdinalIgnoreCase) || role.Equals("ai", StringComparison.OrdinalIgnoreCase))) ||
            (!_options.StoreToolMessages && role.Equals("tool", StringComparison.OrdinalIgnoreCase)))
        {
            return null;
        }

        return role.ToLowerInvariant() switch
        {
            "user" => new ChatMessageContent(AuthorRole.User, content),
            "assistant" or "ai" => new ChatMessageContent(AuthorRole.Assistant, content),
            "system" => new ChatMessageContent(AuthorRole.System, content),
            "tool" => new ChatMessageContent(new AuthorRole("tool"), content),
            _ => null
        };
    }
}
