# Lab 9: Implementing Semantic Caching for Conversations

## Learning Objectives

By the end of this lab, you will:
- implement semantic caching for chat responses
- check the cache before making a new OpenAI call
- store fresh responses after cache misses
- reduce repeat model calls for similar prompts

#### Estimated Time: 15 minutes

## What You're Building

In this final lab, you'll add semantic response caching to the dotnet backend so repeated or similar questions can be answered without calling the model every time.

At this checkpoint, the backend already has:

- few-shot prompting
- query compression and semantic routing
- ONNX reranking
- token-windowed prompt construction

The last missing piece is semantic caching around the chat completion call.

## Prerequisites Check

Before starting, ensure you have:

- completed Lab 8 successfully
- token windowing working
- Redis running for the semantic cache index
- a working frontend build

## Setup Instructions

### Step 1: Review the Cache Service

Open `backend-dotnet-layer/Services/ResponseSemanticCacheService.cs`.

This branch already includes the cache operations you need:

- `TryGetResponseAsync(...)`
- `StoreResponseAsync(...)`

### Step 2: Review the Cache Configuration

Open `backend-dotnet-layer/Configuration/ResponseSemanticCacheOptions.cs` and `backend-dotnet-layer/appsettings.json`.

The branch already includes starter cache settings such as:

- cache name
- Redis URL
- distance threshold
- TTL
- per-session filtering

### Step 3: Implement Cache Lookup and Store

Open `backend-dotnet-layer/Services/OpenAiChatService.cs`.

Right now, the starter branch still goes directly to OpenAI:

```csharp
// TODO: Implement semantic caching with ResponseSemanticCacheService before calling OpenAI.
var response = await _chatCompletionService.GetChatMessageContentAsync(
    history,
    executionSettings: executionSettings,
    kernel: _kernel,
    cancellationToken: cancellationToken);
```

Update the chat flow to:

1. check the semantic cache with the current query and session ID
2. return the cached response when a hit is found
3. call OpenAI only on a cache miss
4. store the new response in the semantic cache afterward

### Step 4: Rebuild and Run the Backend

```bash
docker compose up -d
cd frontend-layer
npm run build
cd ..
dotnet run --project backend-dotnet-layer
```

The ASP.NET Core backend listens on `http://localhost:8081` and serves the built frontend.

## Testing Semantic Caching

### Test Cache Miss and Store

1. Open `http://localhost:8081`
2. Ask a question once
3. Confirm the first request goes through the model

### Test Cache Hit

1. Ask the same question again
2. Confirm the second response is served from cache
3. Compare latency and backend logs

### Test Similar Queries

Try multiple phrasings of the same question and observe whether the semantic cache returns the stored answer.

## Understanding the Code

### 1. `ResponseSemanticCacheService`

- stores prompt/response pairs in RedisVL semantic cache
- performs semantic lookup with a distance threshold
- supports per-session filtering to reduce cross-session bleed

### 2. Chat Flow Integration

- cache lookup happens before the model call
- cache store happens after a miss and successful response
- repeated prompts become cheaper and faster

## Completion

Once Lab 9 is finished, `main` is the completed workshop reference branch.
