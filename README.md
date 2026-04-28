# Lab 4: Implementing Basic RAG with Knowledge Base Data

## 🎯 Learning Objectives

By the end of this lab, you will:
- Implement Retrieval-Augmented Generation (RAG) using the knowledge base
- Search the ingested document chunks for relevant content
- Inject retrieved document context into chat requests
- Enable the AI to answer questions using document knowledge
- Test basic RAG behavior with knowledge-base-backed prompts

#### 🕗 Estimated Time: 20 minutes

## 🏗️ What You're Building

In this lab, you'll connect the knowledge base to the chat interface so the AI can retrieve and use relevant document information when answering questions. This includes:

- **Knowledge Retriever**: Searches the knowledge-base namespace
- **Context Injection**: Formats retrieved document chunks into a `[Context]` block
- **Basic RAG Flow**: Retrieve relevant content, inject it, then call OpenAI

## 📋 Prerequisites Check

Before starting, ensure you have:

- [ ] Completed Lab 3 successfully
- [ ] At least one PDF processed into the knowledge base
- [ ] Redis Agent Memory Server running with stored document chunks
- [ ] Backend application with document processing enabled

## 🚀 Setup Instructions

### Step 1: Review the Knowledge-base Search Method

Open `backend-dotnet-layer/Services/MemoryService.cs` and review `SearchKnowledgeBaseAsync(...)`.

This method already searches the knowledge-base namespace and returns matching chunks.

### Step 2: Review the Chat Flow

Open `backend-dotnet-layer/Services/OpenAiChatService.cs`.

At this point, the chat flow still uses only short-term memory. It does not yet augment the user message with knowledge-base context.

### Step 3: Add Basic Knowledge Retrieval

Update `OpenAiChatService.cs` so that, before calling OpenAI:

- it searches the knowledge base using the current user query
- it selects a small number of relevant document chunks
- it appends them to the user message inside a `[Context]` block

The shape should be similar to:

```text
<original user message>

[Context]
- first relevant chunk
- second relevant chunk
```

### Step 4: Keep the Background File Processor Enabled

Open `backend-dotnet-layer/Program.cs` and confirm the hosted `FilesProcessor` remains registered so newly added PDFs continue to be ingested.

### Step 5: Rebuild and Run the Backend

```bash
dotnet build backend-dotnet-layer/BackendDotnetLayer.csproj
dotnet run --project backend-dotnet-layer
```

## 🧪 Testing Your RAG Implementation

### Basic RAG Test

1. Open `http://localhost:8081` in your browser
2. Ask a question about content from your uploaded PDF
3. Verify the AI uses document information in its response

Example queries:

- `What does the document say about the garage door code?`
- `What information is available about the lock box?`
- `Summarize the details in the uploaded document about exterior access`

## 🎨 Understanding the Code

### 1. `MemoryService.SearchKnowledgeBaseAsync(...)`
- Searches the knowledge-base namespace
- Returns semantically matched chunks
- Provides the retrieval side of basic RAG

### 2. `OpenAiChatService`
- Loads short-term memory as before
- Adds knowledge-base context when available
- Sends the augmented request to OpenAI

### 3. Context Injection
- Keeps the user query intact
- Appends retrieved chunks in a structured block
- Helps the model distinguish between user intent and supporting evidence

## 🔍 What's Still Missing? (Context Engineering Perspective)

Your application now has basic RAG, but still lacks:
- ❌ **No User Memories**: Cannot retrieve personal long-term memories yet
- ❌ **No Query Optimization**: No query compression or rewriting
- ❌ **No Content Reranking**: Retrieved chunks are not prioritized by a scorer
- ❌ **No Dynamic Routing**: No decision-making across multiple retrieval sources

**The next labs will add those capabilities.**

## 🐛 Troubleshooting

### Common Issues and Solutions

<details>
<summary>AI does not use document knowledge</summary>

Solution:
- Verify PDFs were successfully processed into `.processed` files
- Ensure knowledge-base entries exist in the Agent Memory Server
- Confirm `OpenAiChatService` is appending the retrieved context before the OpenAI call
</details>

<details>
<summary>Retrieved content seems irrelevant</summary>

Solution:
- Try more specific queries using terms found in the document
- Check whether the PDF was split into meaningful chunks
- Inspect the stored entries through Redis Insight
</details>

## 🎉 Lab Completion

Congratulations! You've successfully:
- ✅ Implemented basic Retrieval-Augmented Generation
- ✅ Connected the knowledge base to chat responses
- ✅ Enabled document-aware AI answers

## 📚 Additional Resources

- [Retrieval-Augmented Generation Overview](https://redis.io/glossary/retrieval-augmented-generation/)
- [Semantic Kernel Chat Concepts](https://learn.microsoft.com/semantic-kernel/concepts/ai-services/chat-completion/)

## ➡️ Next Steps

You're ready for [Lab 5: Enabling On-demand Context Management for Memories](../lab-5-starter/README.md) where you'll add user-specific long-term memory capabilities.

- Switch to the `lab-5-starter` branch

```bash
git checkout lab-5-starter
```

- Then follow the README instructions
