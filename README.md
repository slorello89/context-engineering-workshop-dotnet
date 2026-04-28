# Lab 4: Implementing Basic RAG with Knowledge Base Data

This branch is the .NET starter checkpoint for Lab 4 of the Context Engineering Workshop. The completed reference implementation remains on `main`.

## Current Checkpoint

Implemented so far:
- Lab 3 ingestion scaffolding
- knowledge-base service configuration
- background file processor enabled

In this lab, you will:
- inject retrieved document context into chat requests
- answer questions from ingested knowledge-base content
- validate basic RAG behavior

## Run the Workshop App

```bash
docker compose up -d
cd frontend-layer && npm run build
cd ..
dotnet run --project backend-dotnet-layer
```

The ASP.NET Core backend listens on `http://localhost:8081` and serves the built frontend. The Redis support services run through `docker compose`.

## Branch Flow

- Current branch: `lab-4-starter`
- Next starter branch: `lab-5-starter`
- Completed workshop reference: `main`

## Notes

- These starter branches mirror the Java workshop progression, adapted to the ASP.NET Core / Semantic Kernel implementation.
- Some future-lab support files may still exist in the tree, but the active application behavior on this branch is scoped to the checkpoint for this lab.
