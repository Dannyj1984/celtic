<template>
  <div class="pb-20 md:pb-0">
    <div class="mb-8">
      <h1 class="text-2xl font-bold text-text-primary">Season Fixtures</h1>
      <p class="text-text-secondary mt-1">View all matches and fixtures for the season</p>
    </div>

    <!-- Tabs -->
    <div class="mb-6">
      <div class="flex p-1 bg-surface border border-border rounded-xl w-full max-w-md">
        <button v-for="tab in tabs" :key="tab.id" @click="activeTab = tab.id"
          :class="['flex-1 py-2 px-4 rounded-lg text-sm font-bold transition-all', 
            activeTab === tab.id ? 'bg-celtic-green text-white shadow-lg' : 'text-text-secondary hover:text-text-primary']">
          {{ tab.label }}
        </button>
      </div>
    </div>

    <div v-if="loading" class="flex justify-center py-12">
      <div class="animate-spin w-8 h-8 rounded-full border-4 border-celtic-green border-t-transparent"></div>
    </div>

    <div v-else-if="error" class="bg-danger/10 border border-danger/20 text-danger p-4 rounded-lg">
      {{ error }}
    </div>

    <div v-else class="space-y-4">
      <div v-for="event in filteredMatches" :key="event.id"
        class="card p-4 hover:border-celtic-green/50 transition-all overflow-hidden relative">
        
        <!-- Past Match Indicator -->
        <div v-if="activeTab === 'past'" :class="['absolute top-0 right-0 px-3 py-1 text-[10px] font-black uppercase tracking-tighter rounded-bl-lg',
          event.result === 'Win' ? 'bg-celtic-green text-white' : 
          event.result === 'Loss' ? 'bg-danger text-white' : 'bg-text-muted text-white']">
          {{ event.result }}
        </div>

        <div class="flex items-center justify-between">
          <div class="flex items-center gap-6">
            <div class="text-center min-w-[80px]">
              <div class="text-xs text-text-muted uppercase font-bold">{{ new
                Date(event.dateTime).toLocaleDateString('en-GB', { weekday: 'short' }) }}</div>
              <div class="text-xl font-bold text-text-primary">{{ new Date(event.dateTime).toLocaleDateString('en-GB', {
                day: 'numeric', month: 'short'
              }) }}</div>
            </div>

            <div class="h-10 w-[1px] bg-border"></div>

            <div>
              <div class="flex items-center gap-2 mb-1">
                <UBadge color="primary" variant="subtle" size="xs">Match</UBadge>
                <span class="text-xs text-text-muted font-medium">{{ new
                  Date(event.dateTime).toLocaleTimeString('en-GB', { hour: '2-digit', minute: '2-digit' }) }}</span>
              </div>
              <div class="text-lg font-bold text-text-primary uppercase tracking-tight">
                Celtic FC 
                <span class="text-text-muted mx-2">{{ activeTab === 'past' ? event.score : 'vs' }}</span> 
                {{ event.opposition || 'TBD' }}
              </div>
              <div class="text-sm text-text-secondary flex items-center gap-1">
                <UIcon name="i-heroicons-map-pin-20-solid" class="w-4 h-4" />
                {{ event.location || 'TBC' }}
              </div>
            </div>
          </div>

          <div class="hidden sm:block">
            <UButton v-if="activeTab === 'upcoming'" color="gray" variant="ghost" icon="i-heroicons-calendar-20-solid" label="Add to Calendar" size="xs" />
          </div>
        </div>

        <!-- Match Details (Report & PotM) -->
        <div v-if="activeTab === 'past' && (event.matchReport || event.playerOfTheMatchName)" 
          class="mt-4 pt-4 border-t border-border/50">
          <div v-if="event.playerOfTheMatchName" class="flex items-center gap-2 mb-3">
            <div class="p-1 bg-celtic-gold/10 rounded-md">
              <UIcon name="i-heroicons-star-20-solid" class="w-4 h-4 text-celtic-gold" />
            </div>
            <span class="text-xs font-bold text-text-primary uppercase tracking-tight">
              Player of the Match: <span class="text-celtic-gold ml-1">{{ event.playerOfTheMatchName }}</span>
            </span>
          </div>
          
          <div v-if="event.matchReport" class="bg-surface-hover/50 p-4 rounded-lg border border-border/30">
            <h4 class="text-[10px] font-black uppercase tracking-widest text-text-muted mb-2">Match Report</h4>
            <p class="text-sm text-text-secondary leading-relaxed whitespace-pre-wrap">{{ event.matchReport }}</p>
          </div>
        </div>
      </div>

      <!-- Empty State -->
      <div v-if="filteredMatches.length === 0" class="card p-12 text-center border-dashed">
        <UIcon name="i-heroicons-calendar-days-20-solid" class="w-12 h-12 text-text-muted mx-auto mb-4" />
        <h3 class="text-lg font-medium text-text-primary mb-2">No {{ activeTab }} matches</h3>
        <p class="text-text-muted text-sm">Check back later for newly scheduled fixtures.</p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'

definePageMeta({
  layout: 'app',
})

useHead({
  title: 'Season Fixtures - Celtic FC',
})

const { getAuthHeaders } = useAuth()
const upcomingMatches = ref<any[]>([])
const pastMatches = ref<any[]>([])
const loading = ref(false)
const error = ref<string | null>(null)

const tabs = [
  { id: 'upcoming', label: 'Upcoming' },
  { id: 'past', label: 'Past Fixtures' }
]
const activeTab = ref('upcoming')

const filteredMatches = computed(() => {
  return activeTab.value === 'upcoming' ? upcomingMatches.value : pastMatches.value
})

async function fetchFixtures() {
  loading.value = true
  error.value = null
  try {
    const [upcoming, past] = await Promise.all([
      $fetch<any[]>('/api/parent/upcoming/Match', { headers: getAuthHeaders() }),
      $fetch<any[]>('/api/parent/past/Match', { headers: getAuthHeaders() })
    ])
    upcomingMatches.value = upcoming
    pastMatches.value = past
  } catch (err: any) {
    error.value = 'Failed to load season fixtures.'
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  fetchFixtures()
})
</script>
