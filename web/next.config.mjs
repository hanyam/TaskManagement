/** @type {import('next').NextConfig} */
const nextConfig = {
  reactStrictMode: true,
  swcMinify: true,
  output: 'standalone', // Enable standalone output for Docker
  experimental: {
    typedRoutes: true,
    serverActions: {
      bodySizeLimit: '2mb'
    }
  },
  eslint: {
    dirs: ['src'],
    ignoreDuringBuilds: true // Ignore ESLint warnings/errors during production builds
  }
};

export default nextConfig;
