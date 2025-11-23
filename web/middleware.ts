import type { NextRequest } from "next/server";
import { NextResponse } from "next/server";
import { LOCALE_COOKIE_KEY, normalizeLocale, isSupportedLocale, toLocalePath } from "@/core/routing/locales";
import { AUTH_TOKEN_COOKIE, AUTH_USER_COOKIE } from "@/core/auth/constants";

/**
 * Decodes JWT token payload (middleware compatible)
 */
function decodeJwtPayload(token: string): Record<string, unknown> | null {
  try {
    const parts = token.split(".");
    if (parts.length !== 3) {
      return null;
    }

    // Use atob in middleware (runs in Edge Runtime)
    const payload = JSON.parse(atob(parts[1]));
    return payload as Record<string, unknown>;
  } catch {
    return null;
  }
}

/**
 * Checks if a token is expired
 * @param token JWT token string
 * @returns true if token is expired or will expire within 1 minute
 */
function isTokenExpired(token: string): boolean {
  const payload = decodeJwtPayload(token);
  if (payload?.exp) {
    // JWT exp is in seconds, convert to milliseconds
    const expirationTime = (payload.exp as number) * 1000;
    // Consider expired if less than 1 minute remaining (buffer for clock skew)
    return expirationTime - Date.now() < 60_000;
  }
  // If we can't determine expiration, assume it's valid (let backend decide)
  return false;
}

const PUBLIC_SEGMENTS = new Set(["sign-in"]);
const IGNORED_PATH_PREFIXES = ["/_next", "/assets", "/public", "/favicon", "/api", "/auth"];

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
  const tokenValue = request.cookies.get(AUTH_TOKEN_COOKIE)?.value;
  
  // Check if token exists and is not expired
  const token = tokenValue && !isTokenExpired(tokenValue) ? tokenValue : undefined;
  
  const section = segments[1] ?? "";
  const isPublicRoute = PUBLIC_SEGMENTS.has(section);

  if (!token && !isPublicRoute) {
    // If token was expired, clear it
    if (tokenValue) {
      const response = NextResponse.rewrite(
        new URL(`/${locale}/sign-in`, request.url)
      );
      response.cookies.delete(AUTH_TOKEN_COOKIE);
      response.cookies.delete(AUTH_USER_COOKIE);
      response.cookies.set({
        name: AUTH_TOKEN_COOKIE,
        value: "",
        path: "/",
        expires: new Date(0),
        sameSite: "strict",
        secure: true
      });
      response.cookies.set({
        name: AUTH_USER_COOKIE,
        value: "",
        path: "/",
        expires: new Date(0),
        sameSite: "strict",
        secure: true
      });
      return response;
    }
    
    const rewriteUrl = request.nextUrl.clone();
    rewriteUrl.pathname = `/${locale}/sign-in`;
    rewriteUrl.search = search;
    return NextResponse.rewrite(rewriteUrl);
  }

  if (token && isPublicRoute) {
    const redirectUrl = new URL(`/${locale}/dashboard`, request.url);
    return NextResponse.redirect(redirectUrl);
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

