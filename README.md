# Lab 6: Implementing Query Compression and Context Reranking

This branch is the .NET starter checkpoint for Lab 6 of the Context Engineering Workshop. The completed reference implementation remains on `main`.

## Current Checkpoint

Implemented so far:
- Lab 5 hybrid chat context from short-term memory, long-term memory, and knowledge-base matches
- direct retrieval from both memory sources
- context injection before the OpenAI call

In this lab, you will:
- compress user queries before retrieval
- rerank retrieved context for higher relevance
- reduce noisy context before it reaches the prompt

## Run the Workshop App

```bash
docker compose up -d
cd frontend-layer && npm run build
cd ..
dotnet run --project backend-dotnet-layer
```

The ASP.NET Core backend listens on `http://localhost:8081` and serves the built frontend. The Redis support services run through `docker compose`.

## Branch Flow

- Current branch: `lab-6-starter`
- Next starter branch: `lab-7-starter`
- Completed workshop reference: `main`

## Notes

- These starter branches mirror the Java workshop progression, adapted to the ASP.NET Core / Semantic Kernel implementation.
- Some future-lab support files may still exist in the tree, but the active application behavior on this branch is scoped to the checkpoint for this lab.
