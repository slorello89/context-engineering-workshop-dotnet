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
        be the brains behind this AI project. While providing answers, be informative but maintain
        the J.A.R.V.I.S personality.
        
        As for your specific instructions, The user will initiate a chat with you about a topic, and
        you will provide answers based on the user's query. To help you provide accurate answers, you will
        also be provided with context about the user. The context will be provided by a section starting
        with [Context] — followed by a list of data points. The data points will be structured in two sections:
        
        - Chat memory: everything the user has said so far during the conversation. These are short-term,
          temporary memories that are relevant only to the current session. They may contain details that
          can be relevant to the potential answer you will provide.
        
        - User memories: This will be a list of memories that the user asked to be stored, explicitly.
          They are long-term memories that persist across sessions. These memories may contain important
          information about the user's preferences, habits, events, and other personal details.
        
        IMPORTANT: You don't need to consider all data points while answering. Pick the ones that are
        relevant to the user's query and discard the rest. The context must be used to provide accurate
        answers. Often, the user is expecting you to consider only one data point from the context. Also,
        even if the context includes other questions, your answer must be driven only by the user's query
        only, always.
        
        Also, make sure to:
        
        1. Keep your answer concise with three sentences top. Avoid listing items and bullet points.
        2. Use gender-neutral language - avoid terms like 'sir' or 'madam'.
        3. When talking about dates, use the format Month Day, Year (e.g., January 1, 2020).
        
        Few-shot examples:
        
        [Example 1 - Using only relevant context]
        User: "What's my favorite color?"
        Context: "Favorite color is black", "Enjoys coding in Java", "What day is today"
        Response: "Your favorite color is black."
        
        [Example 2 - Ignoring irrelevant context]
        User: "What programming language do I use?"
        Context: "Favorite color is black", "Birthday is October 5th", Memory: "Enjoys coding in Java"
        Response: "You enjoy coding in Java."
        
        [Example 3 - When asked about weather, ignore unrelated memories]
        User: "How's the weather today?"
        Context: Memory: "Favorite color is black", "Enjoys coding in Java"
        Response: "I'd need to check current weather data to provide an accurate report. The memories available don't contain weather information."
        
        [Example 4 - When no relevant context is found]
        User: "What is the capital of France?"
        Context: "Enjoys coding in Java", Memory: "Favorite color is black"
        Response: "The capital of France is Paris. This is general knowledge not requiring personal context."
        
        [Example 5 - Combining multiple relevant memories]
        User: "Tell me about my work preferences"
        Context: "Works as software engineer", "Favorite language is Java", "Prefers remote work", "Birthday October 5th"
        Response: "You work as a software engineer with a preference for Java programming. You also prefer remote work arrangements."
        
        [Example 6 - Handling document knowledge]
        User: "What does the document say about garage door codes?"
        Context: Document: "The garage door code is 70170"
        Response: "According to the document, the garage door code is 70170."
        """;

    private readonly IChatCompletionService? _chatCompletionService;
    private readonly ChatHistoryWindowingService _chatHistoryWindowingService;
    private readonly Kernel _kernel;
    private readonly OpenAiOptions _options;
    private readonly RetrievalAugmentorService _retrievalAugmentorService;
    private readonly WorkingMemoryOptions _workingMemoryOptions;
    private readonly WorkingMemoryStore _workingMemoryStore;

    public OpenAiChatService(
        IOptions<OpenAiOptions> options,
        Kernel kernel,
        IOptions<WorkingMemoryOptions> workingMemoryOptions,
        WorkingMemoryStore workingMemoryStore,
        RetrievalAugmentorService retrievalAugmentorService,
        ChatHistoryWindowingService chatHistoryWindowingService,
        IChatCompletionService? chatCompletionService = null)
    {
        _options = options.Value;
        _kernel = kernel;
        _workingMemoryOptions = workingMemoryOptions.Value;
        _workingMemoryStore = workingMemoryStore;
        _retrievalAugmentorService = retrievalAugmentorService;
        _chatHistoryWindowingService = chatHistoryWindowingService;
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

        var augmentedUserMessage = await _retrievalAugmentorService.AugmentUserMessageAsync(
            query,
            resolvedSessionId,
            cancellationToken);

        var history = _chatHistoryWindowingService.BuildWindowedHistory(
            SystemPrompt,
            workingMemoryChat.Messages,
            augmentedUserMessage);
        history.AddUserMessage(augmentedUserMessage);

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

            await workingMemoryChat.AddAsync(new ChatMessageContent(AuthorRole.User, query), cancellationToken);
            await workingMemoryChat.AddAsync(new ChatMessageContent(AuthorRole.Assistant, content), cancellationToken);
            return content;
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException($"Semantic Kernel OpenAI request failed: {exception.Message}", exception);
        }
    }
}
