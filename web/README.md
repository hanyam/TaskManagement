## Task Management Console (Web)

Enterprise-grade React front-end for the Task Management API. Built with Next.js App Router, TypeScript, TanStack Query, React Hook Form, Tailwind CSS, and react-i18next with full Arabic/English support and RTL awareness.

## Prerequisites

- Node.js 20+
- npm 10+
- Running instance of the Task Management API (defaults to `http://localhost:5000`)

## Environment

Create a `.env.local` based on the template below:

```bash
# API Configuration
NEXT_PUBLIC_API_BASE_URL=http://localhost:5010
NEXT_PUBLIC_APP_NAME=Task Management Console

# Debug Logging (Optional - for development)
# See DEBUG_SETUP.md for details
NEXT_PUBLIC_DEBUG_ENABLED=false
NEXT_PUBLIC_DEBUG_LEVEL=all

# Azure AD Configuration (Optional - for SSO)
# See AZURE_AD_QUICKSTART.md for setup instructions
NEXT_PUBLIC_AZURE_AD_CLIENT_ID=your-client-id-here
NEXT_PUBLIC_AZURE_AD_TENANT_ID=your-tenant-id-here
NEXT_PUBLIC_AZURE_AD_REDIRECT_URI=http://localhost:3000
NEXT_PUBLIC_AZURE_AD_SCOPES=api://your-backend-api-client-id/.default,openid,profile,email
```

**Debug Logging**: See [DEBUG_SETUP.md](./DEBUG_SETUP.md) for client-side debug logging configuration.

**Azure AD Setup**: See [AZURE_AD_QUICKSTART.md](./AZURE_AD_QUICKSTART.md) for a 5-minute setup guide, or [docs/AZURE_AD_SETUP.md](./docs/AZURE_AD_SETUP.md) for detailed documentation.

**Note**: Azure AD SSO is optional. If not configured, users can still sign in by manually entering an Azure AD token.

## Scripts

```bash
npm install          # install dependencies
npm run dev          # start dev server (http://localhost:3000)
npm run build        # production build
npm start            # serve production build
npm run lint         # eslint (a11y, security, import order, tailwind)
npm run typecheck    # strict TypeScript check
npm run test         # unit/component tests (vitest + RTL)
npm run test:coverage
```

## Running with Docker Compose

The repository root ships with a `docker-compose.yml` that spins up:

- SQL Server (`sqlserver`)
- Task Management API (`taskmanagement.api`) built from `src/TaskManagement.Api/Dockerfile`
- Next.js web UI (`taskmanagement.web`) built from `web/Dockerfile`

From the repository root:

```bash
docker compose build        # build all images
docker compose up           # launch api + sql + web (http://localhost:3000)
```

Environment variables for the web container (`NEXT_PUBLIC_API_BASE_URL`, `NEXT_PUBLIC_APP_NAME`) are set in `docker-compose.yml`. Adjust them if your API runs on a different host/port.

## Project Layout

```
src/
 ├─ app/                 # Next.js App Router structure
 │   └─ [locale]/...     # locale-aware routes (en/ar)
 ├─ core/                # cross-cutting concerns (api, auth, routing, providers, theme)
 ├─ features/            # vertical slices (tasks, dashboard, auth)
 ├─ i18n/                # translation resources and helpers
 └─ ui/                  # shared UI primitives (buttons, inputs, layout)
```

## Key Features

- **Internationalization:** react-i18next + i18next-icu with locale-aware formatting, dynamic locale switching, and RTL support.
  - Language switcher button (shows opposite language indicator)
  - RTL-aware UI components (selects, date pickers, navigation)
  - Automatic date formatting based on locale
- **State Management:** TanStack Query for server state, minimal local state for view concerns.
- **Unified API Client:** Envelope parsing, centralized error taxonomy, automatic auth header injection, idempotency support.
- **Tasks Module:** Paginated virtualized table, full lifecycle actions (assign, progress updates, extensions), detail view with action modals.
- **Dashboard:** KPI cards driven by `/dashboard/stats`.
- **Auth:** Azure AD token exchange with direct backend API communication and client-side session management.
- **UI Components:** 
  - Beautiful calendar date picker (`DatePicker`) with RTL support using `react-day-picker`
  - RTL-aware select dropdowns with proper arrow positioning
  - Consistent design system with Tailwind CSS
- **Tooling:** ESLint (a11y, security, Tailwind), Prettier, Vitest + RTL, strict TypeScript.

## Testing

Vitest runs in `jsdom` with React Testing Library. Add new tests under `src/**/__tests__`. Use MSW or custom mocks for networked hooks.

## Contributing

1. Branch from `main`.
2. Add or update translations in `src/i18n/resources/{locale}`.
3. Keep vertical slice structure (`src/features/<Feature>`).
4. Update `features-map.json` when introducing new endpoints or UI surfaces.
5. Run `npm run lint && npm run typecheck && npm run test`.

## Further Reading

- [Azure AD Quick Start](./AZURE_AD_QUICKSTART.md) - 5-minute Azure AD SSO setup
- [Azure AD Setup Guide](./docs/AZURE_AD_SETUP.md) - Detailed authentication configuration
- [Direct Backend Authentication](./docs/DIRECT_BACKEND_AUTH.md) - How authentication works (updated flow)
- [I18n & RTL Guide](./docs/I18N.md) - Internationalization best practices
- [UI Components Guide](./docs/UI_COMPONENTS.md) - Reusable UI components (DatePicker, LocaleSwitcher, etc.)
- [Action Modals Guide](./docs/ActionModals.md) - Extending task action modals
- `docs/` in the repository root for API domain knowledge
- `features-map.json` for endpoint/UI alignment
