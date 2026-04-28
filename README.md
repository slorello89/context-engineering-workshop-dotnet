# Lab 2: Enabling Short-term Memory with Chat Memory

## 🎯 Learning Objectives

By the end of this lab, you will:
- Set up the Redis Agent Memory Server for the dotnet workshop
- Implement short-term memory for chat sessions
- Enable conversation continuity within a single session
- Understand how the working-memory store persists message history
- Test memory retention across multiple exchanges

#### 🕗 Estimated Time: 10 minutes

## 🏗️ What You're Building

In this lab, you'll enhance the basic chat application with short-term memory so the AI can remember previous messages within the same conversation session. This includes:

- **WorkingMemoryStore**: ASP.NET Core wrapper around the Agent Memory Server REST APIs
- **WorkingMemoryChat**: Small chat-history adapter that reads and writes the working-memory store
- **Context Preservation**: Maintaining the conversation flow across multiple messages

## 📋 Prerequisites Check

Before starting, ensure you have:

- [ ] Completed Lab 1 successfully
- [ ] Backend application running without errors
- [ ] Frontend application accessible at `http://localhost:8081`
- [ ] OpenAI API key configured and working

## 🚀 Setup Instructions

> 💡 This lab uses the Redis Agent Memory Server from `docker compose`. If you are using a local host terminal, the service is available at `http://localhost:8000`.

### Step 1: Define the Agent Memory Server URL in `.env`

Add this to your `.env` file:

If running from a local host terminal:

```bash
AGENT_MEMORY_SERVER_URL=http://localhost:8000
```

If running from a Dev Container or Codespaces terminal:

```bash
AGENT_MEMORY_SERVER_URL=http://redis-agent-memory-server:8000
```

### Step 2: Start the Support Services

```bash
docker compose up -d
```

### Step 3: Review the Working Memory Store

Open `backend-dotnet-layer/Services/WorkingMemoryStore.cs` and review the code.

This class wraps the Redis Agent Memory Server working-memory REST APIs and is responsible for storing and retrieving chat messages.

### Step 4: Review the Working Memory Chat Adapter

Open `backend-dotnet-layer/Services/WorkingMemoryChat.cs` and review the code.

This class loads chat history for a session, appends new messages, and writes the updated history back to the Agent Memory Server.

### Step 5: Review the Working Memory Configuration

Open `backend-dotnet-layer/Configuration/WorkingMemoryOptions.cs` and `backend-dotnet-layer/appsettings.json`.

Notice the values used for:

- `AgentMemoryServerUrl`
- `DefaultSessionId`
- `Namespace`
- `TimeToLiveInSeconds`

### Step 6: Rebuild and Run the Backend

```bash
dotnet build backend-dotnet-layer/BackendDotnetLayer.csproj
dotnet run --project backend-dotnet-layer
```

### Step 7: Keep the Frontend Running

If you have not already built the frontend for this repo:

```bash
cd frontend-layer
npm install
npm run build
cd ..
```

## 🧪 Testing Your Memory Implementation

### Memory Retention Test

1. Open `http://localhost:8081` in your browser
2. Clear any previous conversation state by refreshing the page
3. Type `Hi, my name is <your-name>` in the chat
4. Verify you receive a response acknowledging your name
5. Type `What is my name?`
6. **Verify the AI now remembers your name** within the same session

This demonstrates that short-term memory is functioning correctly through the Agent Memory Server.

If you want to inspect the stored messages, open Redis Insight at `http://localhost:5540` and look at the stored working-memory keys.

## 🎨 Understanding the Code

### 1. `WorkingMemoryStore`
- Storage wrapper for the Agent Memory Server
- Session-based message isolation through a dedicated namespace
- Temporary conversation storage with a TTL

### 2. `WorkingMemoryChat`
- Lightweight chat-history manager for the current session
- Loads and stores messages through `WorkingMemoryStore`
- Keeps the backend code simple while enabling memory

### 3. Memory Integration in `OpenAiChatService`
- Loads the existing session history
- Adds the new user message
- Sends the full conversation history to OpenAI
- Stores the assistant reply back into working memory

## 🔍 What's Still Missing? (Context Engineering Perspective)

Your application now has short-term memory, but still lacks:
- ❌ **No Long-term Memory**: Memory is lost outside the short-term session window
- ❌ **No Knowledge Base**: No document retrieval yet
- ❌ **No Semantic Search**: No contextual retrieval beyond chat history

**The next labs will address these limitations.**

## 🐛 Troubleshooting

### Common Issues and Solutions

<details>
<summary>AI still doesn't remember previous messages</summary>

Solution:
- Verify `AGENT_MEMORY_SERVER_URL` is set correctly
- Confirm `docker compose up -d` started `redis-agent-memory-server`
- Make sure you are staying in the same browser session while testing
</details>

<details>
<summary>Error connecting to the Agent Memory Server</summary>

Solution:
- Check that `http://localhost:8000` is reachable from your terminal
- If using a containerized workspace, use the sidecar hostname instead of `localhost`
- Review container logs with `docker compose logs redis-agent-memory-server`
</details>

<details>
<summary>Memory disappears after waiting too long</summary>

Solution:
- The Lab 2 working-memory entries use a TTL
- Review `WorkingMemory:TimeToLiveInSeconds` in `appsettings.json`
</details>

## 🎉 Lab Completion

Congratulations! You've successfully:
- ✅ Implemented short-term chat memory
- ✅ Enabled conversation continuity within a session
- ✅ Tested memory retention across multiple messages

## 📚 Additional Resources

- [Redis Agent Memory Server](https://redis.github.io/agent-memory-server/)
- [Semantic Kernel Chat Concepts](https://learn.microsoft.com/semantic-kernel/concepts/ai-services/chat-completion/)

## ➡️ Next Steps

You're ready for [Lab 3: Knowledge Base with Embeddings, Parsers, and Splitters](../lab-3-starter/README.md) where you'll add document ingestion and knowledge-base capabilities.

- Switch to the `lab-3-starter` branch

```bash
git checkout lab-3-starter
```

- Then follow the README instructions
