import type { NextRequest } from "next/server";
import { NextResponse } from "next/server";
import { LOCALE_COOKIE_KEY, normalizeLocale, isSupportedLocale, toLocalePath } from "@/core/routing/locales";
import { AUTH_TOKEN_COOKIE } from "@/core/auth/constants";

const PUBLIC_SEGMENTS = new Set(["sign-in"]);
const IGNORED_PATH_PREFIXES = ["/_next", "/assets", "/public", "/favicon", "/api"];

function isIgnoredPath(pathname: string): boolean {
  return IGNORED_PATH_PREFIXES.some((prefix) => pathname.startsWith(prefix));
}

export function middleware(request: NextRequest) {
  const { pathname, search } = request.nextUrl;

  if (isIgnoredPath(pathname)) {
    return NextResponse.next();
  }

  const segments = pathname.split("/").filter(Boolean);

  if (segments.length === 0) {
    return NextResponse.next();
  }

  const localeSegment = segments[0];

  if (!isSupportedLocale(localeSegment)) {
    const locale = normalizeLocale(localeSegment);
    const newPath = toLocalePath(locale, pathname);
    const url = request.nextUrl.clone();
    url.pathname = newPath;
    return NextResponse.redirect(url);
  }

  const locale = normalizeLocale(localeSegment);
  const token = request.cookies.get(AUTH_TOKEN_COOKIE)?.value;
  const section = segments[1] ?? "";
  const isPublicRoute = PUBLIC_SEGMENTS.has(section);

  if (!token && !isPublicRoute) {
    const url = request.nextUrl.clone();
    url.pathname = `/${locale}/sign-in`;
    url.search = search;
    return NextResponse.redirect(url);
  }

  if (token && isPublicRoute) {
    const url = request.nextUrl.clone();
    url.pathname = `/${locale}/dashboard`;
    url.search = "";
    return NextResponse.redirect(url);
  }

  const response = NextResponse.next();
  response.cookies.set({
    name: LOCALE_COOKIE_KEY,
    value: locale,
    path: "/",
    sameSite: "strict"
  });

  return response;
}

export const config = {
  matcher: ["/((?!api|_next|.*\\..*).*)"]
};

