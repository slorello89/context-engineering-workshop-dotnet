# Lab 8: Enabling Token Management to Handle Token Limits

This branch is the .NET starter checkpoint for Lab 8 of the Context Engineering Workshop. The completed reference implementation remains on `main`.

## Current Checkpoint

Implemented so far:
- Lab 7 few-shot prompting
- retrieval and reranking already in place
- complete context-aware chat behavior without prompt-window controls yet

In this lab, you will:
- add a prompt-budget-aware history window
- trim old working-memory turns before prompt overflow
- keep relevant recent context while controlling token growth

## Run the Workshop App

```bash
docker compose up -d
cd frontend-layer && npm run build
cd ..
dotnet run --project backend-dotnet-layer
```

The ASP.NET Core backend listens on `http://localhost:8081` and serves the built frontend. The Redis support services run through `docker compose`.

## Branch Flow

- Current branch: `lab-8-starter`
- Next starter branch: `lab-9-starter`
- Completed workshop reference: `main`

## Notes

- These starter branches mirror the Java workshop progression, adapted to the ASP.NET Core / Semantic Kernel implementation.
- Some future-lab support files may still exist in the tree, but the active application behavior on this branch is scoped to the checkpoint for this lab.
