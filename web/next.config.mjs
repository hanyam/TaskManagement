/** @type {import('next').NextConfig} */
const nextConfig = {
  reactStrictMode: true,
  swcMinify: true,
  experimental: {
    typedRoutes: true,
    serverActions: {
      bodySizeLimit: '2mb'
    }
  },
  eslint: {
    dirs: ['src']
  },
  i18n: {
    locales: ['en', 'ar'],
    defaultLocale: 'en',
    localeDetection: false
  }
};

export default nextConfig;
