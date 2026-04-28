# Lab 6: Implementing Query Compression and Context Reranking

## Learning Objectives

By the end of this lab, you will:
- implement query compression to optimize retrieval queries
- configure content reranking to prioritize relevant information
- wire up the local ONNX reranker assets already included in the repo
- reduce context noise before it reaches the chat completion prompt

#### Estimated Time: 15 minutes

## What You're Building

In this lab, you'll optimize the dotnet RAG pipeline by adding query compression and content reranking. The retrieval path is already in place on this branch:

- `RetrievalAugmentorService` routes the query between user memory and knowledge-base retrieval
- `SemanticRoutingService` chooses the retriever path
- `RerankingService` is present, but currently runs in pass-through mode

Your job is to finish the two missing optimization steps:

- compress the retrieval query before routing and search
- rerank the retrieved context with the local ONNX scorer

## Prerequisites Check

Before starting, ensure you have:

- completed Lab 5 successfully
- short-term and long-term memory working
- PDF ingestion and knowledge-base search working
- the frontend build available locally

## Setup Instructions

### Step 1: Review the ONNX Asset Configuration

Open `backend-dotnet-layer/Configuration/RerankingOptions.cs` and review the configured local asset paths:

```csharp
public sealed class RerankingOptions
{
    public const string SectionName = "Reranking";

    public string ModelPath { get; init; } =
        "Assets/ms-marco-MiniLM-L-6/model.onnx";

    public string TokenizerPath { get; init; } =
        "Assets/ms-marco-MiniLM-L-6/tokenizer.json";
}
```

The ONNX model and tokenizer are already in the repo, so you do not need to download them during the lab.

### Step 2: Implement Query Compression

Open `backend-dotnet-layer/Services/RetrievalAugmentorService.cs`.

Right now, the starter branch uses the raw user message for retrieval:

```csharp
private Task<string> CompressQueryAsync(string userMessage, CancellationToken cancellationToken)
{
    // TODO: Implement query compression for retrieval. For now we use the raw user message.
    return Task.FromResult(userMessage);
}
```

Replace that pass-through implementation with a Semantic Kernel chat-completion call that rewrites verbose requests into short retrieval queries.

### Step 3: Implement ONNX Content Reranking

Open `backend-dotnet-layer/Services/RerankingService.cs`.

Right now, the starter branch just returns the first `TopN` candidates:

```csharp
public async Task<IReadOnlyList<RerankedContextItem>> RerankAsync(
    string query,
    IReadOnlyList<string> candidates,
    CancellationToken cancellationToken)
{
    await Task.CompletedTask;

    // TODO: Replace this pass-through implementation with ONNX reranking.
    return candidates
        .Take(_options.TopN)
        .Select(text => new RerankedContextItem(text, null))
        .ToList();
}
```

Update that method to:

- build an `OnnxTextReranker`
- score the retrieved candidates against the compressed query
- return only the highest-scoring context items

The project already references the local `RedisVL.Rerankers.Onnx` package from the sibling `redis-vl-dotnet` checkout.

### Step 4: Rebuild and Run the Backend

```bash
docker compose up -d
cd frontend-layer
npm run build
cd ..
dotnet run --project backend-dotnet-layer
```

The ASP.NET Core backend listens on `http://localhost:8081` and serves the built frontend.

## Testing Query Compression and Reranking

### Test Query Compression

1. Open `http://localhost:8081`
2. Ask a verbose question such as: `Can you please tell me what my favorite programming language is based on what you remember about me?`
3. Watch the backend logs and compare the raw query to the compressed retrieval query
4. Verify the response still uses the correct memory

### Test Content Reranking

1. Ask a question that can match both user memory and knowledge-base content
2. Observe the candidate counts and selected context in the backend logs
3. Verify that low-relevance context is filtered out after reranking

## Understanding the Code

### 1. `RetrievalAugmentorService`

- compresses the incoming query before retrieval
- routes retrieval between user memory and the knowledge base
- injects the final reranked context into the prompt

### 2. `SemanticRoutingService`

- uses the RedisVL semantic router to choose the retrieval path
- routes to `user-memory`, `knowledge-base`, or `hybrid`
- falls back to querying all retrievers if routing fails

### 3. `RerankingService`

- receives the merged retrieval candidates
- runs local ONNX scoring with the MS MARCO MiniLM model
- returns only the top-ranked context items

## What's Still Missing?

Your application now has optimized retrieval, but still lacks:
- no few-shot examples in the system prompt
- no token-window management for long conversations
- no semantic caching for repeated questions

Those come next.

## Next Steps

You're ready for [Lab 7: Implementing Few-shot Learning in System Prompts](../lab-7-starter/README.md).

- switch to the `lab-7-starter` branch

```bash
git checkout lab-7-starter
```
