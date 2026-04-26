<template>
  <div class="min-h-screen flex items-center justify-center px-4">
    <!-- Login Card -->
    <div class="w-full max-w-md">
      <!-- Logo / Branding -->
      <div class="text-center mb-8">
        <div class="inline-flex items-center justify-center w-16 h-16 rounded-2xl bg-gradient-to-br from-celtic-green to-celtic-green-light mb-4 shadow-lg shadow-celtic-green/20">
          <svg xmlns="http://www.w3.org/2000/svg" class="w-8 h-8 text-white" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <circle cx="12" cy="12" r="10" />
            <path d="M12 2C9.5 6.5 9.5 11 12 16" />
            <path d="M12 2c2.5 4.5 2.5 9 0 14" />
            <path d="M5 8.5h14" />
            <path d="M3.5 14h17" />
          </svg>
        </div>
        <h1 class="text-2xl font-bold text-text-primary">Celtic FC</h1>
        <p class="text-text-secondary text-sm mt-1">Team Management Portal</p>
      </div>

      <!-- Login Form -->
      <div class="card p-8">
        <form @submit.prevent="handleLogin" class="space-y-5">
          <div>
            <label for="email" class="block text-sm font-medium text-text-secondary mb-1.5">Email</label>
            <input
              id="email"
              v-model="email"
              type="email"
              class="input"
              placeholder="your@email.com"
              required
              autocomplete="email"
            />
          </div>

          <div>
            <label for="password" class="block text-sm font-medium text-text-secondary mb-1.5">Password</label>
            <input
              id="password"
              v-model="password"
              type="password"
              class="input"
              placeholder="••••••••"
              required
              autocomplete="current-password"
            />
          </div>

          <!-- Error Message -->
          <div v-if="error" class="flex items-center gap-2 p-3 rounded-lg bg-danger/10 border border-danger/20">
            <svg xmlns="http://www.w3.org/2000/svg" class="w-4 h-4 text-danger shrink-0" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <circle cx="12" cy="12" r="10" />
              <line x1="15" y1="9" x2="9" y2="15" />
              <line x1="9" y1="9" x2="15" y2="15" />
            </svg>
            <span class="text-danger text-sm">{{ error }}</span>
          </div>

          <button
            type="submit"
            class="btn-primary w-full flex items-center justify-center gap-2"
            :disabled="loading"
          >
            <svg v-if="loading" class="animate-spin w-4 h-4" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
              <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4" />
              <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
            </svg>
            {{ loading ? 'Signing in...' : 'Sign In' }}
          </button>
        </form>
      </div>

      <p class="text-center text-text-muted text-xs mt-6">
        Contact your team manager for login credentials
      </p>
    </div>
  </div>
</template>

<script setup lang="ts">
definePageMeta({
  layout: 'default',
})

useHead({
  title: 'Sign In - Celtic FC',
  meta: [
    { name: 'description', content: 'Sign in to the Celtic FC team management portal' },
  ],
})

const email = ref('')
const password = ref('')

const { login, loading, error } = useAuth()

async function handleLogin() {
  const success = await login(email.value, password.value)
  if (success) {
    navigateTo('/dashboard')
  }
}
</script>
