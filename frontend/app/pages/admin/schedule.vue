<template>
  <div>
    <div class="flex flex-col sm:flex-row items-center justify-between gap-4 mb-8">
      <div>
        <h1 class="text-2xl font-bold text-text-primary">Schedule</h1>
        <p class="text-text-secondary mt-1">Calendar of all training and match events</p>
      </div>
      <div class="flex items-center gap-3 w-full sm:w-auto">
        <select v-model="selectedTeamFilter" class="input text-sm py-2 bg-surface max-w-[160px]">
          <option value="All">All Teams</option>
          <option value="AllTeamsOnly">All-Team Events</option>
          <option v-for="team in teams" :key="team.id" :value="team.id">
            {{ team.name }}
          </option>
        </select>
        <button @click="openCreateModal" class="btn-secondary whitespace-nowrap">
          + One-off Event
        </button>
      </div>
    </div>

    <div v-if="loading" class="flex justify-center py-12">
      <div class="animate-spin w-8 h-8 rounded-full border-4 border-celtic-green border-t-transparent"></div>
    </div>
    
    <div v-else-if="error" class="bg-danger/10 border border-danger/20 text-danger p-4 rounded-lg">
      {{ error }}
    </div>

    <div v-else class="space-y-6">
      <!-- Tabs -->
      <div class="flex gap-6 mb-8 border-b border-border/50">
        <button 
          @click="activeTab = 'upcoming'"
          :class="['pb-4 px-1 text-xs font-bold uppercase tracking-widest transition-all border-b-2 relative', 
            activeTab === 'upcoming' ? 'text-celtic-green border-celtic-green' : 'text-text-muted border-transparent hover:text-text-primary']"
        >
          Upcoming Events
          <span v-if="upcomingEventsList.length" class="ml-2 px-1.5 py-0.5 bg-celtic-green/10 text-[10px] rounded-full">{{ upcomingEventsList.length }}</span>
        </button>
        <button 
          @click="activeTab = 'past'"
          :class="['pb-4 px-1 text-xs font-bold uppercase tracking-widest transition-all border-b-2 relative', 
            activeTab === 'past' ? 'text-celtic-green border-celtic-green' : 'text-text-muted border-transparent hover:text-text-primary']"
        >
          Past Events
          <span v-if="pastEventsList.length" class="ml-2 px-1.5 py-0.5 bg-surface-hover text-[10px] rounded-full">{{ pastEventsList.length }}</span>
        </button>
      </div>

      <!-- Group by Month -->
      <div v-for="(group, month) in groupedEvents" :key="month">
        <h2 class="text-sm font-bold text-text-muted uppercase tracking-widest mb-4 flex items-center gap-4">
          {{ month }}
          <div class="h-[1px] flex-1 bg-border/50"></div>
        </h2>
        
        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          <div v-for="event in group" :key="event.id" :class="['card p-4 border-l-4 relative group flex flex-col justify-between', 
            event.isCancelled ? 'opacity-60 border-danger/50' : 
            event.type === 'Match' ? 'border-celtic-gold' : 'border-celtic-green']">
            
            <div v-if="event.isCancelled" class="absolute inset-0 bg-surface/40 flex items-center justify-center rounded-lg z-10 pointer-events-none">
              <span class="bg-danger text-white px-3 py-1 rounded-full text-xs font-bold uppercase tracking-widest -rotate-12">Cancelled</span>
            </div>

            <div>
              <div class="flex justify-between items-start mb-3">
                <div class="flex items-center gap-2 flex-wrap">
                  <span :class="['text-[10px] font-bold uppercase px-2 py-0.5 rounded', 
                    event.type === 'Match' ? 'bg-celtic-gold/20 text-celtic-gold' : 'bg-celtic-green/20 text-celtic-green']">
                    {{ event.type }}
                  </span>
                  <span v-if="event.teamName" class="badge bg-celtic-gold/10 text-celtic-gold border border-celtic-gold/30 text-[10px] px-1.5 py-0.5">
                    {{ event.teamName }}
                  </span>
                  <span v-else class="badge bg-surface-hover text-text-secondary border border-border text-[10px] px-1.5 py-0.5">
                    All Teams
                  </span>
                  <span v-if="event.seasonName" class="text-[10px] text-text-muted font-medium">{{ event.seasonName }}</span>
                </div>
                <div class="flex items-center gap-1 opacity-0 group-hover:opacity-100 transition-opacity">
                  <button @click="openEditModal(event)" title="Edit Event" class="p-1.5 text-text-muted hover:text-celtic-gold">
                    <svg xmlns="http://www.w3.org/2000/svg" class="w-4 h-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"/></svg>
                  </button>
                  <button v-if="event.type !== 'Match'" @click="confirmDelete(event)" title="Delete Event" class="p-1.5 text-text-muted hover:text-danger">
                    <svg xmlns="http://www.w3.org/2000/svg" class="w-4 h-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polyline points="3 6 5 6 21 6"/><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"/></svg>
                  </button>
                </div>
              </div>

              <div class="text-sm font-bold text-text-primary mb-1">
                {{ new Date(event.dateTime).toLocaleTimeString('en-GB', { hour: '2-digit', minute: '2-digit' }) }}
                <span class="text-text-muted mx-1">•</span>
                {{ new Date(event.dateTime).toLocaleDateString('en-GB', { day: 'numeric', month: 'long' }) }}
              </div>

              <div class="text-text-secondary text-sm mb-3">
                {{ event.location }}
              </div>

              <div v-if="event.notes" class="text-xs text-text-muted italic bg-surface-hover p-2 rounded border border-border/50 mb-3">
                {{ event.notes }}
              </div>
            </div>
            
            <!-- Attendance Section -->
            <div class="mt-4 pt-3 border-t border-border">
              <div class="flex items-center justify-between gap-2">
                <div class="flex items-center gap-2">
                  <UIcon name="i-heroicons-users" class="w-4 h-4 text-text-muted" />
                  <span class="text-xs font-bold text-text-primary">
                    {{ event.attendingPlayers?.length || 0 }} {{  activeTab === 'past' ? 'Played': 'Playing'}}
                  </span>
                </div>

                <div class="flex items-center gap-2">
                  <button 
                    @click="openAttendanceModal(event)"
                    class="text-[10px] uppercase font-bold text-celtic-gold hover:underline bg-celtic-gold/10 px-2 py-1 rounded border border-celtic-gold/20"
                  >
                    Manage Squad
                  </button>
                  <button 
                    v-if="event.attendingPlayers?.length > 0" 
                    @click="toggleAttendance(event.id)"
                    class="text-[10px] uppercase font-bold text-celtic-green hover:underline"
                  >
                    {{ expandedEvents[event.id] ? 'Hide' : 'View' }}
                  </button>
                </div>
              </div>
              
              <div v-if="expandedEvents[event.id]" class="mt-3 space-y-1 animate-fade-in max-h-40 overflow-y-auto pr-1">
                <div v-for="player in event.attendingPlayers" :key="player.playerId" 
                  class="text-[11px] text-text-secondary flex items-center gap-2 py-0.5">
                  <div class="w-1.5 h-1.5 rounded-full bg-celtic-green shrink-0"></div>
                  {{ player.fullName }}
                </div>
              </div>
            </div>
            
            <div v-if="event.type === 'Match'" class="mt-3 pt-3 border-t border-border/50 flex justify-between items-center">
               <NuxtLink :to="'/admin/matches'" class="text-[10px] text-celtic-gold hover:underline font-bold uppercase">View Match Details →</NuxtLink>
            </div>
          </div>
        </div>
      </div>

      <!-- Empty State -->
      <div v-if="Object.keys(groupedEvents).length === 0" class="col-span-full card p-12 text-center border-dashed">
        <h3 class="text-lg font-medium text-text-primary mb-2">No {{ activeTab }} events</h3>
        <p v-if="activeTab === 'upcoming'" class="text-text-muted text-sm mb-6">Events are automatically generated for training sessions and matches.</p>
        <p v-else class="text-text-muted text-sm mb-6">Past events will appear here once their scheduled time has passed.</p>
      </div>
    </div>

    <!-- Event Modal (Create / Edit Event) -->
    <div v-if="isModalOpen" class="fixed inset-0 z-[100] flex items-center justify-center bg-black/60 backdrop-blur-sm p-4">
      <div class="card w-full max-w-md p-6 animate-fade-in shadow-2xl border-celtic-green/30">
        <h2 class="text-xl font-bold text-text-primary mb-6">{{ isEditing ? 'Edit Event' : 'Add One-off Event' }}</h2>

        <form @submit.prevent="submitForm" class="space-y-4 text-left">
          <div v-if="!isEditing">
            <label class="block text-sm font-medium text-text-secondary mb-1">Event Type</label>
            <select v-model="form.type" class="input">
              <option value="Training">Training</option>
              <option value="Other">Other</option>
            </select>
          </div>

          <div>
            <label class="block text-sm font-medium text-text-secondary mb-1">Date & Time *</label>
            <input v-model="form.dateTime" type="datetime-local" class="input" required />
          </div>

          <div>
            <label class="block text-sm font-medium text-text-secondary mb-1">Location *</label>
            <input v-model="form.location" type="text" class="input" placeholder="e.g. Training Ground" required />
          </div>

          <div>
            <label class="block text-sm font-medium text-text-secondary mb-1">Notes</label>
            <textarea v-model="form.notes" class="input min-h-[80px]" placeholder="Optional details..."></textarea>
          </div>

          <div v-if="isEditing" class="flex items-center gap-3 py-2">
            <input type="checkbox" v-model="form.isCancelled" id="isCancelled" class="rounded border-border bg-surface text-danger focus:ring-danger" />
            <label for="isCancelled" class="text-sm font-medium text-danger">Cancel this event</label>
          </div>

          <div v-if="formError" class="text-danger text-sm mt-2">
            {{ formError }}
          </div>

          <div class="flex justify-end gap-3 mt-6 pt-4 border-t border-border">
            <button type="button" @click="closeModal" class="btn-secondary">Cancel</button>
            <button type="submit" class="btn-primary" :disabled="saving">
              {{ saving ? 'Saving...' : (isEditing ? 'Update Event' : 'Create Event') }}
            </button>
          </div>
        </form>
      </div>
    </div>

    <!-- Attendance Management Modal -->
    <div v-if="isAttendanceModalOpen" class="fixed inset-0 z-[100] flex items-center justify-center bg-black/60 backdrop-blur-sm p-4">
      <div class="card w-full max-w-lg p-6 animate-fade-in shadow-2xl border-celtic-gold/30">
        <div class="flex items-center justify-between mb-2">
          <div>
            <h2 class="text-xl font-bold text-text-primary">Manage Attending Squad</h2>
            <p class="text-xs text-text-secondary mt-0.5">
              Select attending players for <span v-if="attendanceEvent?.teamName" class="font-bold text-celtic-gold">{{ attendanceEvent.teamName }} </span>{{ attendanceEvent?.type }} on {{ new Date(attendanceEvent?.dateTime || '').toLocaleDateString('en-GB', { day: 'numeric', month: 'short' }) }}
            </p>
          </div>
          <button @click="isAttendanceModalOpen = false" class="text-text-muted hover:text-text-primary p-1">
            ✕
          </button>
        </div>

        <!-- Quick Controls: Search & Multi-Select Helpers -->
        <div class="my-4 space-y-3">
          <input 
            v-model="attendanceSearchQuery" 
            type="text" 
            placeholder="Search active squad players..." 
            class="input text-sm py-2" 
          />
          <div class="flex items-center justify-between text-xs">
            <span class="text-text-secondary font-medium">
              {{ selectedPlayerIds.length }} of {{ targetTeamPlayers.length }} players selected
            </span>
            <div class="flex items-center gap-2">
              <button @click="selectAllActivePlayers" class="text-celtic-green hover:underline font-semibold">
                Select All
              </button>
              <span class="text-text-muted">•</span>
              <button @click="clearAllAttendance" class="text-text-muted hover:text-danger font-semibold">
                Clear All
              </button>
            </div>
          </div>
        </div>

        <!-- Active Squad Players List -->
        <div class="max-h-72 overflow-y-auto border border-border rounded-xl divide-y divide-border bg-surface-hover/30 p-1 mb-6">
          <div 
            v-for="player in filteredActivePlayers" 
            :key="player.id"
            @click="togglePlayerSelection(player.id)"
            class="flex items-center justify-between p-3 rounded-lg hover:bg-surface-hover cursor-pointer transition-colors"
          >
            <div class="flex items-center gap-3">
              <input 
                type="checkbox" 
                :checked="selectedPlayerIds.includes(player.id)"
                @click.stop="togglePlayerSelection(player.id)"
                class="rounded border-border text-celtic-green focus:ring-celtic-green w-4 h-4"
              />
              <div>
                <span class="text-sm font-semibold text-text-primary block">{{ player.firstName }} {{ player.lastName }}</span>
                <span v-if="wasParentAttending(player.id)" class="text-[10px] text-celtic-green font-medium">● Marked by Parent</span>
              </div>
            </div>

            <span 
              v-if="selectedPlayerIds.includes(player.id)" 
              class="badge badge-success text-xs"
            >
              Attending
            </span>
            <span v-else class="text-xs text-text-muted">Not Attending</span>
          </div>

          <div v-if="filteredActivePlayers.length === 0" class="p-6 text-center text-text-muted text-sm">
            No active players found matching "{{ attendanceSearchQuery }}"
          </div>
        </div>

        <div v-if="attendanceError" class="text-danger text-sm mb-4">
          {{ attendanceError }}
        </div>

        <div class="flex justify-end gap-3 pt-4 border-t border-border">
          <button type="button" @click="isAttendanceModalOpen = false" class="btn-secondary">Cancel</button>
          <button @click="saveAttendance" class="btn-primary" :disabled="attendanceSaving">
            {{ attendanceSaving ? 'Saving...' : 'Save Attendance' }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useEvents, type Event } from '~/composables/useEvents'
