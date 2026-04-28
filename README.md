# Lab 2: Enabling Short-term Memory with Chat Memory

This branch is the .NET starter checkpoint for Lab 2 of the Context Engineering Workshop. The completed reference implementation remains on `main`.

## Current Checkpoint

Implemented so far:
- Lab 1 deployment flow
- chat endpoint served by ASP.NET Core
- working UI against the dotnet backend

In this lab, you will:
- wire short-term conversation memory through the agent-memory service
- persist turns across requests for the same session
- confirm the assistant can recall recent messages

## Run the Workshop App

```bash
docker compose up -d
cd frontend-layer && npm run build
cd ..
dotnet run --project backend-dotnet-layer
```

The ASP.NET Core backend listens on `http://localhost:8081` and serves the built frontend. The Redis support services run through `docker compose`.

## Branch Flow

- Current branch: `lab-2-starter`
- Next starter branch: `lab-3-starter`
- Completed workshop reference: `main`

## Notes

- These starter branches mirror the Java workshop progression, adapted to the ASP.NET Core / Semantic Kernel implementation.
- Some future-lab support files may still exist in the tree, but the active application behavior on this branch is scoped to the checkpoint for this lab.
