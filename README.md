# SmartShuffle Engine (.NET + SQL Server)

An integration-ready ASP.NET Core Web API that accepts playlist data and returns a constraint-based “smart shuffle” order.
Created to replace Spotify's psuedo-random shuffling algorithm.

## Tech Stack
- ASP.NET Core Web API (.NET 10)
- Entity Framework Core (EF Core)
- SQL Server (Docker)
- Swagger / OpenAPI

## Features
- **Shuffle-bag**: randomizes without replacement (no repeats until full pass)
- **Recency memory**: persists recent history per `userKey` in SQL Server
- **Artist spacing**: reduces same-artist clustering within a configurable window
- **Metrics**: clustering score, top-artist share, and spacing violations

## Endpoints
- `POST /shuffle` — returns shuffled tracks + metrics
- `POST /reset_history/{userKey}` — clears stored history for a user

## Local Setup (macOS / VS Code)
### 1) Start SQL Server
```bash
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=Your_password123" \
  -p 1433:1433 --name smartshuffle-sql --hostname smartshuffle-sql \
  -d mcr.microsoft.com/mssql/server:2022-latest
