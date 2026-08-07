// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  compatibilityDate: '2024-11-01',
  devtools: { enabled: process.env.NODE_ENV === 'development' },

  css: ['~/assets/css/main.css'],

  modules: [
    '@pinia/nuxt',
    '@nuxt/ui',
    '@vite-pwa/nuxt'
  ],

  colorMode: {
    preference: 'system',
    fallback: 'dark',
    classSuffix: ''
  },

  pwa: {
    manifest: {
      name: 'Stalybridge Celtic Juniors',
      short_name: 'SBC Juniors',
      description: 'Stalybridge Celtic Juniors Team Management',
      theme_color: '#006837',
      background_color: '#111827',
      display: 'standalone',
      start_url: '/',
      scope: '/',
      lang: 'en',
      orientation: 'portrait',
      icons: [
        {
          src: 'pwa-192x192.png',
          sizes: '192x192',
          type: 'image/png'
        },
        {
          src: 'pwa-512x512.png',
          sizes: '512x512',
          type: 'image/png'
        }
      ],
    },
    registerType: 'prompt',

    workbox: {
      importScripts: ['/custom-sw.js'],
      navigateFallback: '/'
    },

    injectRegister: 'auto',
    devOptions: {
      enabled: true
    }
  },

  tailwindcss: {
    configPath: 'tailwind.config.ts',
  },

  runtimeConfig: {
    public: {
      apiBase: process.env.NUXT_PUBLIC_API_BASE || 'http://localhost:5233',
      vapidPublicKey: process.env.NUXT_PUBLIC_VAPID_PUBLIC_KEY || 'BCDkFDo1v8ekozayIWpwhOpauvaMTm8AQKq0Yn0XnhIa0SSG4wTqLbcqbAW2FafCKNv_fGy1nTueUZOpysWrjxs'
    },
  },

  // Proxy API requests to backend
  routeRules: {
    '/api/**': { proxy: `${process.env.NUXT_PUBLIC_API_BASE || 'http://localhost:5233'}/api/**` },
  },
})
