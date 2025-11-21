"use client";

import { Listbox, Transition } from "@headlessui/react";
import { ChevronDownIcon, ChevronUpIcon } from "@heroicons/react/24/outline";
import { Fragment, forwardRef } from "react";
import type { ComponentProps } from "react";
import type React from "react";

import { getDirection } from "@/core/routing/locales";
import { useCurrentLocale } from "@/core/routing/useCurrentLocale";
import { cn } from "@/ui/utils/cn";

export interface SelectOption<T = string> {
  value: T;
  label: string;
  disabled?: boolean;
}

interface SelectProps<T = string> extends Omit<ComponentProps<"button">, "onChange" | "value"> {
  options: SelectOption<T>[];
  value?: T;
  onChange?: (value: T) => void;
  placeholder?: string;
  disabled?: boolean;
  className?: string;
  error?: boolean;
}

const SelectComponent = <T = string,>(
  { options, value, onChange, placeholder, disabled, className, error }: SelectProps<T>,
  _ref: React.ForwardedRef<HTMLButtonElement>
) => {
    const locale = useCurrentLocale();
    const direction = getDirection(locale);

    const selectedOption = options.find((option) => option.value === value);

    const listboxProps: {
      value?: T;
      onChange?: (value: T) => void;
      disabled?: boolean;
    } = {};

    if (value !== undefined) {
      listboxProps.value = value;
    }
    if (onChange) {
      listboxProps.onChange = onChange;
    }
    if (disabled !== undefined) {
      listboxProps.disabled = disabled;
    }

    return (
      <Listbox {...listboxProps}>
        {({ open }) => (
          <div className="relative">
            <Listbox.Button
              className={cn(
                "relative flex h-10 w-full cursor-pointer items-center justify-between rounded-md border bg-background px-3 py-2 text-left text-sm text-foreground shadow-sm transition",
                "hover:border-primary focus-visible:border-primary focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 focus-visible:ring-offset-background",
                "disabled:cursor-not-allowed disabled:bg-muted disabled:text-muted-foreground",
                error && "border-destructive",
                direction === "rtl" ? "pl-10" : "pr-10",
                className
              )}
            >
              <span className={cn("block truncate", !selectedOption && "text-muted-foreground")}>
                {selectedOption ? selectedOption.label : placeholder || "Select..."}
              </span>
              <span
                className={cn(
                  "pointer-events-none absolute inset-y-0 flex items-center",
                  direction === "rtl" ? "left-0 pl-3" : "right-0 pr-3"
                )}
              >
                {open ? (
                  <ChevronUpIcon className="h-5 w-5 text-muted-foreground" aria-hidden="true" />
                ) : (
                  <ChevronDownIcon className="h-5 w-5 text-muted-foreground" aria-hidden="true" />
                )}
              </span>
            </Listbox.Button>

            <Transition
              as={Fragment}
              leave="transition ease-in duration-100"
              leaveFrom="opacity-100"
              leaveTo="opacity-0"
            >
              <Listbox.Options
                className={cn(
                  "absolute z-50 mt-1 max-h-60 w-full overflow-auto rounded-md border border-border bg-background py-1 text-sm shadow-lg focus:outline-none",
                  direction === "rtl" ? "left-0" : "right-0"
                )}
              >
                {options.map((option) => {
                  const optionProps: {
                    key: string;
                    value: T;
                    disabled?: boolean;
                    className: (props: { active: boolean; selected: boolean; disabled: boolean }) => string;
                  } = {
                    key: String(option.value),
                    value: option.value,
                    className: ({ active, selected, disabled }) =>
                      cn(
                        "relative cursor-pointer select-none py-2 px-3 transition-colors",
                        active && !selected && "bg-accent text-accent-foreground",
                        selected && "bg-primary text-primary-foreground",
                        disabled && "cursor-not-allowed opacity-50"
                      )
                  };
                  if (option.disabled !== undefined) {
                    optionProps.disabled = option.disabled;
                  }
                  const { key, ...restProps } = optionProps;
                  return (
                    <Listbox.Option key={key} {...restProps}>
                      {({ selected }) => (
                        <>
                          <span className={cn("block truncate", selected && "font-medium")}>
                            {option.label}
                          </span>
                        </>
                      )}
                    </Listbox.Option>
                  );
                })}
              </Listbox.Options>
            </Transition>
          </div>
        )}
      </Listbox>
    );
};

export const Select = forwardRef(SelectComponent) as <T = string>(
  props: SelectProps<T> & { ref?: React.Ref<HTMLButtonElement> }
) => React.ReactElement;

