import { z } from "zod";

const envSchema = z.object({
  NEXT_PUBLIC_API_BASE_URL: z
    .string()
    .url()
    .default("http://localhost:5000"),
  NEXT_PUBLIC_APP_NAME: z.string().default("Task Management Console"),
  NEXT_PUBLIC_AZURE_AD_CLIENT_ID: z.string().optional(),
  NEXT_PUBLIC_AZURE_AD_TENANT_ID: z.string().optional(),
  NEXT_PUBLIC_AZURE_AD_REDIRECT_URI: z.string().url().optional(),
  NEXT_PUBLIC_AZURE_AD_SCOPES: z.string().optional()
});

type EnvConfig = {
  apiBaseUrl: string;
  appName: string;
  azureAd?: {
    clientId: string;
    tenantId: string;
    redirectUri?: string;
    scopes: string[];
  };
};

let cachedEnv: EnvConfig | undefined;

export function getEnvConfig(): EnvConfig {
  if (cachedEnv) {
    return cachedEnv;
  }

  const parsed = envSchema.safeParse({
    NEXT_PUBLIC_API_BASE_URL: process.env.NEXT_PUBLIC_API_BASE_URL,
    NEXT_PUBLIC_APP_NAME: process.env.NEXT_PUBLIC_APP_NAME,
    NEXT_PUBLIC_AZURE_AD_CLIENT_ID: process.env.NEXT_PUBLIC_AZURE_AD_CLIENT_ID,
    NEXT_PUBLIC_AZURE_AD_TENANT_ID: process.env.NEXT_PUBLIC_AZURE_AD_TENANT_ID,
    NEXT_PUBLIC_AZURE_AD_REDIRECT_URI: process.env.NEXT_PUBLIC_AZURE_AD_REDIRECT_URI,
    NEXT_PUBLIC_AZURE_AD_SCOPES: process.env.NEXT_PUBLIC_AZURE_AD_SCOPES
  });

  if (!parsed.success) {
    const issues = parsed.error.issues.map((issue) => `${issue.path.join(".")}: ${issue.message}`).join("; ");
    throw new Error(`Invalid environment configuration: ${issues}`);
  }

  const azureAd =
    parsed.data.NEXT_PUBLIC_AZURE_AD_CLIENT_ID && parsed.data.NEXT_PUBLIC_AZURE_AD_TENANT_ID
      ? {
          clientId: parsed.data.NEXT_PUBLIC_AZURE_AD_CLIENT_ID,
          tenantId: parsed.data.NEXT_PUBLIC_AZURE_AD_TENANT_ID,
          scopes: parsed.data.NEXT_PUBLIC_AZURE_AD_SCOPES
            ? parsed.data.NEXT_PUBLIC_AZURE_AD_SCOPES.split(",").map((scope) => scope.trim()).filter(Boolean)
            : [`api://${parsed.data.NEXT_PUBLIC_AZURE_AD_CLIENT_ID}/.default`],
          ...(parsed.data.NEXT_PUBLIC_AZURE_AD_REDIRECT_URI
            ? { redirectUri: parsed.data.NEXT_PUBLIC_AZURE_AD_REDIRECT_URI }
            : {})
        }
      : undefined;

  cachedEnv = {
    apiBaseUrl: parsed.data.NEXT_PUBLIC_API_BASE_URL.replace(/\/$/, ""),
    appName: parsed.data.NEXT_PUBLIC_APP_NAME,
    ...(azureAd ? { azureAd } : {})
  };

  return cachedEnv;
}

