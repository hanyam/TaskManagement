## Action Modal Patterns

Task actions (assign, progress updates, extensions) use the same structure:

1. **State** – boolean `useState` flag in the parent page to control visibility.
2. **Dialog** – `@headlessui/react` `Dialog` + `Transition` for accessibility.
3. **Forms** – `react-hook-form` with co-located Zod schemas. Map backend validation using `form.setError`.
4. **Mutations** – TanStack Query mutation hooks from `src/features/tasks/api/queries`. Always invalidate the relevant `taskKeys`.
5. **Toasts** – Use `sonner` for success/failure messaging; avoid blocking `alert()` flows.

### Adding a New Action

1. Extend `features-map.json` with the endpoint and expected UI.
2. Create a mutation hook if it does not exist (follow the `use<Verb><Entity>Mutation` naming).
3. Add a modal component mirroring `UpdateProgressDialog`:
   - `Dialog.Title` for accessibility.
   - Inputs wired to React Hook Form.
   - `Button` (primary/outline) for consistent styling.
4. Wire the modal into `TaskDetailsView` (open state, button trigger).
5. Update translations under `tasks.json` (`forms` + `details.actions`).

