// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  compatibilityDate: '2024-11-01',
  devtools: { enabled: process.env.NODE_ENV === 'development' },

  css: ['~/assets/css/main.css'],

  modules: [
    '@pinia/nuxt',
    '@nuxt/ui',
  ],

  tailwindcss: {
    configPath: 'tailwind.config.ts',
  },

  runtimeConfig: {
    public: {
      apiBase: process.env.NUXT_PUBLIC_API_BASE || 'http://localhost:5233',
    },
  },

  // Proxy API requests to backend
  routeRules: {
    '/api/**': { proxy: `${process.env.NUXT_PUBLIC_API_BASE || 'http://localhost:5233'}/api/**` },
  },
})
