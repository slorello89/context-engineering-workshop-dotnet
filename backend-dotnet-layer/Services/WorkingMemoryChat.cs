using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace BackendDotnetLayer.Services;

public sealed class WorkingMemoryChat
{
    private readonly string _sessionId;
    private readonly WorkingMemoryStore _workingMemoryStore;
    private readonly List<ChatMessageContent> _messages;

    private WorkingMemoryChat(string sessionId, WorkingMemoryStore workingMemoryStore, List<ChatMessageContent> messages)
    {
        _sessionId = sessionId;
        _workingMemoryStore = workingMemoryStore;
        _messages = messages;
    }

    public static async Task<WorkingMemoryChat> CreateAsync(
        string sessionId,
        WorkingMemoryStore workingMemoryStore,
        CancellationToken cancellationToken)
    {
        var messages = await workingMemoryStore.GetMessagesAsync(sessionId, cancellationToken);
        return new WorkingMemoryChat(sessionId, workingMemoryStore, messages.ToList());
    }

    public IReadOnlyList<ChatMessageContent> Messages => _messages;

    public async Task AddAsync(ChatMessageContent message, CancellationToken cancellationToken)
    {
        _messages.Add(message);
        await _workingMemoryStore.UpdateMessagesAsync(_sessionId, _messages, cancellationToken);
    }

    public async Task ClearAsync(CancellationToken cancellationToken)
    {
        _messages.Clear();
        await _workingMemoryStore.DeleteMessagesAsync(_sessionId, cancellationToken);
    }

    public ChatHistory ToChatHistory(string systemPrompt)
    {
        var history = new ChatHistory();
        history.AddSystemMessage(systemPrompt);

        foreach (var message in _messages)
        {
            history.Add(message);
        }

        return history;
    }
}
