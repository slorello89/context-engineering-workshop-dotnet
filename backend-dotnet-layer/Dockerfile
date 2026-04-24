FROM node:20-alpine AS frontend-build
WORKDIR /src/frontend-layer
COPY frontend-layer/package*.json ./
RUN npm ci
COPY frontend-layer/ ./
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS backend-build
WORKDIR /src
COPY backend-dotnet-layer/ ./backend-dotnet-layer/
RUN dotnet publish backend-dotnet-layer/BackendDotnetLayer.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=backend-build /app/publish ./
COPY --from=frontend-build /src/frontend-layer/build ./wwwroot
ENTRYPOINT ["dotnet", "BackendDotnetLayer.dll"]
