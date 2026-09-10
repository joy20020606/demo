export default defineNuxtConfig({
  modules: ['@nuxt/eslint', '@pinia/nuxt'],
  devtools: { enabled: true },
  app: {
    head: {
      htmlAttrs: { lang: 'zh-Hant-TW' },
      titleTemplate: '%s · Mini Stay',
      meta: [
        { name: 'viewport', content: 'width=device-width, initial-scale=1' },
        { name: 'theme-color', content: '#2d2c2c' },
      ],
      link: [
        { rel: 'preconnect', href: 'https://fonts.googleapis.com' },
        { rel: 'preconnect', href: 'https://fonts.gstatic.com', crossorigin: '' },
        {
          rel: 'stylesheet',
          href: 'https://fonts.googleapis.com/css2?family=Forum&family=Poppins:wght@300;400&display=swap',
        },
      ],
    },
  },
  css: ['~/assets/scss/main.scss'],
  runtimeConfig: {
    databaseUrl: '',
    adminPassword: '',
    sessionSecret: '',
  },
  compatibilityDate: '2026-09-01',
  vite: {
    css: {
      preprocessorOptions: {
        scss: {
          additionalData: '@use "~/assets/scss/tokens" as *; @use "~/assets/scss/mixins" as *;',
        },
      },
    },
  },
  eslint: {
    config: { stylistic: true },
  },
})
