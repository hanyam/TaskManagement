## Contributing Guide

### Adding a New Feature Slice

1. **Update `features-map.json`:**
   - Document the endpoint(s), DTOs, and intended UI surfaces.
   - Include routes, modals, and pending backend gaps.
2. **Scaffold vertical slice:**
   - Create `src/features/<Feature>/api`, `components`, `hooks`, `types`.
   - Prefer colocated Zod schemas and React Hook Form definitions.
3. **Register localisation:**
   - Add `en`/`ar` strings under `src/i18n/resources/<locale>/<feature>.json`.
   - Use ICU placeholders and mirror key structure across locales.
4. **Hook into API client:**
   - Extend `src/core/api/types` if new envelopes or error shapes appear.
   - Expose `use<Feature>Query/Mutation` helpers with TanStack Query.
5. **Surface UI:**
   - Render pages under `src/app/[locale]/(app)/<feature>` using `AppShell`.
   - Use shared primitives in `src/ui` and keep tailwind classes token-based.
6. **Tests & linting:**
   - Add Vitest coverage for new hooks/utilities.
   - Run `npm run lint && npm run typecheck && npm run test`.

### Internationalisation Checklist

- When adding strings, update both `en` and `ar` resources.
- Use `useTranslation` with namespace prefixes (e.g. `t("tasks:forms.create.title")`).
- For new locales, extend `SUPPORTED_LOCALES` in `src/core/routing/locales.ts`.

### Action Modals / Drawers

- Reuse `@headlessui/react` components (`Dialog`, `Transition`) as shown in existing task actions.
- Map server errors using `form.setError` with `Error.field`.
- Provide optimistic refetch by invalidating TanStack Query cache keys defined in `taskKeys`.

