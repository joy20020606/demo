/** @type {import('next').NextConfig} */
const nextConfig = {
  // 讓 Next.js 能 transpile monorepo 內部的 workspace package
  transpilePackages: ['@sme-crm/shared'],
  reactStrictMode: true,

  // ⚠️ Monorepo NodeNext 相容性
  // shared package 用 NodeNext convention，import 路徑寫 '.js'（指編譯後檔名），
  // 但實體檔案是 '.ts'。Next.js webpack 預設不知道這對應，會 module not found。
  // 加 extensionAlias 後，webpack 看到 .js import 會自動試 .ts / .tsx。
  webpack: (config) => {
    config.resolve.extensionAlias = {
      '.js': ['.ts', '.tsx', '.js', '.jsx'],
      '.mjs': ['.mts', '.mjs'],
    };
    return config;
  },
};

export default nextConfig;
