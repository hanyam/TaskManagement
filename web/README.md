## Task Management Console (Web)

Enterprise-grade React front-end for the Task Management API. Built with Next.js App Router, TypeScript, TanStack Query, React Hook Form, Tailwind CSS, and react-i18next with full Arabic/English support and RTL awareness.

## Prerequisites

- Node.js 20+
- npm 10+
- Running instance of the Task Management API (defaults to `http://localhost:5000`)

## Environment

Create a `.env.local` based on the template below:

```bash
NEXT_PUBLIC_API_BASE_URL=http://localhost:5000
NEXT_PUBLIC_APP_NAME=Task Management Console
```

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
- **State Management:** TanStack Query for server state, minimal local state for view concerns.
- **Unified API Client:** Envelope parsing, centralized error taxonomy, automatic auth header injection, idempotency support.
- **Tasks Module:** Paginated virtualized table, full lifecycle actions (assign, progress updates, extensions), detail view with action modals.
- **Dashboard:** KPI cards driven by `/dashboard/stats`.
- **Auth:** Azure AD token exchange via `/api/auth/login` proxy with secure cookie + local storage session sync.
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

- `docs/` in the repository root for API domain knowledge.
- `features-map.json` (mirrored in this folder) for endpoint/UI alignment.
