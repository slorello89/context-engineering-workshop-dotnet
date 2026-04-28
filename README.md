# Lab 1: Set up and deploy the AI application

## 🎯 Learning Objectives

By the end of this lab, you will:
- Set up the .NET workshop development environment
- Run the base ASP.NET Core application with Semantic Kernel and OpenAI
- Build the React frontend and serve it from the .NET backend
- Understand the core architecture for the .NET workshop application
- Verify basic LLM connectivity before adding any context engineering features

#### 🕗 Estimated Time: 20 minutes

## 🏗️ What You're Building

In this foundational lab, you'll deploy a basic AI chat application that will serve as the platform for the rest of the workshop. This includes:

- **React Frontend**: Simple chat interface for testing the application
- **ASP.NET Core Backend**: Web API backend for AI interactions
- **Semantic Kernel Integration**: LLM orchestration through the OpenAI connector
- **OpenAI Connection**: Chat model for generating responses

## 📋 Prerequisites Check

Before starting, confirm the checklist for the setup option you selected:

### Option 1: GitHub Codespaces
- [ ] GitHub Codespace created for this repository
- [ ] Codespace is running and terminal is available
- [ ] OpenAI API key ready

### Option 2: Dev Containers locally
- [ ] Docker up and running on your machine
- [ ] Repository cloned locally
- [ ] Project opened in your IDE Dev Container
- [ ] OpenAI API key ready

### Option 3: Local development
- [ ] .NET 9 SDK installed
- [ ] Docker up and running
- [ ] Git configured and authenticated
- [ ] Node.js 18+ and npm installed
- [ ] OpenAI API key ready
- [ ] Your IDE of choice

## 🚀 Setup Instructions

> 💡 The Redis support services run through `docker compose`, but Lab 1 does not use them yet. They remain available because later labs build on the same environment.

### Step 1: Create an Environment File

```bash
cp .env.example .env
```

### Step 2: Define your OpenAI API Key

Set this in `.env`:

```bash
OPENAI_API_KEY=your-openai-api-key
```

### Step 3: Start the Support Services

```bash
docker compose up -d
```

### Step 4: Build the Frontend

```bash
cd frontend-layer
npm install
npm run build
cd ..
```

### Step 5: Run the Backend Application

```bash
dotnet run --project backend-dotnet-layer
```

The ASP.NET Core application will start on `http://localhost:8081`.

## 🧪 Testing Your Setup

### API Health Check

Test the health endpoint:

```bash
curl http://localhost:8081/health
```

Expected response:

```json
{"status":"UP"}
```

### Basic Chat Test

Test basic chat functionality:

```bash
curl "http://localhost:8081/ai/chat/string?query=Hello"
```

You should receive a plain-text response from the AI.

### Frontend Verification

1. Open `http://localhost:8081` in your browser
2. Type `Hi, my name is <your-name>` in the chat
3. Verify you receive a response from the AI
4. Type `Can you tell me my name?`
5. Verify the AI does not remember your name

## 🎨 Understanding the Code

### 1. `OpenAiChatService.cs`
- Sends a simple prompt to OpenAI through Semantic Kernel
- Uses synchronous request/response chat behavior

### 2. `ChatController.cs`
- Exposes the REST endpoint for chat interactions
- Returns a plain-text response

### 3. `Program.cs`
- Configures ASP.NET Core, CORS, environment loading, and Semantic Kernel
- Serves the built frontend from the .NET application

### 4. `appsettings.json`
- Defines the OpenAI model and basic generation settings

## 🔍 What's Missing? (Context Engineering Perspective)

At this stage, your application lacks:
- ❌ **No Short-term Memory**: Each request is isolated
- ❌ **No Context Awareness**: No prior message history is loaded
- ❌ **No Knowledge Base**: No external document context is available

**This is intentional.** The next labs will add those capabilities step by step.

## 🐛 Troubleshooting

### Common Issues and Solutions

<details>
<summary>Error: OpenAI API key is missing or invalid</summary>

Solution:
- Verify your API key in the `.env` file
- Ensure the key has access to the configured model
- Restart the backend after updating the `.env` file
</details>

<details>
<summary>Error: connection refused on localhost:8081</summary>

Solution:
- Ensure `dotnet run --project backend-dotnet-layer` is still running
- Check whether another process is already using port `8081`
- Review backend startup logs for configuration or OpenAI errors
</details>

<details>
<summary>Error: frontend page shows "Frontend build not found"</summary>

Solution:
- Run `npm install` and `npm run build` in `frontend-layer`
- Restart the backend after the frontend build completes
</details>

## 🎉 Lab Completion

Congratulations! You've successfully:
- ✅ Set up the development environment
- ✅ Deployed the base AI application
- ✅ Verified OpenAI connectivity through the .NET backend

## 📚 Additional Resources

- [Semantic Kernel Documentation](https://learn.microsoft.com/semantic-kernel/)
- [OpenAI API Reference](https://platform.openai.com/docs)
- [ASP.NET Core Documentation](https://learn.microsoft.com/aspnet/core/)

## ➡️ Next Steps

You're ready for [Lab 2: Enabling Short-term Memory with Chat Memory](../lab-2-starter/README.md) where you'll add conversation memory to maintain context across messages.

- Switch to the `lab-2-starter` branch

```bash
git checkout lab-2-starter
```

- Then follow the README instructions
