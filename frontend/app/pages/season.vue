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
        class="card p-4 sm:p-5 hover:border-celtic-green/50 transition-all overflow-hidden relative group">
        
        <!-- Card Header: Date, Time, Type & Match Result / Status -->
        <div class="flex items-center justify-between gap-2 pb-3 border-b border-border/50 flex-wrap">
          <div class="flex items-center gap-2 flex-wrap">
            <!-- Date Pill -->
            <div class="flex items-center gap-1.5 px-2.5 py-1 rounded-lg bg-surface-hover border border-border/80 text-xs font-bold text-text-primary">
              <CalendarDaysIcon class="w-3.5 h-3.5 text-celtic-gold" />
              <span>{{ new Date(event.dateTime).toLocaleDateString('en-GB', { weekday: 'short', day: 'numeric', month: 'short' }) }}</span>
            </div>

            <!-- Time Pill -->
            <span class="text-xs font-semibold px-2 py-0.5 rounded bg-surface border border-border text-text-secondary">
              {{ new Date(event.dateTime).toLocaleTimeString('en-GB', { hour: '2-digit', minute: '2-digit' }) }}
            </span>

            <UBadge color="primary" variant="subtle" size="xs">Match</UBadge>
          </div>

          <!-- Result / Status Badge -->
          <div class="flex items-center gap-2">
            <span v-if="activeTab === 'past' && event.result" :class="['px-2.5 py-1 text-xs font-black uppercase tracking-wider rounded-lg border shadow-sm',
              event.result === 'Win' ? 'bg-celtic-green/10 text-celtic-green border-celtic-green/30' : 
              event.result === 'Loss' ? 'bg-danger/10 text-danger border-danger/30' : 'bg-surface-hover text-text-muted border-border']">
              {{ event.result }} {{ event.score ? `• ${event.score}` : '' }}
            </span>

            <span v-if="event.played || event.status === 'Attending'" class="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-lg text-xs font-semibold bg-celtic-green/10 border border-celtic-green/30 text-celtic-green">
              <CheckCircleIcon class="w-3.5 h-3.5 text-celtic-green" />
              {{ activeTab === 'past' ? 'Played' : 'Playing' }}
            </span>
          </div>
        </div>

        <!-- Fixture Matchup Box -->
        <div class="py-3 sm:py-4">
          <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-3">
            <div class="space-y-1">
              <div class="text-xs font-bold text-text-muted uppercase tracking-wider">
                Stalybridge Celtic U7
              </div>
              <div class="text-lg sm:text-xl font-black text-text-primary flex items-center gap-2 flex-wrap">
                <span class="text-celtic-gold font-bold text-xs sm:text-sm bg-celtic-gold/10 px-2 py-0.5 rounded border border-celtic-gold/20">
                  {{ activeTab === 'past' && event.score ? event.score : 'VS' }}
                </span>
                <span class="uppercase tracking-tight">{{ event.opposition || 'TBD' }}</span>
              </div>
            </div>

            <!-- Location pill -->
            <div class="flex items-center gap-1.5 text-xs text-text-secondary bg-surface/80 px-3 py-1.5 rounded-lg border border-border/50 w-fit">
              <MapPinIcon class="w-4 h-4 text-celtic-gold shrink-0" />
              <span class="truncate max-w-[220px] sm:max-w-xs">{{ event.location || 'TBC' }}</span>
            </div>
          </div>
        </div>

        <!-- Card Footer: Mobile-Friendly Actions -->
        <div v-if="activeTab === 'upcoming'" class="pt-3 border-t border-border/50 flex items-center justify-between sm:justify-end gap-2">
          <button 
            v-if="event.location"
            @click="openInMaps(event.location)"
            class="flex-1 sm:flex-none px-3.5 py-2 bg-surface hover:bg-surface-hover text-text-secondary hover:text-text-primary rounded-xl text-xs font-bold border border-border flex items-center justify-center gap-1.5 transition-colors shadow-sm"
          >
            <MapPinIcon class="w-4 h-4 text-celtic-gold" />
            <span>Open in Maps</span>
          </button>

          <UDropdown :items="calendarMenuItems(event)" :popper="{ placement: 'bottom-end' }" class="flex-1 sm:flex-none">
            <button class="w-full px-3.5 py-2 bg-surface hover:bg-surface-hover text-text-secondary hover:text-text-primary rounded-xl text-xs font-bold border border-border flex items-center justify-center gap-1.5 transition-colors shadow-sm">
              <CalendarDaysIcon class="w-4 h-4 text-celtic-green" />
              <span>Add to Calendar</span>
            </button>
            <template #item="{ item }">
              <component :is="item.icon" class="w-4 h-4 mr-2" v-if="item.icon && typeof item.icon !== 'string'" />
              <UIcon :name="item.icon" class="w-4 h-4 mr-2" v-else-if="item.icon" />
              <span>{{ item.label }}</span>
            </template>
          </UDropdown>
        </div>

        <!-- Match Details (Report & PotM) -->
        <div v-if="activeTab === 'past' && (event.matchReport || event.playerOfTheMatchName)" 
          class="mt-3 pt-3 border-t border-border/50 space-y-3">
          <div v-if="event.playerOfTheMatchName" class="flex items-center gap-2">
            <div class="p-1 bg-celtic-gold/10 rounded-md border border-celtic-gold/20">
              <UIcon name="i-heroicons-star-20-solid" class="w-4 h-4 text-celtic-gold" />
            </div>
            <span class="text-xs font-bold text-text-primary uppercase tracking-tight">
              Player of the Match: <span class="text-celtic-gold ml-1">{{ event.playerOfTheMatchName }}</span>
            </span>
          </div>
          
          <div v-if="event.matchReport" class="bg-surface-hover/50 p-3.5 rounded-xl border border-border/40">
            <h4 class="text-[10px] font-black uppercase tracking-widest text-text-muted mb-1.5">Match Report</h4>
            <p class="text-xs sm:text-sm text-text-secondary leading-relaxed whitespace-pre-wrap">{{ event.matchReport }}</p>
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
import { MapPinIcon, CalendarDaysIcon, GlobeAltIcon, ArrowDownTrayIcon, CheckCircleIcon } from '@heroicons/vue/24/solid'

definePageMeta({
  layout: 'app',
})

useHead({
  title: 'Season Fixtures - Stalybridge Celtic U7',
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

const { downloadIcs, openGoogleCalendar } = useCalendar()

const getCalendarEvent = (event: any) => ({
  title: `Stalybridge Celtic U7 vs ${event.opposition || 'TBD'}`,
  dateTime: event.dateTime,
  location: event.location,
  description: `Match: Stalybridge Celtic U7 vs ${event.opposition || 'TBD'}`
})

const calendarMenuItems = (event: any) => [[
  {
    label: 'Google Calendar',
    icon: GlobeAltIcon,
    click: () => openGoogleCalendar(getCalendarEvent(event))
  },
  {
    label: 'Download .ics',
    icon: ArrowDownTrayIcon,
    click: () => downloadIcs(getCalendarEvent(event))
  }
]]

const openInMaps = (location: string) => {
  window.open(`https://www.google.com/maps/search/?api=1&query=${encodeURIComponent(location)}`, '_blank')
}

onMounted(() => {
  fetchFixtures()
})
</script>
