import { forwardRef } from "react";
import type { LabelHTMLAttributes } from "react";

import { cn } from "@/ui/utils/cn";

type LabelProps = LabelHTMLAttributes<HTMLLabelElement>;

export const Label = forwardRef<HTMLLabelElement, LabelProps>(({ className, children, ...props }, ref) => {
  return (
    <label ref={ref} className={cn("block text-sm font-medium text-muted-foreground", className)} {...props}>
      {children}
    </label>
  );
});

Label.displayName = "Label";

