# Lab 9: Implementing Semantic Caching for Conversations

This branch is the .NET starter checkpoint for Lab 9 of the Context Engineering Workshop. The completed reference implementation remains on `main`.

## Current Checkpoint

Implemented so far:
- Lab 8 token-windowed history
- few-shot prompting plus retrieval augmentation
- bounded prompt construction before the OpenAI call

In this lab, you will:
- add RedisVL semantic response caching
- reuse semantically equivalent answers within a session
- reduce repeated LLM calls while preserving conversation continuity

## Run the Workshop App

```bash
docker compose up -d
cd frontend-layer && npm run build
cd ..
dotnet run --project backend-dotnet-layer
```

The ASP.NET Core backend listens on `http://localhost:8081` and serves the built frontend. The Redis support services run through `docker compose`.

## Branch Flow

- Current branch: `lab-9-starter`
- Next starter branch: `main`
- Completed workshop reference: `main`

## Notes

- These starter branches mirror the Java workshop progression, adapted to the ASP.NET Core / Semantic Kernel implementation.
- Some future-lab support files may still exist in the tree, but the active application behavior on this branch is scoped to the checkpoint for this lab.
