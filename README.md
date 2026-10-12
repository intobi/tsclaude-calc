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
| `Calculator.Contracts` | Interfaces and models only (services, repositories, errors, settings). Shipped as a NuGet package |
| `Calculator.Services` | Business logic implementing the service contracts: calculations, history, login/JWT |
| `Calculator.Data` | EF Core entities, `AppDbContext`, migrations and repositories implementing the persistence contracts |
| `Calculator.Api` | Controllers and request/response DTOs coded against the contracts; `Program.cs` is the composition root |
| `Calculator.Tests` | Unit, API integration and architecture tests |

```
            Calculator.Contracts   (interfaces + models)
              ▲        ▲        ▲
   Calculator.Services  │  Calculator.Data      ← implementations, unaware of each other
              ▲        │        ▲
              └── Calculator.Api ┘               ← wires implementations via DI in Program.cs
```

`Calculator.Services` and `Calculator.Data` never reference each other; `LayerDependencyTests` enforce it.
Each implementation registers itself through a `DependencyInjection` extension method.

### Contracts as a NuGet package
```
cd backend
dotnet pack Calculator.Contracts -c Release -o artifacts/packages      # publish to the local feed
dotnet build Calculator.slnx -p:UseContractsPackage=true               # consume the package instead of the source
```
The version lives in `backend/Directory.Build.props` (`ContractsVersion`); bump it on every contract change,
because NuGet caches a published version.
