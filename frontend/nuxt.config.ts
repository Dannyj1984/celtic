// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  compatibilityDate: '2024-11-01',
  devtools: { enabled: true },

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
      apiBase: 'http://localhost:5233',
    },
  },

  // Proxy API requests to .NET backend in development
  routeRules: {
    '/api/auth/**': { proxy: 'http://localhost:5233/api/auth/**' },
    '/api/players/**': { proxy: 'http://localhost:5233/api/players/**' },
    '/api/matches/**': { proxy: 'http://localhost:5233/api/matches/**' },
    '/api/seasons/**': { proxy: 'http://localhost:5233/api/seasons/**' },
    '/api/events/**': { proxy: 'http://localhost:5233/api/events/**' },
    '/api/parent/**': { proxy: 'http://localhost:5233/api/parent/**' },
    '/api/settings/**': { proxy: 'http://localhost:5233/api/settings/**' },
  },
})
