<template>
  <div class="min-h-screen bg-surface">
    <VitePwaManifest />
    <!-- Top Navigation -->
    <nav class="glass sticky top-0 z-50 border-b border-border">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div class="flex items-center justify-between h-16">
          <!-- Logo -->
          <NuxtLink to="/dashboard" class="flex items-center gap-3">
            <div
              class="w-8 h-8 rounded-lg bg-gradient-to-br from-celtic-green to-celtic-green-light flex items-center justify-center">
              <svg xmlns="http://www.w3.org/2000/svg" class="w-4 h-4 text-white" viewBox="0 0 24 24" fill="none"
                stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <circle cx="12" cy="12" r="10" />
                <path d="M12 2C9.5 6.5 9.5 11 12 16" />
                <path d="M12 2c2.5 4.5 2.5 9 0 14" />
              </svg>
            </div>
            <span class="font-bold text-text-primary">Stalybridge Celtic U7</span>
          </NuxtLink>

          <div class="hidden md:flex items-center gap-6">
            <template v-if="isAdmin">
              <NuxtLink to="/admin/players"
                class="text-sm font-medium hover:text-celtic-green text-text-secondary transition-colors"
                active-class="text-celtic-green">Squad</NuxtLink>
              <NuxtLink to="/admin/schedule"
                class="text-sm font-medium hover:text-celtic-green text-text-secondary transition-colors"
                active-class="text-celtic-green">Schedule</NuxtLink>
              <NuxtLink to="/admin/matches"
                class="text-sm font-medium hover:text-celtic-green text-text-secondary transition-colors"
                active-class="text-celtic-green">Matches</NuxtLink>
              <NuxtLink to="/admin/seasons"
                class="text-sm font-medium hover:text-celtic-green text-text-secondary transition-colors"
                active-class="text-celtic-green">Seasons</NuxtLink>
              <NuxtLink to="/admin/payments"
                class="text-sm font-medium hover:text-celtic-green text-text-secondary transition-colors"
                active-class="text-celtic-green">Payments</NuxtLink>
              <NuxtLink to="/admin/parents"
                class="text-sm font-medium hover:text-celtic-green text-text-secondary transition-colors"
                active-class="text-celtic-green">Parents</NuxtLink>
              <NuxtLink to="/admin/settings"
                class="text-sm font-medium hover:text-celtic-green text-text-secondary transition-colors"
                active-class="text-celtic-green">Settings</NuxtLink>
            </template>
            <template v-else>
              <NuxtLink to="/dashboard"
                class="text-sm font-medium hover:text-celtic-green text-text-secondary transition-colors"
                active-class="text-celtic-green">Dashboard</NuxtLink>
              <NuxtLink to="/season"
                class="text-sm font-medium hover:text-celtic-green text-text-secondary transition-colors"
                active-class="text-celtic-green">Season</NuxtLink>
              <NuxtLink to="/profile"
                class="text-sm font-medium hover:text-celtic-green text-text-secondary transition-colors"
                active-class="text-celtic-green">Profile</NuxtLink>
              <NuxtLink to="/settings"
                class="text-sm font-medium hover:text-celtic-green text-text-secondary transition-colors"
                active-class="text-celtic-green">Settings</NuxtLink>
            </template>
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

    <!-- Bottom Navigation (Mobile Only) - Parent -->
    <nav v-if="!isAdmin"
      class="md:hidden fixed bottom-0 left-0 right-0 z-50 bg-surface/80 backdrop-blur-xl border-t border-border px-6 py-3 pb-8">
      <div class="flex items-center justify-between">
        <NuxtLink to="/dashboard" class="flex flex-col items-center gap-1 text-text-secondary"
          active-class="text-celtic-green">
          <HomeIcon class="w-6 h-6" />
          <span class="text-[10px] font-medium uppercase tracking-wider">Home</span>
        </NuxtLink>
        <NuxtLink to="/season" class="flex flex-col items-center gap-1 text-text-secondary"
          active-class="text-celtic-green">
          <TrophyIcon class="w-6 h-6" />
          <span class="text-[10px] font-medium uppercase tracking-wider">Season</span>
        </NuxtLink>
        <NuxtLink to="/profile" class="flex flex-col items-center gap-1 text-text-secondary"
          active-class="text-celtic-green">
          <UserIcon class="w-6 h-6" />
          <span class="text-[10px] font-medium uppercase tracking-wider">Profile</span>
        </NuxtLink>
        <NuxtLink to="/settings" class="flex flex-col items-center gap-1 text-text-secondary"
          active-class="text-celtic-green">
          <Cog6ToothIcon class="w-6 h-6" />
          <span class="text-[10px] font-medium uppercase tracking-wider">Settings</span>
        </NuxtLink>
      </div>
    </nav>

    <!-- Bottom Navigation (Mobile Only) - Admin -->
    <nav v-else
      class="md:hidden fixed bottom-0 left-0 right-0 z-50 bg-surface/90 backdrop-blur-xl border-t border-border pb-safe">
      <div class="flex items-center justify-around px-4 py-2">
        <NuxtLink to="/dashboard"
          class="flex flex-col items-center gap-1 px-3 py-2 rounded-xl text-text-secondary transition-colors"
          active-class="text-celtic-green bg-celtic-green/10">
          <HomeIcon class="w-6 h-6" />
          <span class="text-[10px] font-medium uppercase tracking-wider">Home</span>
        </NuxtLink>
        <NuxtLink to="/admin/players"
          class="flex flex-col items-center gap-1 px-3 py-2 rounded-xl text-text-secondary transition-colors"
          active-class="text-celtic-green bg-celtic-green/10">
          <UserGroupIcon class="w-6 h-6" />
          <span class="text-[10px] font-medium uppercase tracking-wider">Squad</span>
        </NuxtLink>
        <NuxtLink to="/admin/payments"
          class="flex flex-col items-center gap-1 px-3 py-2 rounded-xl text-text-secondary transition-colors"
          active-class="text-celtic-green bg-celtic-green/10">
          <BanknotesIcon class="w-6 h-6" />
          <span class="text-[10px] font-medium uppercase tracking-wider">Payments</span>
        </NuxtLink>
        <NuxtLink to="/admin/matches"
          class="flex flex-col items-center gap-1 px-3 py-2 rounded-xl text-text-secondary transition-colors"
          active-class="text-celtic-green bg-celtic-green/10">
          <TrophyIcon class="w-6 h-6" />
          <span class="text-[10px] font-medium uppercase tracking-wider">Matches</span>
        </NuxtLink>
        <NuxtLink to="/admin/settings"
          class="flex flex-col items-center gap-1 px-3 py-2 rounded-xl text-text-secondary transition-colors"
          active-class="text-celtic-green bg-celtic-green/10">
          <Cog6ToothIcon class="w-6 h-6" />
          <span class="text-[10px] font-medium uppercase tracking-wider">Settings</span>
        </NuxtLink>
      </div>
    </nav>


    <main class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8 mb-20 md:mb-0">
      <slot />
    </main>

    <PwaUpdateBanner />
  </div>
</template>

<script setup lang="ts">
import { HomeIcon, TrophyIcon, UserIcon, UserGroupIcon, Cog6ToothIcon, BanknotesIcon } from '@heroicons/vue/24/outline'
const { user, isAdmin, logout } = useAuth()
</script>