import { usePlayers } from '~/composables/usePlayers'
import { useTeams } from '~/composables/useTeams'

definePageMeta({
  layout: 'app',
})

useHead({
  title: 'Schedule - Stalybridge Celtic U7',
})

const { events, loading, error, fetchEvents, createEvent, updateEvent, updateEventAttendance, deleteEvent } = useEvents()
const { players, fetchPlayers } = usePlayers()
const { teams, fetchTeams } = useTeams()

const expandedEvents = ref<Record<string, boolean>>({})
const activeTab = ref<'upcoming' | 'past'>('upcoming')
const selectedTeamFilter = ref<string>('All')

onMounted(() => {
  fetchEvents()
  fetchPlayers()
  fetchTeams()
})

const activePlayers = computed(() => {
  return players.value.filter(p => p.isActive)
})

const toggleAttendance = (id: string) => {
  expandedEvents.value[id] = !expandedEvents.value[id]
}

// --- Create / Edit Event Modal ---
const isModalOpen = ref(false)
const isEditing = ref(false)
const editingEvent = ref<Event | null>(null)
const saving = ref(false)
const formError = ref<string | null>(null)

const form = ref({
  type: 'Training',
  dateTime: '',
  location: '',
  notes: '',
  isCancelled: false
})

// --- Attendance Management Modal ---
const isAttendanceModalOpen = ref(false)
const attendanceEvent = ref<Event | null>(null)
const selectedPlayerIds = ref<string[]>([])
const attendanceSearchQuery = ref('')
const attendanceSaving = ref(false)
const attendanceError = ref<string | null>(null)

