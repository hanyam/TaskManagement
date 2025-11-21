import { NextResponse } from "next/server";
import type { NextRequest } from "next/server";

import { AUTH_TOKEN_COOKIE } from "@/core/auth/constants";
import { DEFAULT_LOCALE, normalizeLocale } from "@/core/routing/locales";

export function middleware(request: NextRequest) {
  const { pathname } = request.nextUrl;
  const token = request.cookies.get(AUTH_TOKEN_COOKIE)?.value;

  // Extract locale from pathname
  const pathSegments = pathname.split("/").filter(Boolean);
  const locale = pathSegments[0] && (pathSegments[0] === "en" || pathSegments[0] === "ar")
    ? normalizeLocale(pathSegments[0])
    : DEFAULT_LOCALE;

  // If accessing root, redirect based on auth status
  if (pathname === "/") {
    if (token) {
      // Auto-sign in: redirect to dashboard if authenticated
      return NextResponse.redirect(new URL(`/${locale}/dashboard`, request.url));
    } else {
      // Auto-sign in: redirect to sign-in if not authenticated
      return NextResponse.redirect(new URL(`/${locale}/sign-in`, request.url));
    }
  }

  // If accessing locale root (e.g., /en or /ar), redirect based on auth status
  if (pathname === `/${locale}`) {
    if (token) {
      // Auto-sign in: redirect to dashboard if authenticated
      return NextResponse.redirect(new URL(`/${locale}/dashboard`, request.url));
    } else {
      // Auto-sign in: redirect to sign-in if not authenticated
      return NextResponse.redirect(new URL(`/${locale}/sign-in`, request.url));
    }
  }

  // Allow signed-out page (public route)
  if (pathname.startsWith(`/${locale}/signed-out`)) {
    return NextResponse.next();
  }

  // Protect app routes - require authentication
  const isAppRoute = pathname.startsWith(`/${locale}/dashboard`) || 
                     pathname.startsWith(`/${locale}/tasks`) ||
                     pathname.startsWith(`/${locale}/settings`);

  if (isAppRoute && !token) {
    // Redirect to sign-in with redirect parameter
    const signInUrl = new URL(`/${locale}/sign-in`, request.url);
    signInUrl.searchParams.set("redirect", pathname);
    return NextResponse.redirect(signInUrl);
  }

  // If authenticated user tries to access sign-in, redirect to dashboard
  if (pathname.startsWith(`/${locale}/sign-in`) && token) {
    return NextResponse.redirect(new URL(`/${locale}/dashboard`, request.url));
  }

  return NextResponse.next();
}

export const config = {
  matcher: [
    /*
     * Match all request paths except for the ones starting with:
     * - _next/static (static files)
     * - _next/image (image optimization files)
     * - favicon.ico (favicon file)
     * - public folder
     */
    "/((?!_next/static|_next/image|favicon.ico|.*\\.(?:svg|png|jpg|jpeg|gif|webp)$).*)"
  ]
};

