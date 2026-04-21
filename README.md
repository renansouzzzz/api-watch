# api-watch

Monitor de saúde de APIs e serviços HTTP.

## Tecnologias

- .NET 8 — Minimal API + Worker Service
- PostgreSQL — Banco de dados
- Entity Framework Core — ORM
- Docker — Ambiente local

## Como rodar

```bash
# Banco de dados
cd docker && docker compose up -d postgres

# API
cd src/ApiWatch.Api && dotnet run

# Worker
cd src/ApiWatch.Worker && dotnet run
```
