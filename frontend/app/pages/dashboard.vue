<template>
  <div class="min-h-screen">
    <!-- Top Navigation -->
    <nav class="glass sticky top-0 z-50 border-b border-border">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div class="flex items-center justify-between h-16">
          <!-- Logo -->
          <div class="flex items-center gap-3">
            <div class="w-8 h-8 rounded-lg bg-gradient-to-br from-celtic-green to-celtic-green-light flex items-center justify-center">
              <svg xmlns="http://www.w3.org/2000/svg" class="w-4 h-4 text-white" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <circle cx="12" cy="12" r="10" />
                <path d="M12 2C9.5 6.5 9.5 11 12 16" />
                <path d="M12 2c2.5 4.5 2.5 9 0 14" />
              </svg>
            </div>
            <span class="font-bold text-text-primary">Celtic FC</span>
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

    <!-- Main Content -->
    <main class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <!-- Welcome Section -->
      <div class="mb-8">
        <h1 class="text-2xl font-bold text-text-primary">
          Welcome back, {{ user?.fullName?.split(' ')[0] }} 👋
        </h1>
        <p class="text-text-secondary mt-1">
          {{ isAdmin ? 'Here\'s your team overview' : 'Here\'s what\'s happening with the team' }}
        </p>
      </div>

      <!-- Quick Stats (Admin) -->
      <div v-if="isAdmin" class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 mb-8">
        <div class="card p-5">
          <p class="text-text-muted text-xs font-medium uppercase tracking-wide">Players</p>
          <p class="text-2xl font-bold text-text-primary mt-1">-</p>
        </div>
        <div class="card p-5">
          <p class="text-text-muted text-xs font-medium uppercase tracking-wide">Next Event</p>
          <p class="text-2xl font-bold text-text-primary mt-1">-</p>
        </div>
        <div class="card p-5">
          <p class="text-text-muted text-xs font-medium uppercase tracking-wide">Season Record</p>
          <p class="text-2xl font-bold text-text-primary mt-1">-</p>
        </div>
        <div class="card p-5">
          <p class="text-text-muted text-xs font-medium uppercase tracking-wide">Team Balance</p>
          <p class="text-2xl font-bold text-celtic-gold mt-1">-</p>
        </div>
      </div>

      <!-- Placeholder sections -->
      <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <div class="card p-6">
          <h2 class="text-lg font-semibold text-text-primary mb-4">Upcoming Events</h2>
          <p class="text-text-muted text-sm">No upcoming events yet. Events will appear here once created.</p>
        </div>

        <div class="card p-6">
          <h2 class="text-lg font-semibold text-text-primary mb-4">Recent Results</h2>
          <p class="text-text-muted text-sm">No match results yet. Results will show here after matches are played.</p>
        </div>
      </div>

      <!-- Announcements -->
      <div class="card p-6 mt-6">
        <h2 class="text-lg font-semibold text-text-primary mb-4">Announcements</h2>
        <p class="text-text-muted text-sm">No announcements. Check back later for updates from the team manager.</p>
      </div>
    </main>
  </div>
</template>

<script setup lang="ts">
definePageMeta({
  layout: 'default',
})

useHead({
  title: 'Dashboard - Celtic FC',
  meta: [
    { name: 'description', content: 'Celtic FC team management dashboard' },
  ],
})

const { user, isAdmin, logout } = useAuth()
</script>