const targetTeamPlayers = computed(() => {
  let list = activePlayers.value
  if (attendanceEvent.value?.teamId) {
    list = list.filter(p => p.teamId === attendanceEvent.value?.teamId)
  }
  return list
})

const filteredActivePlayers = computed(() => {
  let list = targetTeamPlayers.value
  if (attendanceSearchQuery.value.trim()) {
    const q = attendanceSearchQuery.value.toLowerCase()
    list = list.filter(p => 
      `${p.firstName} ${p.lastName}`.toLowerCase().includes(q)
    )
  }
  return list
})

function openAttendanceModal(event: Event) {
  attendanceEvent.value = event
  attendanceSearchQuery.value = ''
  attendanceError.value = null
  selectedPlayerIds.value = event.attendingPlayers ? event.attendingPlayers.map(p => p.playerId) : []
  isAttendanceModalOpen.value = true
}

function wasParentAttending(playerId: string): boolean {
  return attendanceEvent.value?.attendingPlayers?.some(p => p.playerId === playerId) ?? false
}

function togglePlayerSelection(playerId: string) {
  const index = selectedPlayerIds.value.indexOf(playerId)
  if (index === -1) {
    selectedPlayerIds.value.push(playerId)
  } else {
    selectedPlayerIds.value.splice(index, 1)
  }
}

