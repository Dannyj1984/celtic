<template>
  <div class="min-h-screen bg-surface">
    <!-- Top Navigation -->
    <nav class="glass sticky top-0 z-50 border-b border-border">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div class="flex items-center justify-between h-16">
          <!-- Logo -->
          <NuxtLink to="/dashboard" class="flex items-center gap-3">
            <div class="w-8 h-8 rounded-lg bg-gradient-to-br from-celtic-green to-celtic-green-light flex items-center justify-center">
              <svg xmlns="http://www.w3.org/2000/svg" class="w-4 h-4 text-white" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <circle cx="12" cy="12" r="10" />
                <path d="M12 2C9.5 6.5 9.5 11 12 16" />
                <path d="M12 2c2.5 4.5 2.5 9 0 14" />
              </svg>
            </div>
            <span class="font-bold text-text-primary">Celtic FC</span>
          </NuxtLink>

          <!-- Desktop Navigation -->
          <div class="hidden md:flex items-center gap-6" v-if="isAdmin">
            <NuxtLink to="/admin/players" class="text-sm font-medium hover:text-celtic-green text-text-secondary transition-colors" active-class="text-celtic-green">Roster</NuxtLink>
            <NuxtLink to="/admin/seasons" class="text-sm font-medium hover:text-celtic-green text-text-secondary transition-colors" active-class="text-celtic-green">Seasons</NuxtLink>
            <NuxtLink to="/admin/parents" class="text-sm font-medium hover:text-celtic-green text-text-secondary transition-colors" active-class="text-celtic-green">Parents</NuxtLink>
          </div>

          <!-- User Info -->
          <div class="flex items-center gap-4">
            <span class="text-sm text-text-secondary hidden sm:block">{{ user?.fullName }}</span>
            <span v-if="isAdmin" class="badge badge-success">Admin</span>
            <button @click="logout" class="text-text-muted hover:text-text-primary transition-colors text-sm">
              Sign Out
            </button>
          </div>
        </div>
      </div>
    </nav>

    <main class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <slot />
    </main>
  </div>
</template>

<script setup lang="ts">
const { user, isAdmin, logout } = useAuth()
</script>
