# Lab 5: Enabling On-demand Context Management for Memories

## 🎯 Learning Objectives

By the end of this lab, you will:
- Implement long-term memory retrieval for user-specific information
- Enable explicit storage of personal facts and preferences
- Combine user memories with the existing knowledge-base RAG flow
- Improve personalization across sessions
- Test memory persistence outside the short-term chat window

#### 🕗 Estimated Time: 10 minutes

## 🏗️ What You're Building

In this lab, you'll add long-term memory capabilities that allow users to explicitly store personal information, preferences, and facts that persist across sessions. This includes:

- **Long-term Memory Storage**: Persistent user-specific memories
- **Memory Retrieval**: Searching those memories on demand
- **Personalized RAG**: Combining user memories with knowledge-base context

## 📋 Prerequisites Check

Before starting, ensure you have:

- [ ] Completed Lab 4 successfully
- [ ] Knowledge-base RAG working with at least one processed document
- [ ] Redis Agent Memory Server running
- [ ] Short-term memory functioning from Lab 2

## 🚀 Setup Instructions

### Step 1: Review the Memory Service

Open `backend-dotnet-layer/Services/MemoryService.cs` and review these methods:

- `CreateUserMemoryAsync(...)`
- `SearchUserMemoriesAsync(...)`

Those methods already provide the basic storage and retrieval primitives for user-specific long-term memory.

### Step 2: Review the Current Chat Flow

Open `backend-dotnet-layer/Services/OpenAiChatService.cs`.

Right now, the chat flow uses:

- short-term working memory
- knowledge-base retrieval

but it does not yet retrieve user-specific long-term memories.

### Step 3: Add User-memory Retrieval

Update `OpenAiChatService.cs` so that the chat flow also searches user-specific memories using the current session ID and query text.

The augmented context should be able to include:

- user memories
- knowledge-base chunks

inside the same `[Context]` block before the OpenAI call.

### Step 4: Keep the Existing Knowledge-base RAG Flow

The Lab 4 knowledge-base lookup should remain in place. This lab extends that flow rather than replacing it.

### Step 5: Rebuild and Run the Backend

```bash
dotnet build backend-dotnet-layer/BackendDotnetLayer.csproj
dotnet run --project backend-dotnet-layer
```

## 🧪 Testing Your Long-term Memory

### Store Personal Information

Use `curl` to store a user memory directly in the Agent Memory Server.

From a local host terminal:

```bash
curl -X POST http://localhost:8000/v1/long-term-memory/ \
  -H "Content-Type: application/json" \
  -d '{
    "memories": [
      {
        "id": "memory-1",
        "session_id": "user-2bfc7e6e-452f-40d6-b7e7-29855518B052",
        "text": "My favorite programming language is C#",
        "namespace": "long-term-memory",
        "memory_type": "semantic"
      }
    ]
  }'
```

### Test Memory Retrieval

1. Open `http://localhost:8081` in your browser
2. Ask `Which programming language do I enjoy using?`
3. Verify the AI recalls the stored memory

### Store Multiple Memories

Add a few more long-term memories such as:

- a preference
- a birthday
- a work detail

Then ask questions that should combine:

- user-specific memory
- document-backed knowledge

## 🎨 Understanding the Code

### 1. `MemoryService.SearchUserMemoriesAsync(...)`
- Searches long-term user memory by semantic similarity
- Filters results by session/user ID
- Returns memories relevant to the current query

### 2. Long-term + Knowledge-base Context
- Short-term memory keeps recent chat state
- Long-term memory provides persistent personal information
- Knowledge-base retrieval provides document facts
- Together they create a richer context layer

### 3. `OpenAiChatService`
- Remains the orchestration point
- Builds the prompt context for the chat request
- Is the natural place to merge the different memory sources in this lab

## 🔍 What's Still Missing? (Context Engineering Perspective)

Your application now has multi-layer memory, but still lacks:
- ❌ **No Query Compression**: Queries are not rewritten for retrieval
- ❌ **No Content Reranking**: Retrieved context is not scored or prioritized
- ❌ **No Dynamic Routing**: No dedicated router decides which memory source to query
- ❌ **No Token Management**: No prompt-budget management yet

**The next labs will optimize this retrieval flow.**

## 🐛 Troubleshooting

### Common Issues and Solutions

<details>
<summary>Memories are not being recalled</summary>

Solution:
- Verify the `session_id` used during storage matches the session used in chat
- Confirm the memory exists through Redis Insight or the Agent Memory Server API
- Use a more direct query that clearly matches the stored memory
</details>

<details>
<summary>The AI only uses document knowledge</summary>

Solution:
- Confirm `OpenAiChatService` was updated to call `SearchUserMemoriesAsync(...)`
- Verify both user memories and document chunks are appended into the final context
</details>

## 🎉 Lab Completion

Congratulations! You've successfully:
- ✅ Enabled long-term user memory retrieval
- ✅ Combined personal memory with the existing RAG flow
- ✅ Added personalization across sessions

## 📚 Additional Resources

- [Redis Agent Memory Server](https://redis.github.io/agent-memory-server/)
- [Semantic Memory Concepts](https://redis.io/glossary/vector-embeddings/)

## ➡️ Next Steps

You're ready for [Lab 6: Implementing Query Compression and Context Reranking](../lab-6-starter/README.md) where you'll optimize retrieval quality and reduce noisy context.

- Switch to the `lab-6-starter` branch

```bash
git checkout lab-6-starter
```

- Then follow the README instructions
