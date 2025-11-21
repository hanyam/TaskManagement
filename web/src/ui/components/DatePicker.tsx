"use client";

import { CalendarIcon } from "@heroicons/react/24/outline";
import { format, getMonth, getYear, setMonth, setYear } from "date-fns";
import { ar, enUS } from "date-fns/locale";
import { forwardRef, useEffect, useState } from "react";
import type { ComponentProps } from "react";
import { DayPicker } from "react-day-picker";

import { getDirection } from "@/core/routing/locales";
import { useCurrentLocale } from "@/core/routing/useCurrentLocale";
import { Select } from "@/ui/components/Select";
import { cn } from "@/ui/utils/cn";

import "react-day-picker/dist/style.css";

interface DatePickerProps extends Omit<ComponentProps<"input">, "value" | "onChange" | "type"> {
  value?: string | undefined;
  onChange?: (value: string) => void;
  placeholder?: string;
}

export const DatePicker = forwardRef<HTMLInputElement, DatePickerProps>(
  ({ value, onChange, placeholder, className, disabled, ...props }, ref) => {
    const locale = useCurrentLocale();
    const [isOpen, setIsOpen] = useState(false);
    const [selectedDate, setSelectedDate] = useState<Date | undefined>(
      value ? new Date(value) : undefined
    );
    const [currentMonth, setCurrentMonth] = useState<Date>(
      selectedDate || new Date()
    );

    // Sync selectedDate with value prop changes
    useEffect(() => {
      if (value && value.trim() !== "") {
        const date = new Date(value);
        if (!isNaN(date.getTime())) {
          setSelectedDate(date);
        } else {
          setSelectedDate(undefined);
        }
      } else {
        setSelectedDate(undefined);
      }
    }, [value]);

    const dateFnsLocale = locale === "ar" ? ar : enUS;
    const direction = getDirection(locale);

    // Update currentMonth when selectedDate changes
    useEffect(() => {
      if (selectedDate) {
        setCurrentMonth(selectedDate);
      }
    }, [selectedDate]);

    function handleSelect(date: Date | undefined) {
      setSelectedDate(date);
      if (date) {
        const dateString = format(date, "yyyy-MM-dd");
        onChange?.(dateString);
      } else {
        onChange?.("");
      }
      setIsOpen(false);
    }

    function handleMonthChange(monthIndex: number) {
      const newDate = setMonth(currentMonth, monthIndex);
      setCurrentMonth(newDate);
    }

    function handleYearChange(year: number) {
      const newDate = setYear(currentMonth, year);
      setCurrentMonth(newDate);
    }

    const displayValue = selectedDate ? format(selectedDate, "PPP", { locale: dateFnsLocale }) : "";

    // Generate month options
    const monthOptions = Array.from({ length: 12 }, (_, i) => ({
      value: i,
      label: format(new Date(2024, i, 1), "MMMM", { locale: dateFnsLocale })
    }));

    // Generate year options (current year ± 10 years)
    const currentYear = getYear(new Date());
    const yearOptions = Array.from({ length: 21 }, (_, i) => currentYear - 10 + i);


    return (
      <div className="relative">
        <div className="relative">
          <input
            ref={ref}
            type="text"
            readOnly
            value={displayValue || ""}
            placeholder={placeholder}
            disabled={disabled}
            onClick={() => !disabled && setIsOpen(!isOpen)}
            className={cn(
              "flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm text-foreground shadow-sm transition hover:border-primary focus-visible:border-primary focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 focus-visible:ring-offset-background disabled:cursor-not-allowed disabled:bg-muted disabled:text-muted-foreground",
              direction === "rtl" ? "pl-10" : "pr-10",
              className
            )}
            {...props}
          />
          <CalendarIcon
            className={cn(
              "absolute top-1/2 h-5 w-5 -translate-y-1/2 text-muted-foreground",
              direction === "rtl" ? "left-3" : "right-3"
            )}
          />
        </div>

        {isOpen && (
          <>
            <div
              className="fixed inset-0 z-40"
              onClick={() => setIsOpen(false)}
              aria-hidden="true"
            />
            <div
              className={cn(
                "absolute z-50 mt-2 w-auto rounded-lg border border-border bg-background p-3 shadow-lg",
                direction === "rtl" ? "left-0" : "right-0"
              )}
            >
              {/* Month and Year Selectors */}
              <div className="flex items-center gap-2 mb-4">
                <Select<number>
                  options={monthOptions.map((option) => ({
                    value: option.value,
                    label: option.label
                  }))}
                  value={getMonth(currentMonth)}
                  onChange={(monthIndex) => handleMonthChange(monthIndex)}
                  className="h-8 min-w-[120px]"
                  aria-label="Select month"
                />
                <Select<number>
                  options={yearOptions.map((year) => ({
                    value: year,
                    label: String(year)
                  }))}
                  value={getYear(currentMonth)}
                  onChange={(year) => handleYearChange(year)}
                  className="h-8 min-w-[100px]"
                  aria-label="Select year"
                />
              </div>
              <DayPicker
                mode="single"
                selected={selectedDate}
                onSelect={handleSelect}
                month={currentMonth}
                onMonthChange={setCurrentMonth}
                locale={dateFnsLocale}
                dir={direction}
                disabled={disabled}
                className="rdp"
                classNames={{
                  months: "flex flex-col sm:flex-row space-y-4 sm:space-x-4 sm:space-y-0",
                  month: "space-y-4",
                  caption: "hidden",
                  nav: "hidden",
                  table: "w-full border-collapse space-y-1",
                  head_row: "flex",
                  head_cell: "text-muted-foreground rounded-md w-9 font-normal text-[0.8rem]",
                  row: "flex w-full mt-2",
                  cell: "h-9 w-9 text-center text-sm p-0 relative [&:has([aria-selected].day-range-end)]:rounded-r-md [&:has([aria-selected].day-outside)]:bg-accent/50 [&:has([aria-selected])]:bg-accent first:[&:has([aria-selected])]:rounded-l-md last:[&:has([aria-selected])]:rounded-r-md focus-within:relative focus-within:z-20",
                  day: cn(
                    "h-9 w-9 p-0 font-normal aria-selected:opacity-100 rounded-md hover:bg-accent hover:text-accent-foreground focus:bg-accent focus:text-accent-foreground transition-colors"
                  ),
                  day_selected:
                    "bg-primary text-primary-foreground hover:bg-primary hover:text-primary-foreground focus:bg-primary focus:text-primary-foreground",
                  day_today: "bg-accent text-accent-foreground font-semibold",
                  day_outside:
                    "day-outside text-muted-foreground opacity-50 aria-selected:bg-accent/50 aria-selected:text-muted-foreground aria-selected:opacity-30",
                  day_disabled: "text-muted-foreground opacity-50",
                  day_range_middle: "aria-selected:bg-accent aria-selected:text-accent-foreground",
                  day_hidden: "invisible"
                }}
                styles={{
                  months: { margin: 0 },
                  month: { margin: 0 },
                  caption: { marginBottom: "1rem" },
                  nav: { margin: 0 }
                }}
              />
            </div>
          </>
        )}
      </div>
    );
  }
);

DatePicker.displayName = "DatePicker";

