# Lab 5: Enabling On-demand Context Management for Memories

This branch is the .NET starter checkpoint for Lab 5 of the Context Engineering Workshop. The completed reference implementation remains on `main`.

## Current Checkpoint

Implemented so far:
- Lab 4 knowledge-base RAG
- PDF ingestion running in the background
- short-term memory still available for conversations

In this lab, you will:
- retrieve user-specific long-term memories on demand
- combine personal memories with short-term conversation state
- improve personalized answers without loading everything blindly

## Run the Workshop App

```bash
docker compose up -d
cd frontend-layer && npm run build
cd ..
dotnet run --project backend-dotnet-layer
```

The ASP.NET Core backend listens on `http://localhost:8081` and serves the built frontend. The Redis support services run through `docker compose`.

## Branch Flow

- Current branch: `lab-5-starter`
- Next starter branch: `lab-6-starter`
- Completed workshop reference: `main`

## Notes

- These starter branches mirror the Java workshop progression, adapted to the ASP.NET Core / Semantic Kernel implementation.
- Some future-lab support files may still exist in the tree, but the active application behavior on this branch is scoped to the checkpoint for this lab.