function selectAllActivePlayers() {
  selectedPlayerIds.value = targetTeamPlayers.value.map(p => p.id)
}

function clearAllAttendance() {
  selectedPlayerIds.value = []
}

async function saveAttendance() {
  if (!attendanceEvent.value) return
  attendanceSaving.value = true
  attendanceError.value = null

  const result = await updateEventAttendance(attendanceEvent.value.id, selectedPlayerIds.value)
  if (result.success) {
    isAttendanceModalOpen.value = false
    expandedEvents.value[attendanceEvent.value.id] = true
  } else {
    attendanceError.value = result.error || 'Failed to save attendance'
  }

  attendanceSaving.value = false
}

// --- Events Filtering & Grouping ---
const filteredByTeam = computed(() => {
  if (selectedTeamFilter.value === 'All') return events.value
  if (selectedTeamFilter.value === 'AllTeamsOnly') return events.value.filter(e => !e.teamId)
  return events.value.filter(e => e.teamId === selectedTeamFilter.value)
})

const upcomingEventsList = computed(() => {
  return filteredByTeam.value.filter(e => new Date(e.dateTime) >= new Date())
})

const pastEventsList = computed(() => {
  return filteredByTeam.value.filter(e => new Date(e.dateTime) < new Date())
})

const filteredEvents = computed(() => {
  return activeTab.value === 'upcoming' ? upcomingEventsList.value : pastEventsList.value
})

