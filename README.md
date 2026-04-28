# Lab 8: Enabling Token Management to Handle Token Limits

## Learning Objectives

By the end of this lab, you will:
- implement token-window management for prompt construction
- configure a dynamic message budget for working memory
- preserve recent conversation context while trimming older turns
- test long conversations without overflowing the prompt

#### Estimated Time: 10 minutes

## What You're Building

In this lab, you'll add token management to the dotnet chat flow so long conversations stay within a configured prompt budget.

At this checkpoint, the backend already has:

- few-shot prompting in the system prompt
- query compression and semantic routing
- ONNX reranking for retrieved context
- short-term and long-term memory integration

The missing step is prompt-window management for working-memory history.

## Prerequisites Check

Before starting, ensure you have:

- completed Lab 7 successfully
- few-shot prompting in place
- the Redis Agent Memory Server running
- a working frontend build

## Setup Instructions

### Step 1: Review Token Configuration

Open `backend-dotnet-layer/Configuration/ChatMemoryOptions.cs` and `backend-dotnet-layer/appsettings.json`.

This branch already includes the token budget setting:

```json
"ChatMemory": {
  "MaxPromptTokens": 768
}
```

### Step 2: Wire Token Windowing Into the Chat Flow

Open `backend-dotnet-layer/Services/OpenAiChatService.cs`.

Right now, the starter branch still sends the full working-memory history:

```csharp
// TODO: Use ChatHistoryWindowingService to keep the prompt within a token budget.
var history = workingMemoryChat.ToChatHistory(SystemPrompt);
history.AddUserMessage(augmentedUserMessage);
```

Replace that with logic that uses `ChatHistoryWindowingService` to build a windowed history before the user message is sent to the model.

### Step 3: Review the Windowing Helper

Open `backend-dotnet-layer/Services/ChatHistoryWindowingService.cs`.

This helper already contains the token-estimation and window-selection logic. Your task in this lab is to wire it into the active chat path.

### Step 4: Rebuild and Run the Backend

```bash
docker compose up -d
cd frontend-layer
npm run build
cd ..
dotnet run --project backend-dotnet-layer
```

The ASP.NET Core backend listens on `http://localhost:8081` and serves the built frontend.

## Testing Token Management

### Test Token Window Behavior

1. Open `http://localhost:8081`
2. Have a longer conversation with several detailed prompts
3. Verify older working-memory messages are trimmed from the prompt window
4. Confirm recent turns are preserved

### Verify Context Preservation

1. Ask several long questions in the same session
2. Watch the backend logs for token-window trimming
3. Confirm the assistant still remembers the most recent turns

### Test With Different Token Limits

Temporarily adjust `ChatMemory:MaxPromptTokens` or the matching environment variable and observe how aggressively the history is pruned.

## Understanding the Code

### 1. `ChatHistoryWindowingService`

- estimates token usage for the system prompt, working-memory history, and pending user message
- keeps the most recent messages that fit within the configured budget
- trims older messages first

### 2. Prompt Budget Management

- reserves space for the system prompt
- keeps the latest conversational context
- avoids oversized prompts before the OpenAI request is sent

## What's Still Missing?

Your application now has:

- few-shot prompting
- optimized retrieval
- token-aware prompt construction

But it still lacks:

- semantic caching for repeated questions

That comes next.

## Next Steps

You're ready for [Lab 9: Implementing Semantic Caching for Conversations](../lab-9-starter/README.md).

- switch to the `lab-9-starter` branch

```bash
git checkout lab-9-starter
```
