import { createInstance, type i18n as I18nInstance } from "i18next";
import ICU from "i18next-icu";
import { initReactI18next } from "react-i18next";

import { DEFAULT_LOCALE, type SupportedLocale } from "@/core/routing/locales";
import { namespaces, resources } from "@/i18n/resources";

const instances = new Map<SupportedLocale, I18nInstance>();

export function getI18nInstance(locale: SupportedLocale): I18nInstance {
  const normalized = locale ?? DEFAULT_LOCALE;
  const cached = instances.get(normalized);

  if (cached && cached.isInitialized) {
    return cached;
  }

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
      },
      initImmediate: false
    });

  instances.set(normalized, instance);
  return instance;
}

