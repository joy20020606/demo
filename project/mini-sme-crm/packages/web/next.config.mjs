/** @type {import('next').NextConfig} */
const nextConfig = {
  // 讓 Next.js 能 transpile monorepo 內部的 workspace package
  transpilePackages: ['@sme-crm/shared'],
  reactStrictMode: true,
};

export default nextConfig;
