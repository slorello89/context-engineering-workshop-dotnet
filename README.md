# Lab 3: Knowledge Base with Embeddings, Parsers, and Splitters

This branch is the .NET starter checkpoint for Lab 3 of the Context Engineering Workshop. The completed reference implementation remains on `main`.

## Current Checkpoint

Implemented so far:
- Lab 2 short-term memory
- session-based working memory for chat turns
- same frontend and backend deployment flow

In this lab, you will:
- finish the file-ingestion pipeline for PDFs
- parse and chunk document content
- write those chunks into the knowledge-base namespace

## Run the Workshop App

```bash
docker compose up -d
cd frontend-layer && npm run build
cd ..
dotnet run --project backend-dotnet-layer
```

The ASP.NET Core backend listens on `http://localhost:8081` and serves the built frontend. The Redis support services run through `docker compose`.

## Branch Flow

- Current branch: `lab-3-starter`
- Next starter branch: `lab-4-starter`
- Completed workshop reference: `main`

## Notes

- These starter branches mirror the Java workshop progression, adapted to the ASP.NET Core / Semantic Kernel implementation.
- Some future-lab support files may still exist in the tree, but the active application behavior on this branch is scoped to the checkpoint for this lab.
