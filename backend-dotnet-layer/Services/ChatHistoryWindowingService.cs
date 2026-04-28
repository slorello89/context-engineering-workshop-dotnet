using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using BackendDotnetLayer.Configuration;

namespace BackendDotnetLayer.Services;

public sealed class ChatHistoryWindowingService
{
    private readonly ChatMemoryOptions _options;
    private readonly ILogger<ChatHistoryWindowingService> _logger;

    public ChatHistoryWindowingService(
        IOptions<ChatMemoryOptions> options,
        ILogger<ChatHistoryWindowingService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public ChatHistory BuildWindowedHistory(
        string systemPrompt,
        IReadOnlyList<ChatMessageContent> messages,
        string pendingUserMessage)
    {
        var history = new ChatHistory();
        history.AddSystemMessage(systemPrompt);

        var baseTokens = EstimateTokens(systemPrompt) + EstimateTokens(pendingUserMessage);
        var budget = Math.Max(_options.MaxPromptTokens - baseTokens, 0);
        var selectedMessages = new List<ChatMessageContent>();
        var consumedTokens = 0;

        for (var index = messages.Count - 1; index >= 0; index--)
        {
            var message = messages[index];
            var estimatedTokens = EstimateTokens(message.Content ?? string.Empty);
            if (selectedMessages.Count > 0 && consumedTokens + estimatedTokens > budget)
            {
                break;
            }

            if (selectedMessages.Count == 0 && estimatedTokens > budget)
            {
                continue;
            }

            selectedMessages.Insert(0, message);
            consumedTokens += estimatedTokens;
        }

        foreach (var message in selectedMessages)
        {
            history.Add(message);
        }

        var trimmedCount = messages.Count - selectedMessages.Count;
        if (trimmedCount > 0)
        {
            _logger.LogInformation(
                "Token window retained {RetainedCount} of {OriginalCount} working-memory messages for the current prompt budget of {MaxPromptTokens} tokens",
                selectedMessages.Count,
                messages.Count,
                _options.MaxPromptTokens);
        }

        return history;
    }

    private static int EstimateTokens(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return 8;
        }

        return Math.Max(8, (int)Math.Ceiling(text.Length / 4.0) + 8);
    }
}
