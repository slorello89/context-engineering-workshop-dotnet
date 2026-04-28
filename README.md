# Lab 7: Implementing Few-shot Learning in System Prompts

## Learning Objectives

By the end of this lab, you will:
- understand few-shot prompting patterns for LLMs
- implement example-based guidance in the system prompt
- improve response consistency with concrete demonstrations
- guide the assistant's context handling with prompt examples

#### Estimated Time: 5 minutes

## What You're Building

In this lab, you'll enhance the dotnet system prompt with few-shot examples so the assistant uses retrieved context more consistently.

At this checkpoint, the backend already has:

- short-term and long-term memory
- knowledge-base ingestion and retrieval
- query compression
- semantic routing
- local ONNX reranking

The only missing piece is prompt guidance through examples.

## Prerequisites Check

Before starting, ensure you have:

- completed Lab 6 successfully
- query compression and reranking working
- the system prompt available in `OpenAiChatService`
- a few test questions ready for comparison

## Setup Instructions

### Step 1: Review the Current System Prompt

Open `backend-dotnet-layer/Services/OpenAiChatService.cs` and find the `SystemPrompt` constant:

```csharp
private const string SystemPrompt = """
    You are an AI assistant that should act, talk, and behave as if you were J.A.R.V.I.S AI
    from the Iron Man movies...
    """;
```

### Step 2: Add Few-shot Examples to the System Prompt

Replace the current prompt with an expanded version that includes example user questions, context, and expected responses.

The few-shot examples should teach the assistant how to:

- use only the relevant context items
- ignore unrelated memories
- answer cleanly when no relevant context exists
- combine multiple relevant facts when needed
- answer from uploaded document knowledge when appropriate

### Step 3: Rebuild and Run the Backend

```bash
docker compose up -d
cd frontend-layer
npm run build
cd ..
dotnet run --project backend-dotnet-layer
```

The ASP.NET Core backend listens on `http://localhost:8081` and serves the built frontend.

## Testing Few-shot Learning Impact

### Test Context Selection

1. Open `http://localhost:8081`
2. Ask questions that have mixed relevant and irrelevant context
3. Verify the assistant focuses on the right memory or document snippet

Example:

- Ask: `What's my favorite programming language?`
- The response should focus on the relevant stored programming-language memory

### Test Response Conciseness

1. Ask questions that previously produced wordier answers
2. Verify responses stay short and consistent
3. Check that the J.A.R.V.I.S. tone remains intact

### Test Edge Cases

Try prompts where:

- no relevant context exists
- the user asks about current weather
- multiple relevant memories need to be combined

## Understanding the Code

### 1. Few-shot Prompting

- provides concrete examples of desired behavior
- shows how context should and should not be used
- improves consistency without changing the API contract

### 2. Prompt Design Goals

- keep the J.A.R.V.I.S. tone
- preserve concise answers
- demonstrate context filtering explicitly
- show how to handle missing or irrelevant context

## What's Still Missing?

Your application now has:

- optimized retrieval
- context reranking
- improved prompt guidance

But it still lacks:

- token-window management for long conversations
- semantic caching for repeated questions

Those come next.

## Next Steps

You're ready for [Lab 8: Enabling Token Management to Handle Token Limits](../lab-8-starter/README.md).

- switch to the `lab-8-starter` branch

```bash
git checkout lab-8-starter
```
