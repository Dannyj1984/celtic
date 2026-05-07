<template>
  <div class="min-h-screen flex items-center justify-center p-4 bg-gradient-to-b from-primary to-surface">
    <!-- Login Card -->
    <div class="w-full max-w-md bg-white rounded-3xl shadow-2xl overflow-hidden relative">

      <!-- Top Ball Icon -->
      <div class="pt-8 pb-4 flex justify-center relative">
        <!-- Floating shadow effect for the ball could go here -->
        <div
          class="w-16 h-16 rounded-full bg-secondary flex items-center justify-center shadow-lg shadow-secondary/40 ring-4 ring-white relative z-10">
          <svg xmlns="http://www.w3.org/2000/svg" class="w-8 h-8 text-white" viewBox="0 0 24 24" fill="none"
            stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <circle cx="12" cy="12" r="10" />
            <path d="M12 7l-3 4h6z" />
            <path d="M15 11l2.5 4.5L12 18l-5.5-2.5L9 11" />
            <path d="M12 7V2" />
            <path d="M9 11L4.5 9.5" />
            <path d="M15 11l4.5-1.5" />
            <path d="M6.5 15.5L3 18" />
            <path d="M17.5 15.5L21 18" />
            <path d="M12 18v4" />
          </svg>
        </div>
        <!-- Gradient splash behind ball -->
        <div class="absolute top-0 left-0 w-full h-32 bg-gradient-to-b from-slate-200/50 to-transparent"></div>
      </div>

      <!-- Header -->
      <div class="text-center px-8 relative z-10">
        <h1 class="text-3xl font-black text-primary tracking-tight mb-2">Junior Football</h1>
        <p class="text-neutral font-medium mb-8">Welcome back!</p>
      </div>

      <!-- Login Form -->
      <div class="px-8 pb-8 relative z-10">
        <form @submit.prevent="handleLogin" class="space-y-6">

          <!-- Email Input -->
          <div>
            <label for="email" class="block text-sm font-bold text-primary mb-2">Parent Email</label>
            <div class="relative">
              <div class="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none">
                <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 text-neutral/70" viewBox="0 0 24 24" fill="none"
                  stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                  <rect x="2" y="4" width="20" height="16" rx="2" />
                  <path d="m22 7-8.97 5.7a1.94 1.94 0 0 1-2.06 0L2 7" />
                </svg>
              </div>
              <input id="email" v-model="email" type="email"
                class="w-full pl-12 pr-4 py-3.5 bg-slate-100 border-2 border-transparent focus:bg-white focus:border-secondary focus:ring-4 focus:ring-secondary/10 rounded-xl text-primary font-medium placeholder:text-neutral/60 transition-all outline-none"
                placeholder="email@example.com" required autocomplete="email" />
            </div>
          </div>

          <!-- Password Input -->
          <div>
            <div class="flex justify-between items-center mb-2">
              <label for="password" class="block text-sm font-bold text-primary">Password</label>
              <a href="#" class="text-sm font-bold text-secondary hover:text-secondary/80 transition-colors">Forgot
                Password?</a>
            </div>
            <div class="relative">
              <div class="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none">
                <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 text-neutral/70" viewBox="0 0 24 24" fill="none"
                  stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                  <rect x="3" y="11" width="18" height="11" rx="2" ry="2" />
                  <path d="M7 11V7a5 5 0 0 1 10 0v4" />
                </svg>
              </div>
              <input id="password" v-model="password" type="password"
                class="w-full pl-12 pr-4 py-3.5 bg-slate-100 border-2 border-transparent focus:bg-white focus:border-secondary focus:ring-4 focus:ring-secondary/10 rounded-xl text-primary font-medium placeholder:text-neutral/70 tracking-widest transition-all outline-none"
                placeholder="••••••••" required autocomplete="current-password" />
            </div>
          </div>

          <!-- Remember / Options -->
          <div class="flex items-center">
            <input id="remember" type="checkbox"
              class="w-5 h-5 rounded border-slate-300 text-secondary focus:ring-secondary cursor-pointer" />
            <label for="remember" class="ml-2 text-sm font-bold text-neutral cursor-pointer select-none">
              Remember this device
            </label>
          </div>

          <!-- Error Message -->
          <div v-if="error" class="flex items-center gap-2 p-3 rounded-lg bg-danger/10 border border-danger/20">
            <svg xmlns="http://www.w3.org/2000/svg" class="w-4 h-4 text-danger shrink-0" viewBox="0 0 24 24" fill="none"
              stroke="currentColor" stroke-width="2">
              <circle cx="12" cy="12" r="10" />
              <line x1="15" y1="9" x2="9" y2="15" />
              <line x1="9" y1="9" x2="15" y2="15" />
            </svg>
            <span class="text-danger text-sm font-medium">{{ error }}</span>
          </div>

          <!-- Submit Button -->
          <button type="submit"
            class="w-full bg-secondary hover:bg-[#E66000] text-white py-4 rounded-xl font-bold text-lg flex items-center justify-center gap-2 transition-all transform active:scale-[0.98] shadow-lg shadow-secondary/30"
            :disabled="loading">
            <svg v-if="loading" class="animate-spin w-5 h-5" xmlns="http://www.w3.org/2000/svg" fill="none"
              viewBox="0 0 24 24">
              <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4" />
              <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
            </svg>
            <template v-else>
              Log In
              <svg xmlns="http://www.w3.org/2000/svg" class="w-5 h-5" viewBox="0 0 24 24" fill="none"
                stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
                <line x1="5" y1="12" x2="19" y2="12" />
                <polyline points="12 5 19 12 12 19" />
              </svg>
            </template>
          </button>
        </form>
      </div>

    </div>
  </div>
</template>

<script setup lang="ts">
definePageMeta({
  layout: 'default',
})

useHead({
  title: 'Sign In - Junior Football',
  meta: [
    { name: 'description', content: 'Sign in to the Junior Football portal' },
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
