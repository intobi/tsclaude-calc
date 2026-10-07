---
name: deploy
description: Build and prepare the calculator (.NET API + Angular frontend) for deployment. Use when the user asks to deploy, publish, or release the project.
disable-model-invocation: true
---

# Deploy

Sample deployment skill. Adjust the final step to your real hosting target.

## Steps

1. **Check the working tree.** Run `git status`. If there are uncommitted changes, stop and tell the user.
2. **Build the backend.**
   ```
   cd backend/Calculator.Api && dotnet publish -c Release -o ../../artifacts/api
   ```
3. **Build the frontend.**
   ```
   cd frontend && npm ci && npx ng build --configuration production
   ```
   Output goes to `frontend/dist/frontend/browser`.
4. **Verify.** Start the published API (`dotnet artifacts/api/Calculator.Api.dll`), call `POST /api/calculate` with `{"a":2,"b":3,"operation":"add"}` and expect `{"result":5}`. Then stop it.
5. **Ask before publishing.** Show the user what was built and ask where to deploy (e.g. Azure App Service, Docker, static hosting). Never push to a server without explicit confirmation.

## Notes
- Do not commit `artifacts/`; it is build output.
- Report any failed step with its output instead of continuing.
