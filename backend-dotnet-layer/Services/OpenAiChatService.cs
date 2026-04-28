using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using BackendDotnetLayer.Configuration;

namespace BackendDotnetLayer.Services;

public sealed class OpenAiChatService
{
    private const string SystemPrompt = """
        You are an AI assistant that should act, talk, and behave as if you were J.A.R.V.I.S AI
        from the Iron Man movies. Be formal but friendly, and add personality. You are going to
        be the brains behind this AI project.

        Also, make sure to:

        1. Keep your answer concise with three sentences top. Avoid listing items and bullet points.
        2. Use gender-neutral language - avoid terms like 'sir' or 'madam'.
        3. When talking about dates, use the format Month Day, Year (e.g., January 1, 2020).
        """;

    private readonly IChatCompletionService? _chatCompletionService;
    private readonly Kernel _kernel;
    private readonly OpenAiOptions _options;
    private readonly WorkingMemoryOptions _workingMemoryOptions;
    private readonly WorkingMemoryStore _workingMemoryStore;

    public OpenAiChatService(
        IOptions<OpenAiOptions> options,
        Kernel kernel,
        IOptions<WorkingMemoryOptions> workingMemoryOptions,
        WorkingMemoryStore workingMemoryStore,
        IChatCompletionService? chatCompletionService = null)
    {
        _options = options.Value;
        _kernel = kernel;
        _workingMemoryOptions = workingMemoryOptions.Value;
        _workingMemoryStore = workingMemoryStore;
        _chatCompletionService = chatCompletionService;
    }

    public async Task<string> ChatAsync(string query, string? sessionId, CancellationToken cancellationToken)
    {
        var apiKey = string.IsNullOrWhiteSpace(_options.ApiKey)
            ? Environment.GetEnvironmentVariable("OPENAI_API_KEY")
            : _options.ApiKey;

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("OpenAI API key is missing. Set OPENAI_API_KEY before starting the app.");
        }

        if (_chatCompletionService is null)
        {
            throw new InvalidOperationException("Semantic Kernel OpenAI chat service is not configured.");
        }

        var resolvedSessionId = string.IsNullOrWhiteSpace(sessionId)
            ? _workingMemoryOptions.DefaultSessionId
            : sessionId;

        var workingMemoryChat = await WorkingMemoryChat.CreateAsync(
            resolvedSessionId,
            _workingMemoryStore,
            cancellationToken);

        await workingMemoryChat.AddAsync(new ChatMessageContent(AuthorRole.User, query), cancellationToken);

        var history = workingMemoryChat.ToChatHistory(SystemPrompt);

        var executionSettings = new OpenAIPromptExecutionSettings
        {
            Temperature = _options.Temperature,
            MaxTokens = _options.MaxCompletionTokens
        };

        try
        {
            var response = await _chatCompletionService.GetChatMessageContentAsync(
                history,
                executionSettings: executionSettings,
                kernel: _kernel,
                cancellationToken: cancellationToken);

            var content = response.Content?.Trim()
                ?? throw new InvalidOperationException("OpenAI response did not include assistant content.");

            await workingMemoryChat.AddAsync(new ChatMessageContent(AuthorRole.Assistant, content), cancellationToken);
            return content;
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException($"Semantic Kernel OpenAI request failed: {exception.Message}", exception);
        }
    }
}
