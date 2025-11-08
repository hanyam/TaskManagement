import type { Resource } from "i18next";

import authAr from "@/i18n/resources/ar/auth.json";
import commonAr from "@/i18n/resources/ar/common.json";
import dashboardAr from "@/i18n/resources/ar/dashboard.json";
import navigationAr from "@/i18n/resources/ar/navigation.json";
import tasksAr from "@/i18n/resources/ar/tasks.json";
import validationAr from "@/i18n/resources/ar/validation.json";
import authEn from "@/i18n/resources/en/auth.json";
import commonEn from "@/i18n/resources/en/common.json";
import dashboardEn from "@/i18n/resources/en/dashboard.json";
import navigationEn from "@/i18n/resources/en/navigation.json";
import tasksEn from "@/i18n/resources/en/tasks.json";
import validationEn from "@/i18n/resources/en/validation.json";

export const namespaces = [
  "common",
  "navigation",
  "auth",
  "dashboard",
  "tasks",
  "validation"
] as const;

export type AppNamespace = (typeof namespaces)[number];

export const resources: Resource = {
  en: {
    common: commonEn,
    navigation: navigationEn,
    auth: authEn,
    dashboard: dashboardEn,
    tasks: tasksEn,
    validation: validationEn
  },
  ar: {
    common: commonAr,
    navigation: navigationAr,
    auth: authAr,
    dashboard: dashboardAr,
    tasks: tasksAr,
    validation: validationAr
  }
};

