# Debug Logging Setup

## Quick Start

1. Create a `.env.local` file in the `web` directory (if it doesn't exist)

   **Important**: The file must be named `.env.local` (not `.env`) for Next.js to load it automatically.

2. Add the following configuration:

```env
# Enable debug logging
NEXT_PUBLIC_DEBUG_ENABLED=true

# Set log level (all, log, warn, error, none)
NEXT_PUBLIC_DEBUG_LEVEL=all

# Optional: Customize prefix
NEXT_PUBLIC_DEBUG_PREFIX=🔍

# Optional: Show timestamps
NEXT_PUBLIC_DEBUG_SHOW_TIMESTAMP=true

# Optional: Show caller info (file/line)
NEXT_PUBLIC_DEBUG_SHOW_CALLER=false
```

3. **Restart your Next.js dev server** (this is required for env vars to load):

```bash
# Stop the current server (Ctrl+C), then:
npm run dev
```

4. Open your browser console - you should now see:
   - A `[DEBUG LOGGER INIT]` message showing the current configuration
   - All debug logs from the application

## Troubleshooting

### No logs appearing?

1. **Check the initialization message**: When the app loads, you should see a `[DEBUG LOGGER INIT]` message in the console. This shows:
   - Whether debug is enabled
   - The current log level
   - The raw environment variable values

2. **Verify the file name**: Make sure the file is named `.env.local` (not `.env` or `.env.development`)

3. **Check the file location**: The `.env.local` file must be in the `web/` directory (same level as `package.json`)

4. **Restart the dev server**: Environment variables are loaded at server startup. After adding/changing them, you must:
   - Stop the dev server (Ctrl+C)
   - Start it again (`npm run dev`)

5. **Check the browser console**: Open DevTools (F12) and look for the `[DEBUG LOGGER INIT]` message. If you see `enabled: false`, the environment variable isn't being read correctly.

6. **Verify environment variables**: In the browser console, you can check if the variables are loaded:

   ```javascript
   // This won't work in the browser (env vars are replaced at build time)
   // But you can check the initialization log to see what values were read
   ```

7. **Build-time vs Runtime**: `NEXT_PUBLIC_*` variables are embedded at build time. If you're running a production build, you need to rebuild after changing env vars.

### Still not working?

If you see the `[DEBUG LOGGER INIT]` message but logs aren't appearing:

- Check that `enabled: true` in the init message
- Check that `level` is set to `"all"` or `"log"` (not `"none"` or `"error"`)
- Make sure you're triggering actions that actually call the debug functions (e.g., navigate to a task details page)

## Disable Debugging

To disable all debug logs, set:

```env
NEXT_PUBLIC_DEBUG_ENABLED=false
```

Or set the level to `none`:

```env
NEXT_PUBLIC_DEBUG_LEVEL=none
```

## Log Levels

- **`all`**: Show all logs (recommended for development)
- **`log`**: Show log, warn, and error
- **`warn`**: Show warn and error only
- **`error`**: Show error only
- **`none`**: Disable all logs

## Example Output

With debug enabled, you'll see logs like:

```
🔍 [2024-01-15T10:30:00.000Z] [LOG] Component: TaskDetailsViewEmployee - mount { taskId: "..." }
🔍 [2024-01-15T10:30:00.100Z] [LOG] API: GET /tasks/123 { status: 200, hasLinks: true, linksCount: 5 }
🔍 [2024-01-15T10:30:00.200Z] [LOG] Checking link: mark-completed { found: true, linksCount: 5 }
```

## Production

**Important**: Always set `NEXT_PUBLIC_DEBUG_ENABLED=false` in production to avoid exposing debug information to users.
