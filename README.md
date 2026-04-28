# Lab 7: Implementing Few-shot Learning in System Prompts

This branch is the .NET starter checkpoint for Lab 7 of the Context Engineering Workshop. The completed reference implementation remains on `main`.

## Current Checkpoint

Implemented so far:
- Lab 6 retrieval augmentation
- query compression, routing, and reranking services
- contextual answers built from retrieved memory and knowledge-base content

In this lab, you will:
- expand the system prompt with examples
- make the assistant more consistent when deciding which context to use
- improve response shape without changing the API surface

## Run the Workshop App

```bash
docker compose up -d
cd frontend-layer && npm run build
cd ..
dotnet run --project backend-dotnet-layer
```

The ASP.NET Core backend listens on `http://localhost:8081` and serves the built frontend. The Redis support services run through `docker compose`.

## Branch Flow

- Current branch: `lab-7-starter`
- Next starter branch: `lab-8-starter`
- Completed workshop reference: `main`

## Notes

- These starter branches mirror the Java workshop progression, adapted to the ASP.NET Core / Semantic Kernel implementation.
- Some future-lab support files may still exist in the tree, but the active application behavior on this branch is scoped to the checkpoint for this lab.
