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

## Backend layout
| Project | Responsibility |
|---|---|
| `Calculator.Api` | Controllers, request/response DTOs, auth & CORS setup, composition root |
| `Calculator.Services` | Business logic: calculations, history, login/JWT |
| `Calculator.Data` | EF Core entities, `AppDbContext`, repositories, migrations |
| `Calculator.Tests` | xUnit tests for services and repositories |

Dependencies point one way: Api → Services → Data. Each layer registers itself through a `DependencyInjection` extension method.
