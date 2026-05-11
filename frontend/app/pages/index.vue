<template>
  <div class="min-h-screen flex items-center justify-center">
    <div class="text-center">
      <h1 class="text-2xl font-bold text-text-primary">Celtic FC</h1>
      <p class="text-text-secondary mt-2">Redirecting...</p>
    </div>
  </div>
</template>

<script setup lang="ts">
definePageMeta({
  layout: 'default',
})

const { isAuthenticated } = useAuth()

// Redirect immediately based on auth state
if (isAuthenticated.value) {
  navigateTo('/dashboard', { replace: true })
} else {
  navigateTo('/login', { replace: true })
}
</script>
