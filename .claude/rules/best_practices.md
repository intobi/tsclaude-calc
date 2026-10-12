# Development best practices

Always follow established best practices; do not take shortcuts.

## Frontend (Angular)
- Style with Tailwind CSS v4 utility classes in templates. No inline styles (`style=""`, `[style.*]`).
- Design tokens (colors, fonts, radii, shadows, easing, breakpoints) live in `@theme` in `frontend/src/tailwind.css`; only those colors exist as utilities. Never hardcode colors or fonts.
- Global non-utility styles go in `frontend/src/styles.scss`; component styles, when Tailwind is not enough, go in `.scss` files.
- Use `rem`/Tailwind spacing for sizes, mobile-friendly layouts, and `prefers-reduced-motion`.
- Accessibility: semantic elements, `aria-label` for icon/symbol-only controls, visible `:focus-visible` styles.
- Prefer data-driven templates (`@for`) over repeated markup; keep component logic typed and `private` unless used by the template.
- Tests select elements by role, text, `aria-*` or `data-*` attributes, not by styling classes.
- Format with Prettier (it also sorts Tailwind classes) and keep unit tests passing (`npx ng test --watch=false`); update tests when behavior changes.

## Backend (.NET)
- Interfaces and shared models live in `Calculator.Contracts` (no implementations, no third-party dependencies, XML docs on public types). It is versioned via `ContractsVersion` in `backend/Directory.Build.props` and packable as NuGet.
- `Calculator.Services` (business logic) and `Calculator.Data` (EF Core entities, `AppDbContext`, repositories, migrations) implement the contracts and must never reference each other; `LayerDependencyTests` enforce this.
- `Calculator.Api` controllers code against contract interfaces only; implementations are referenced solely for DI registration in `Program.cs`. Never inject `AppDbContext` outside `Calculator.Data`, and never expose EF entities outside it.
- Everything is registered through dependency injection via each layer's `DependencyInjection` extension (`AddDataLayer`, `AddApplicationServices`); keep `Program.cs` as a thin composition root.
- Validate input, return proper HTTP status codes, and cover services, repositories and API behaviour with tests in `Calculator.Tests` (`dotnet test backend/Calculator.slnx`).
