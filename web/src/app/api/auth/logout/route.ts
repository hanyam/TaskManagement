import { NextResponse } from "next/server";

import { clearServerAuthSession } from "@/core/auth/session.server";

export async function POST() {
  clearServerAuthSession();

  return NextResponse.json({
    success: true
  });
}

