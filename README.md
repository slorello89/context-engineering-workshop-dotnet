# Lab 1: Set up and deploy the AI application

This branch is the .NET starter checkpoint for Lab 1 of the Context Engineering Workshop. The completed reference implementation remains on `main`.

## Current Checkpoint

Implemented so far:
- Basic ASP.NET Core chat endpoint
- React frontend hosted by the dotnet app
- Docker Compose support services

In this lab, you will:
- validate the workshop environment
- run the frontend and backend together
- confirm the base chat flow works end to end

## Run the Workshop App

```bash
docker compose up -d
cd frontend-layer && npm run build
cd ..
dotnet run --project backend-dotnet-layer
```

The ASP.NET Core backend listens on `http://localhost:8081` and serves the built frontend. The Redis support services run through `docker compose`.

## Branch Flow

- Current branch: `lab-1-starter`
- Next starter branch: `lab-2-starter`
- Completed workshop reference: `main`

## Notes

- These starter branches mirror the Java workshop progression, adapted to the ASP.NET Core / Semantic Kernel implementation.
- Some future-lab support files may still exist in the tree, but the active application behavior on this branch is scoped to the checkpoint for this lab.
