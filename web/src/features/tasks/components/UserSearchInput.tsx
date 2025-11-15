"use client";

import { useEffect, useRef, useState } from "react";
import { useTranslation } from "react-i18next";

import type { GraphUser } from "@/core/services/graph-api";
import { searchGraphUsers } from "@/core/services/graph-api";
import { Input } from "@/ui/components/Input";
import { Spinner } from "@/ui/components/Spinner";

interface UserSearchInputProps {
  value?: string | undefined; // User GUID
  onChange: (userId: string) => void;
  placeholder?: string | undefined;
  error?: boolean | undefined;
}

export function UserSearchInput({ value, onChange, placeholder, error }: UserSearchInputProps) {
  const { t } = useTranslation(["tasks", "common"]);
  const [searchQuery, setSearchQuery] = useState("");
  const [users, setUsers] = useState<GraphUser[]>([]);
  const [selectedUser, setSelectedUser] = useState<GraphUser | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [isOpen, setIsOpen] = useState(false);
  const [highlightedIndex, setHighlightedIndex] = useState(-1);
  const wrapperRef = useRef<HTMLDivElement>(null);
  const inputRef = useRef<HTMLInputElement>(null);
  const hasLoadedInitialValue = useRef(false);

  // Load initial user if value is provided (e.g., for edit forms)
  useEffect(() => {
    if (value && !hasLoadedInitialValue.current && !selectedUser) {
      hasLoadedInitialValue.current = true;
      // For now, we just store the GUID. In a real implementation, you might want to fetch the user details
      // from the Graph API to display the name and email
    }
  }, [value, selectedUser]);

  // Close dropdown when clicking outside
  useEffect(() => {
    function handleClickOutside(event: MouseEvent) {
      if (wrapperRef.current && !wrapperRef.current.contains(event.target as Node)) {
        setIsOpen(false);
      }
    }
    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, []);

  // Search users as user types
  useEffect(() => {
    if (searchQuery.trim().length < 2) {
      setUsers([]);
      return;
    }

    const timeoutId = setTimeout(async () => {
      setIsLoading(true);
      try {
        const results = await searchGraphUsers(searchQuery);
        setUsers(results);
        setIsOpen(results.length > 0);
      } catch (error) {
        console.error("Error searching users:", error);
        setUsers([]);
      } finally {
        setIsLoading(false);
      }
    }, 300); // Debounce search

    return () => clearTimeout(timeoutId);
  }, [searchQuery]);

  function handleSelect(user: GraphUser) {
    setSelectedUser(user);
    setSearchQuery("");
    setIsOpen(false);
    onChange(user.id);
  }

  function handleClear() {
    setSelectedUser(null);
    setSearchQuery("");
    setUsers([]);
    onChange("");
    inputRef.current?.focus();
  }

  function handleKeyDown(e: React.KeyboardEvent) {
    if (!isOpen) return;

    switch (e.key) {
      case "ArrowDown":
        e.preventDefault();
        setHighlightedIndex((prev) => (prev < users.length - 1 ? prev + 1 : prev));
        break;
      case "ArrowUp":
        e.preventDefault();
        setHighlightedIndex((prev) => (prev > 0 ? prev - 1 : 0));
        break;
      case "Enter":
        e.preventDefault();
        if (highlightedIndex >= 0 && highlightedIndex < users.length) {
          handleSelect(users[highlightedIndex]);
        }
        break;
      case "Escape":
        setIsOpen(false);
        break;
    }
  }

  return (
    <div ref={wrapperRef} className="relative">
      {selectedUser ? (
        <div className="flex items-center gap-2 rounded-md border border-input bg-background px-3 py-2">
          <div className="flex-1">
            <div className="text-sm font-medium text-foreground">{selectedUser.displayName}</div>
            <div className="text-xs text-muted-foreground">
              {selectedUser.mail || selectedUser.userPrincipalName}
            </div>
          </div>
          <button
            type="button"
            onClick={handleClear}
            className="text-muted-foreground hover:text-foreground"
            aria-label={t("common:actions.clear")}
          >
            <svg
              className="h-4 w-4"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M6 18L18 6M6 6l12 12"
              />
            </svg>
          </button>
        </div>
      ) : (
        <>
          <div className="relative">
            <Input
              ref={inputRef}
              type="text"
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              onKeyDown={handleKeyDown}
              onFocus={() => users.length > 0 && setIsOpen(true)}
              placeholder={placeholder || t("tasks:forms.create.fields.searchUserPlaceholder")}
              className={error ? "border-destructive" : ""}
            />
            {isLoading && (
              <div className="pointer-events-none absolute inset-y-0 right-3 flex items-center">
                <Spinner size="sm" />
              </div>
            )}
          </div>

          {isOpen && users.length > 0 && (
            <div className="absolute z-50 mt-1 max-h-60 w-full overflow-auto rounded-md border border-border bg-background shadow-lg">
              {users.map((user, index) => (
                <button
                  key={user.id}
                  type="button"
                  onClick={() => handleSelect(user)}
                  className={`w-full px-3 py-2 text-left text-sm hover:bg-muted ${
                    index === highlightedIndex ? "bg-muted" : ""
                  }`}
                  onMouseEnter={() => setHighlightedIndex(index)}
                >
                  <div className="font-medium text-foreground">{user.displayName}</div>
                  <div className="text-xs text-muted-foreground">
                    {user.mail || user.userPrincipalName}
                    {user.jobTitle && ` • ${user.jobTitle}`}
                  </div>
                </button>
              ))}
            </div>
          )}
        </>
      )}
    </div>
  );
}