const groupedEvents = computed(() => {
  const groups: Record<string, Event[]> = {}
  
  const sorted = [...filteredEvents.value].sort((a, b) => {
    const timeA = new Date(a.dateTime).getTime()
    const timeB = new Date(b.dateTime).getTime()
    return activeTab.value === 'upcoming' ? timeA - timeB : timeB - timeA
  })

  sorted.forEach(event => {
    const date = new Date(event.dateTime)
    const month = date.toLocaleString('default', { month: 'long', year: 'numeric' })
    
    if (!groups[month]) {
      groups[month] = []
    }
    groups[month].push(event)
  })
  
  return groups
})

function openCreateModal() {
  isEditing.value = false
  editingEvent.value = null
  form.value = {
    type: 'Training',
    dateTime: '',
    location: '',
    notes: '',
    isCancelled: false
  }
  formError.value = null
  isModalOpen.value = true
}

function openEditModal(event: Event) {
  isEditing.value = true
  editingEvent.value = event
  form.value = {
    type: event.type,
    dateTime: new Date(event.dateTime).toISOString().slice(0, 16),
    location: event.location,
    notes: event.notes || '',
    isCancelled: event.isCancelled
  }
  formError.value = null
  isModalOpen.value = true
}

function closeModal() {
  isModalOpen.value = false
}

async function submitForm() {
  saving.value = true
  formError.value = null
  
  const payload = {
    ...form.value,
    dateTime: new Date(form.value.dateTime).toISOString()
  }

  let result
  if (isEditing.value && editingEvent.value) {
    result = await updateEvent(editingEvent.value.id, payload)
  } else {
    result = await createEvent(payload)
  }

  if (result.success) {
    closeModal()
  } else {
    formError.value = result.error || 'An error occurred'
  }
  
  saving.value = false
}

async function confirmDelete(event: Event) {
  if (confirm('Are you sure you want to delete this event? This cannot be undone.')) {
    await deleteEvent(event.id)
  }
}
</script>

<style scoped>
.animate-fade-in {
  animation: fadeIn 0.2s ease-out forwards;
}
@keyframes fadeIn {
  from { opacity: 0; transform: scale(0.95); }
  to { opacity: 1; transform: scale(1); }
}
</style>
