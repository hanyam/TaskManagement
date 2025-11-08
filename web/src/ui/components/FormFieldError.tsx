import { cn } from "@/ui/utils/cn";

interface FormFieldErrorProps {
  message?: string;
  id?: string;
}

export function FormFieldError({ message, id }: FormFieldErrorProps) {
  if (!message) {
    return null;
  }

  return (
    <p id={id} className={cn("mt-1 text-sm text-destructive")}>
      {message}
    </p>
  );
}

