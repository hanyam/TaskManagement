ARG NODE_VERSION=20-alpine

FROM node:${NODE_VERSION} AS base
WORKDIR /app

FROM base AS deps
# Copy package files from web/ directory (context is root)
COPY web/package.json web/package-lock.json ./
# Install dependencies with npm ci for reproducible builds
# Fallback to npm install if package-lock.json is incompatible
RUN npm ci --prefer-offline --no-audit || \
    (echo "Warning: npm ci failed, falling back to npm install" && npm install --prefer-offline --no-audit)

FROM base AS build
# Accept build arguments for NEXT_PUBLIC_* variables
ARG NEXT_PUBLIC_API_BASE_URL=http://localhost:5050
ARG NEXT_PUBLIC_APP_NAME="Task Management Console"
ARG NEXT_PUBLIC_AZURE_AD_CLIENT_ID
ARG NEXT_PUBLIC_AZURE_AD_TENANT_ID
ARG NEXT_PUBLIC_AZURE_AD_REDIRECT_URI
ARG NEXT_PUBLIC_AZURE_AD_SCOPES

# Set them as environment variables for the build
ENV NODE_ENV=production
ENV NEXT_PUBLIC_API_BASE_URL=${NEXT_PUBLIC_API_BASE_URL}
ENV NEXT_PUBLIC_APP_NAME=${NEXT_PUBLIC_APP_NAME}
ENV NEXT_PUBLIC_AZURE_AD_CLIENT_ID=${NEXT_PUBLIC_AZURE_AD_CLIENT_ID}
ENV NEXT_PUBLIC_AZURE_AD_TENANT_ID=${NEXT_PUBLIC_AZURE_AD_TENANT_ID}
ENV NEXT_PUBLIC_AZURE_AD_REDIRECT_URI=${NEXT_PUBLIC_AZURE_AD_REDIRECT_URI}
ENV NEXT_PUBLIC_AZURE_AD_SCOPES=${NEXT_PUBLIC_AZURE_AD_SCOPES}

COPY --from=deps /app/node_modules ./node_modules
# Copy all web files from web/ directory
COPY web/ .
RUN npm run build

FROM base AS runner
ENV NODE_ENV=production
WORKDIR /app
# Copy built files
COPY --from=build /app/public ./public
COPY --from=build /app/.next/standalone ./
COPY --from=build /app/.next/static ./.next/static
EXPOSE 3000
ENV PORT=3000
ENV HOSTNAME="0.0.0.0"
CMD ["node", "server.js"]

