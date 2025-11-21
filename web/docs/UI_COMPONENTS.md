# UI Components Guide

This guide documents reusable UI components and their usage patterns in the Task Management Console.

## DatePicker Component

**Location:** `src/ui/components/DatePicker.tsx`

A beautiful, accessible date picker component built with `react-day-picker` that fully supports RTL and internationalization.

### Features

- **RTL Support:** Automatically adjusts calendar positioning and navigation for Arabic locale
- **i18n Integration:** Uses `date-fns` locales for proper date formatting (English/Arabic)
- **Accessible:** Keyboard navigation, ARIA attributes, focus management
- **Styled:** Matches the application's design system with Tailwind CSS
- **React Hook Form Compatible:** Works seamlessly with `Controller` component

### Usage

#### Basic Usage

```tsx
import { DatePicker } from "@/ui/components/DatePicker";

function MyComponent() {
  const [date, setDate] = useState<string>("");

  return (
    <DatePicker
      value={date}
      onChange={setDate}
      placeholder="Select a date"
    />
  );
}
```

#### With React Hook Form

```tsx
import { Controller, useForm } from "react-hook-form";
import { DatePicker } from "@/ui/components/DatePicker";

function MyForm() {
  const form = useForm();

  return (
    <form>
      <Controller
        name="dueDate"
        control={form.control}
        render={({ field }) => (
          <DatePicker
            value={field.value}
            onChange={field.onChange}
            placeholder="Due date"
          />
        )}
      />
    </form>
  );
}
```

### Props

| Prop | Type | Required | Description |
|------|------|----------|-------------|
| `value` | `string` | No | Date value in `yyyy-MM-dd` format |
| `onChange` | `(value: string) => void` | No | Callback when date is selected |
| `placeholder` | `string` | No | Placeholder text for the input |
| `disabled` | `boolean` | No | Disables the date picker |
| `className` | `string` | No | Additional CSS classes |

### Implementation Details

- Uses `react-day-picker` v8+ with custom styling
- Automatically detects current locale via `useCurrentLocale()` hook
- Formats displayed date using `date-fns` `format(date, "PPP", { locale })`
- Returns date value in ISO format (`yyyy-MM-dd`) for API compatibility
- Calendar popover positioned correctly for both LTR and RTL layouts

### Styling

Custom styles are defined in `src/app/globals.css` under the `.rdp` class. The component uses CSS variables from the design system:

- `--color-primary` for selected dates
- `--color-accent` for hover states and today's date
- `--color-background` for calendar background
- `--color-foreground` for text

## Select Dropdowns

**RTL Support:** All native `<select>` elements automatically have RTL-aware arrow positioning via CSS in `globals.css`.

### Features

- Arrow icon flips position based on `dir` attribute
- Custom SVG arrow replaces browser default
- Proper padding adjustments for RTL layouts

### Usage

```tsx
<select className="h-10 rounded-md border border-input bg-background px-3 text-sm">
  <option value="">Select...</option>
  <option value="1">Option 1</option>
</select>
```

The RTL styling is applied automatically via CSS rules in `globals.css`.

## LocaleSwitcher Component

**Location:** `src/core/routing/LocaleSwitcher.tsx`

A compact button component for switching between supported locales (English/Arabic).

### Features

- Shows opposite language indicator:
  - Displays "ع" when current locale is English
  - Displays "en" when current locale is Arabic
- One-click language switching
- Preserves current route and query parameters
- Updates cookie for persistence

### Usage

```tsx
import { LocaleSwitcher } from "@/core/routing/LocaleSwitcher";

function Header() {
  return (
    <header>
      <LocaleSwitcher className="hidden sm:flex" />
    </header>
  );
}
```

### Props

| Prop | Type | Required | Description |
|------|------|----------|-------------|
| `className` | `string` | No | Additional CSS classes |

## Button Component

**Location:** `src/ui/components/Button.tsx`

A flexible button component with multiple variants and sizes.

### Variants

- `primary` - Primary action button (default)
- `secondary` - Secondary action
- `outline` - Outlined button
- `ghost` - Minimal button with hover effect
- `destructive` - For destructive actions
- `link` - Link-style button

### Sizes

- `sm` - Small button
- `md` - Medium button (default)
- `lg` - Large button
- `icon` - Square icon button

### Usage

```tsx
import { Button } from "@/ui/components/Button";

<Button variant="primary" size="md">Save</Button>
<Button variant="outline" size="sm">Cancel</Button>
<Button variant="ghost" size="icon">
  <Icon />
</Button>
```

## Input Component

**Location:** `src/ui/components/Input.tsx`

A styled input component matching the design system.

### Usage

```tsx
import { Input } from "@/ui/components/Input";

<Input 
  type="text" 
  placeholder="Enter text"
  {...register("fieldName")}
/>
```

## FormFieldError Component

**Location:** `src/ui/components/FormFieldError.tsx`

Displays validation error messages below form fields.

### Usage

```tsx
import { FormFieldError } from "@/ui/components/FormFieldError";

{form.formState.errors.title && (
  <FormFieldError message={t(form.formState.errors.title.message)} />
)}
```

## Best Practices

1. **Always use Controller for DatePicker** when integrating with React Hook Form
2. **Use logical Tailwind classes** (`ps-*`, `pe-*`, `ms-*`, `me-*`) for RTL-aware spacing
3. **Test components in both locales** to ensure RTL support works correctly
4. **Follow the design system** - use existing components rather than creating custom ones
5. **Accessibility first** - ensure all interactive components are keyboard navigable

## Component Dependencies

- `react-day-picker` - Date picker functionality
- `date-fns` - Date formatting and locale support
- `@heroicons/react` - Icon library
- `class-variance-authority` - Variant management for buttons
- `tailwind-merge` - Utility for merging Tailwind classes


