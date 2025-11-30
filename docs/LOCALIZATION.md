# Localization (i18n) Documentation

**Version:** 1.0  
**Last Updated:** December 2025

## Table of Contents

1. [Overview](#overview)
2. [Backend Localization](#backend-localization)
3. [Frontend Localization](#frontend-localization)
4. [Language Detection](#language-detection)
5. [Resource Files](#resource-files)
6. [Best Practices](#best-practices)

---

## Overview

The Task Management solution supports full internationalization (i18n) with English and Arabic languages, including Right-to-Left (RTL) support for Arabic.

### Supported Languages

- **English (`en`)**: Default language
- **Arabic (`ar`)**: Full RTL support

### Features

- ✅ Backend error message localization
- ✅ Frontend UI localization (all text, labels, buttons, placeholders)
- ✅ RTL support for Arabic
- ✅ Automatic language detection from HTTP headers
- ✅ ICU message formatting with placeholders
- ✅ Pluralization support

---

## Backend Localization

### Architecture

**Services:**
- `IUserSettingsService`: Detects user language from HTTP headers
- `ILocalizationService`: Retrieves localized strings from resource files
- `LocalizedErrorFactory`: Creates localized error instances

**Components:**
- `BaseController`: Localizes errors before sending responses
- Resource files: `Resources/en.json` and `Resources/ar.json`

### Language Detection

**Priority Order:**
1. `X-Locale` HTTP header (primary)
2. `Accept-Language` HTTP header (fallback)
3. Default: English (`en`)

**Implementation:**
```csharp
public class UserSettingsService : IUserSettingsService
{
    public string GetLanguage()
    {
        // Check X-Locale header first
        var xLocale = _httpContextAccessor.HttpContext?.Request.Headers["X-Locale"].FirstOrDefault();
        if (!string.IsNullOrEmpty(xLocale))
            return NormalizeLanguage(xLocale);
        
        // Fallback to Accept-Language
        var acceptLanguage = _httpContextAccessor.HttpContext?.Request.Headers["Accept-Language"].FirstOrDefault();
        if (!string.IsNullOrEmpty(acceptLanguage))
            return NormalizeLanguage(acceptLanguage);
        
        return "en"; // Default
    }
}
```

### Error Localization

**Error Creation with Message Key:**
```csharp
var error = Error.Validation(
    "Progress must be at least 50%", 
    "ProgressPercentage", 
    "Errors.Tasks.ProgressMinNotMet" // Message key
);
```

**Localization in BaseController:**
```csharp
protected string LocalizeError(Error error)
{
    if (string.IsNullOrEmpty(error.MessageKey))
        return error.Message;
    
    var language = _userSettingsService.GetLanguage();
    var localized = _localizationService.GetString(
        error.MessageKey, 
        error.Message, 
        language
    );
    
    // Handle formatted messages with placeholders
    return localized;
}
```

### Resource Files

**Location:** `src/TaskManagement.Application/Resources/`

**Structure:**
```json
{
  "Errors": {
    "Tasks": {
      "NotFound": "Task not found",
      "NotFoundById": "Task with ID '{0}' not found",
      "TitleRequired": "Title is required",
      "ProgressMinNotMet": "Progress must be at least {0}% (last approved progress). You can only increase the progress.",
      "OnlyCreatorCanAcceptProgress": "Only the task creator can accept progress updates"
    }
  }
}
```

**Adding New Error Keys:**
1. Add key to `Resources/en.json`
2. Add corresponding translation to `Resources/ar.json`
3. Use key in error creation: `Error.Validation(..., "Errors.Tasks.YourKey")`

---

## Frontend Localization

### Configuration

**i18n Instance:**
```typescript
// web/src/i18n/instance.ts
export function getI18nInstance(locale: SupportedLocale): I18nInstance {
  const instance = createInstance();
  instance
    .use(initReactI18next)
    .use(new ICU())
    .init({
      resources,
      lng: normalized,
      fallbackLng: DEFAULT_LOCALE,
      supportedLngs: ["en", "ar"],
      defaultNS: "common",
      ns: namespaces,
      returnNull: false,
      interpolation: {
        escapeValue: false
      }
    });
  return instance;
}
```

### Usage

**Basic Translation:**
```typescript
const { t } = useTranslation(["tasks", "common"]);

<h1>{t("tasks:forms.create.title")}</h1>
<Button>{t("common:actions.save")}</Button>
```

**With Placeholders:**
```typescript
{t("tasks:list.summary", {
  start: 1,
  end: 10,
  total: 50
})}
// Output: "Showing 1–10 of 50"
```

**With Pluralization:**
```typescript
{t("tasks:attachments.upload.selectedFiles", { count: files.length })}
// Output: "1 file selected" or "5 files selected"
```

### API Client Integration

**Automatic Locale Headers:**
```typescript
// web/src/core/api/client.shared.ts
export function buildHeaders(config?: ApiRequestConfig): Headers {
  const headers = new Headers();
  
  if (config?.locale) {
    headers.set("Accept-Language", config.locale);
    headers.set("X-Locale", config.locale);
    headers.set("X-Locale-Dir", config.locale === "ar" ? "rtl" : "ltr");
  }
  
  return headers;
}
```

**Usage in Hooks:**
```typescript
export function useTasksQuery(filters: TaskListFilters) {
  const locale = useCurrentLocale();
  
  return useQuery({
    queryKey: taskKeys.list(filters),
    queryFn: () => apiClient.request({
      url: "/api/tasks",
      method: "GET",
      locale: locale // Automatically sets headers
    })
  });
}
```

### RTL Support

**Automatic Direction:**
```typescript
// web/src/core/providers/I18nProvider.tsx
useEffect(() => {
  document.documentElement.dir = locale === "ar" ? "rtl" : "ltr";
  document.documentElement.lang = locale;
}, [locale]);
```

**Tailwind Logical Utilities:**
```tsx
// Use logical utilities instead of left/right
<div className="ms-4 me-2 start-0 end-auto">
  <Button className="ps-4 pe-2">Save</Button>
</div>
```

**Components with RTL Support:**
- DatePicker: Calendar positioning adjusts for RTL
- Select dropdowns: Arrow position flips automatically
- All UI components: Use logical utilities

### Translation Keys Structure

**Namespaces:**
- `common`: Shared translations (actions, states, filters)
- `tasks`: Task-related translations
- `auth`: Authentication translations
- `dashboard`: Dashboard translations
- `validation`: Validation messages

**Key Structure:**
```
{namespace}:{section}.{subsection}.{key}
```

**Examples:**
- `tasks:forms.create.title` - Create task form title
- `tasks:forms.progress.fields.progressPercentage` - Progress percentage field label
- `common:actions.save` - Save button label
- `validation:required` - Required field validation message

### Resource Files

**Location:** `web/src/i18n/resources/`

**Structure:**
```
resources/
├── en/
│   ├── common.json
│   ├── tasks.json
│   ├── auth.json
│   └── validation.json
└── ar/
    ├── common.json
    ├── tasks.json
    ├── auth.json
    └── validation.json
```

**Example (tasks.json):**
```json
{
  "forms": {
    "create": {
      "title": "Create task",
      "fields": {
        "title": "Title",
        "titlePlaceholder": "Enter task title...",
        "description": "Description",
        "descriptionPlaceholder": "Enter task description..."
      }
    },
    "progress": {
      "minProgressHint": "Minimum: {min}% (last approved progress). You can only increase the progress.",
      "minProgressError": "Progress must be at least {min}% (last approved progress). You can only increase the progress."
    }
  },
  "history": {
    "statusNames": {
      "0": "Created",
      "1": "Assigned",
      "2": "Under Review",
      "3": "Accepted",
      "4": "Rejected",
      "5": "Completed",
      "6": "Cancelled",
      "7": "Pending Manager Review",
      "8": "Rejected By Manager"
    },
    "statusFallback": "Status {status}"
  }
}
```

---

## Language Detection

### Frontend

**Current Locale Hook:**
```typescript
// web/src/core/routing/useCurrentLocale.ts
export function useCurrentLocale(): SupportedLocale {
  const { i18n } = useTranslation();
  return normalizeLocale(i18n.language) as SupportedLocale;
}
```

**Locale Switcher:**
```typescript
// web/src/core/routing/LocaleSwitcher.tsx
const toggleLocale = () => {
  const newLocale = currentLocale === "en" ? "ar" : "en";
  router.push(`/${newLocale}${pathname.slice(3)}`);
};
```

### Backend

**HTTP Headers:**
- `X-Locale`: Primary language preference (e.g., `en`, `ar`)
- `Accept-Language`: Fallback language preference (e.g., `en-US`, `ar-SA`)

**Normalization:**
- `en-US` → `en`
- `ar-SA` → `ar`
- Unknown → `en` (default)

---

## Best Practices

### Backend

1. **Always provide MessageKey**: Use message keys for all user-facing errors
2. **Use placeholders**: For dynamic values, use format placeholders (e.g., `{0}`, `{min}`)
3. **Consistent naming**: Follow `Errors.{Entity}.{ErrorType}` pattern
4. **Update both languages**: Always add translations to both `en.json` and `ar.json`
5. **Test localization**: Verify errors display correctly in both languages

### Frontend

1. **Use namespaces**: Organize translations by feature/domain
2. **Consistent key structure**: Follow `{namespace}:{section}.{subsection}.{key}` pattern
3. **Use placeholders**: For dynamic content, use ICU placeholders
4. **RTL-aware**: Use logical Tailwind utilities (`start`, `end`, `ms`, `me`)
5. **Complete coverage**: Ensure all UI text is localized (no hardcoded strings)
6. **Test both languages**: Verify UI displays correctly in English and Arabic

### Adding New Translations

**Backend:**
1. Add key to `Resources/en.json`
2. Add corresponding translation to `Resources/ar.json`
3. Use key in error creation

**Frontend:**
1. Add key to `resources/en/{namespace}.json`
2. Add corresponding translation to `resources/ar/{namespace}.json`
3. Use key in components: `t("{namespace}:{key}")`

---

## See Also

- [Error Handling](ERROR_HANDLING.md) - Localized error handling
- [Technical Guidelines](SOLUTION_TECHNICAL_GUIDELINES.md) - Internationalization section
- [Architecture](ARCHITECTURE.md) - System architecture


