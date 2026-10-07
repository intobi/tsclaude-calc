# Development best practices

Always follow established best practices; do not take shortcuts.

## Frontend (Angular)
- No inline styles (`style=""`, `[style.*]`); styling goes in CSS files.
- No hardcoded colors, fonts or spacing in components: use CSS custom properties defined in `frontend/src/styles.css`.
- Use `rem` for sizes, mobile-friendly layouts, and `prefers-reduced-motion`.
- Accessibility: semantic elements, `aria-label` for icon/symbol-only controls, visible `:focus-visible` styles.
- Prefer data-driven templates (`@for`) over repeated markup; keep component logic typed and `private` unless used by the template.
- Format with Prettier and keep unit tests passing (`npx ng test --watch=false`); update tests when behavior changes.

## Backend (.NET)
- Validate input, return proper HTTP status codes, keep logic out of `Program.cs` once it grows.
