// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  compatibilityDate: '2024-11-01',
  devtools: { enabled: true },

  modules: [
    '@pinia/nuxt',
    '@nuxtjs/tailwindcss',
  ],

  runtimeConfig: {
    public: {
      apiBase: 'http://localhost:5137',
    },
  },

  // Proxy API requests to .NET backend in development
  routeRules: {
    '/api/**': {
      proxy: 'http://localhost:5137/**',
    },
  },
})
