# tsclaude-calc

Web calculator: .NET 10 Web API (`backend/`) + Angular 22 with Tailwind CSS v4 (`frontend/`).

## Run
```
cd backend/Calculator.Api && dotnet run     # http://localhost:5278
cd frontend && npm install && npm start     # http://localhost:4200 (proxies /api to the backend)
```

Seeded dev accounts: `andrew.t@intobi.com` and `arty.l@intobi.com`, password `1111`.

## Test
```
dotnet test backend/Calculator.slnx
cd frontend && npx ng test --watch=false
```

## Database migrations
`dotnet-ef` is a local tool pinned in `backend/dotnet-tools.json`. Migrations are applied automatically on startup.
```
cd backend
dotnet tool restore
dotnet dotnet-ef migrations add <Name> --project Calculator.Data --startup-project Calculator.Api
```

## Backend layout
| Project | Responsibility |
|---|---|
| `Calculator.Contracts` | Request/response DTOs exchanged with the frontend. No logic, no dependencies |
| `Calculator.Api` | Controllers, the DTO ⇄ entity mapper (`Mapping/DtoMapper`, Mapperly), auth/CORS/rate limiting; `Program.cs` is the composition root |
| `Calculator.Services` | Business logic and its interfaces: calculations, history, login/JWT |
| `Calculator.Data` | EF Core entities, `AppDbContext`, migrations, repositories and their interfaces |
| `Calculator.Tests` | Unit, mapper, API integration and architecture tests |

```
frontend ⇄ JSON ⇄ Calculator.Contracts (DTOs)
                         ▲
                  Calculator.Api ── IDtoMapper: DTO ⇄ entity
                         │
                  Calculator.Services ── business logic on entities
                         │
                  Calculator.Data ── entities, repositories, EF Core
```

Controllers map incoming DTOs to entities, call a service, and map the returned entities back to DTOs.
Services and Data never see the DTOs; `LayerDependencyTests` enforce the dependency direction.
