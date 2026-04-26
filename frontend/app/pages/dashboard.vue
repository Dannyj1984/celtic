<template>
  <div>
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
      <div class="card p-5 cursor-pointer hover:border-celtic-green transition-colors" @click="navigateTo('/admin/players')">
        <p class="text-text-muted text-xs font-medium uppercase tracking-wide">Players</p>
        <p class="text-2xl font-bold text-text-primary mt-1">Roster &rarr;</p>
      </div>
      <div class="card p-5 cursor-pointer hover:border-celtic-green transition-colors" @click="navigateTo('/admin/seasons')">
         <p class="text-text-muted text-xs font-medium uppercase tracking-wide">Seasons</p>
        <p class="text-2xl font-bold text-text-primary mt-1">Settings &rarr;</p>
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
  </div>
</template>

<script setup lang="ts">
definePageMeta({
  layout: 'app',
})

useHead({
  title: 'Dashboard - Celtic FC',
  meta: [
    { name: 'description', content: 'Celtic FC team management dashboard' },
  ],
})

const { user, isAdmin } = useAuth()
</script>
