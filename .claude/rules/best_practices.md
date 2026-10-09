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
- Layers are separate projects: `Calculator.Api` (controllers, request/response DTOs) → `Calculator.Services` (business logic) → `Calculator.Data` (EF Core entities, `AppDbContext`, repositories, migrations).
- Controllers only call service interfaces; services only reach the database through repository interfaces. Never inject `AppDbContext` outside `Calculator.Data`.
- Everything is registered through dependency injection via each layer's `DependencyInjection` extension (`AddDataLayer`, `AddApplicationServices`); keep `Program.cs` as a thin composition root.
- Validate input, return proper HTTP status codes, and cover services and repositories with tests in `Calculator.Tests` (`dotnet test backend/Calculator.slnx`).
