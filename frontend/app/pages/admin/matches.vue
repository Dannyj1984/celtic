<template>
  <div>
    <div class="flex flex-col sm:flex-row items-center justify-between gap-4 mb-8">
      <div>
        <h1 class="text-2xl font-bold text-text-primary">Matches</h1>
        <p class="text-text-secondary mt-1">Manage results and upcoming fixtures</p>
      </div>
      <div class="flex items-center gap-3 w-full sm:w-auto">
        <select v-model="selectedTeamFilter" class="input text-sm py-2 bg-surface max-w-[160px]">
          <option value="All">All Teams</option>
          <option value="AllTeamsOnly">All-Team Matches</option>
          <option v-for="team in teams" :key="team.id" :value="team.id">
            {{ team.name }}
          </option>
        </select>
        <button @click="openCreateModal" class="btn-primary whitespace-nowrap">
          + Add Match
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
      <div v-for="match in filteredMatches" :key="match.id" class="card p-4 hover:border-celtic-green/50 transition-all group">
        <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
          <div class="flex items-center gap-4 sm:gap-6">
            <div class="text-center min-w-[70px] sm:min-w-[80px]">
              <div class="text-[10px] sm:text-xs text-text-muted uppercase font-bold">{{ new Date(match.date).toLocaleDateString('en-GB', { weekday: 'short' }) }}</div>
              <div class="text-lg sm:text-xl font-bold text-text-primary">{{ new Date(match.date).toLocaleDateString('en-GB', { day: 'numeric', month: 'short' }) }}</div>
            </div>
            
            <div class="h-10 w-[1px] bg-border"></div>
 
            <div class="flex-1">
              <div class="flex items-center gap-2 mb-1">
                <span v-if="match.teamName" class="badge bg-celtic-gold/10 text-celtic-gold border border-celtic-gold/30 text-[10px] px-1.5 py-0.5">
                  {{ match.teamName }}
                </span>
                <span v-else class="badge bg-surface-hover text-text-secondary border border-border text-[10px] px-1.5 py-0.5">
                  All Teams
                </span>
                <span v-if="!match.seasonId" class="badge bg-orange-500/10 text-orange-500 border-orange-500/20 text-[10px] px-1.5 py-0.5">Friendly</span>
                <span v-else class="text-[10px] sm:text-xs text-text-muted font-medium">{{ match.seasonName }}</span>
                <span class="badge text-[10px] px-1.5 py-0.5" :class="match.format === '3v3' ? 'bg-purple-500/10 text-purple-400 border border-purple-500/30 font-bold' : 'bg-cyan-500/10 text-cyan-400 border border-cyan-500/30 font-bold'">
                  {{ match.format === '3v3' ? '3v3 (no GK)' : '5v5 (with GK)' }}
                </span>
                <span class="badge bg-surface-hover text-text-secondary border border-border text-[10px] px-1.5 py-0.5">2 × {{ match.halfDurationMinutes || 20 }} mins</span>
                <span v-if="match.isPublished" class="text-[9px] sm:text-[10px] text-celtic-green font-bold uppercase tracking-wider">● Published</span>
              </div>
              <div class="text-md sm:text-lg font-bold text-text-primary leading-tight">
                Stalybridge Celtic U7 <span class="text-text-muted mx-1 sm:mx-2">vs</span> {{ match.opposition }}
              </div>
              <div class="text-xs sm:text-sm text-text-secondary flex items-center gap-1 mt-0.5">
                <svg xmlns="http://www.w3.org/2000/svg" class="w-3 h-3" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z"/><circle cx="12" cy="10" r="3"/></svg>
                {{ match.location || 'TBC' }}
              </div>
            </div>
          </div>

          <div class="flex items-center justify-between sm:justify-end gap-4 sm:gap-8 w-full sm:w-auto pt-3 sm:pt-0 border-t border-border/50 sm:border-0">
            <div v-if="new Date(match.date) < new Date()" class="flex flex-col sm:items-end">
              <div class="text-xl sm:text-2xl font-black text-text-primary tracking-widest leading-none">
                {{ match.goalsFor }} - {{ match.goalsAgainst }}
              </div>
              <div :class="['text-[10px] sm:text-xs font-bold uppercase tracking-widest mt-1', 
                match.result === 'Win' ? 'text-celtic-green' : 
                match.result === 'Loss' ? 'text-danger' : 'text-text-muted']">
                {{ match.result }}
              </div>
            </div>
            <div v-else class="hidden sm:block"></div> <!-- Spacer for upcoming matches on desktop -->

            <div class="flex items-center gap-2">
              <button 
                @click="openSquadModal(match)"
                class="px-2.5 py-1.5 bg-emerald-500/10 hover:bg-emerald-500 text-emerald-400 hover:text-white rounded-lg text-xs font-bold border border-emerald-500/30 flex items-center gap-1.5 transition-all shadow-sm"
                title="Squad & Live Match Mode"
              >
                <span>⚽</span>
                <span>Squad</span>
              </button>
              <button @click="openEditModal(match)" class="p-2 text-text-muted hover:text-celtic-gold transition-colors rounded-lg hover:bg-celtic-gold/10">
                <svg xmlns="http://www.w3.org/2000/svg" class="w-5 h-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"/></svg>
              </button>
              <button @click="confirmDelete(match)" class="p-2 text-text-muted hover:text-danger transition-colors rounded-lg hover:bg-danger/10">
                <svg xmlns="http://www.w3.org/2000/svg" class="w-5 h-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polyline points="3 6 5 6 21 6"/><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"/></svg>
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- Empty State -->
      <div v-if="matches.length === 0" class="col-span-full card p-12 text-center border-dashed">
        <h3 class="text-lg font-medium text-text-primary mb-2">No matches scheduled</h3>
        <p class="text-text-muted text-sm mb-6">Start by adding your first friendly or season fixture.</p>
        <button @click="openCreateModal" class="btn-primary inline-flex items-center gap-2">
          Add first match
        </button>
      </div>
    </div>

    <!-- Match Modal -->
    <div v-if="isModalOpen" class="fixed inset-0 z-[100] flex items-center justify-center bg-black/60 backdrop-blur-sm p-4">
      <div class="card w-full max-w-xl p-6 animate-fade-in shadow-2xl border-celtic-green/30 max-h-[90vh] overflow-y-auto">
        <h2 class="text-xl font-bold text-text-primary mb-6">{{ isEditing ? 'Edit Match' : 'Add New Match' }}</h2>

        <form @submit.prevent="submitForm" class="space-y-4 text-left">
          <div class="grid grid-cols-2 gap-4">
            <div class="col-span-2 sm:col-span-1">
              <label class="block text-sm font-medium text-text-secondary mb-1">Type</label>
              <select v-model="form.matchType" class="input">
                <option value="friendly">Friendly Match</option>
                <option value="season">Season Match</option>
              </select>
            </div>
            <div v-if="form.matchType === 'season'" class="col-span-2 sm:col-span-1">
              <label class="block text-sm font-medium text-text-secondary mb-1">Season</label>
              <select v-model="form.seasonId" class="input" required>
                <option value="" disabled>Select season...</option>
                <option v-for="s in seasons" :key="s.id" :value="s.id">{{ s.name }}</option>
              </select>
            </div>
          </div>

          <div class="grid grid-cols-2 gap-4">
            <div class="col-span-2 sm:col-span-1">
              <label class="block text-sm font-medium text-text-secondary mb-1">Format</label>
              <select v-model="form.format" class="input bg-surface font-semibold text-text-primary">
                <option value="5v5">5v5 League (with Goalkeeper)</option>
                <option value="3v3">3v3 League (no Goalkeeper)</option>
              </select>
            </div>
            <div class="col-span-2 sm:col-span-1">
              <label class="block text-sm font-medium text-text-secondary mb-1">Half Duration</label>
              <select v-model.number="form.halfDurationMinutes" class="input bg-surface">
                <option :value="15">2 × 15 mins (30m total)</option>
                <option :value="18">2 × 18 mins (36m total)</option>
                <option :value="20">2 × 20 mins (40m total)</option>
                <option :value="25">2 × 25 mins (50m total)</option>
              </select>
            </div>
          </div>

          <div class="grid grid-cols-2 gap-4">
            <div class="col-span-2 sm:col-span-1">
              <label class="block text-sm font-medium text-text-secondary mb-1">Date & Time *</label>
              <input v-model="form.date" type="datetime-local" class="input" required />
            </div>
            <div class="col-span-2 sm:col-span-1">
              <label class="block text-sm font-medium text-text-secondary mb-1">Opposition *</label>
              <input v-model="form.opposition" type="text" class="input" placeholder="e.g. Rangers FC" required />
            </div>
          </div>

          <div>
            <label class="block text-sm font-medium text-text-secondary mb-1">Location</label>
            <input v-model="form.location" type="text" class="input" placeholder="e.g. Home, Away Ground Name" />
          </div>

          <div>
            <label class="block text-sm font-medium text-text-secondary mb-1">Sub-Team Assignment</label>
            <select v-model="form.teamId" class="input bg-surface">
              <option value="">All Teams (Visible to All Squad)</option>
              <option v-for="team in teams" :key="team.id" :value="team.id">
                {{ team.name }}
              </option>
            </select>
          </div>

          <div v-if="isEditing">
             <div class="bg-surface-hover p-4 rounded-lg border border-border">
                <span class="text-xs text-text-muted uppercase font-bold block mb-3">Match Result</span>
                <div class="grid grid-cols-2 gap-4">
                  <div>
                    <label class="block text-sm font-medium text-text-secondary mb-1">Goals For</label>
                    <input v-model.number="form.goalsFor" type="number" min="0" class="input" />
                  </div>
                  <div>
                    <label class="block text-sm font-medium text-text-secondary mb-1">Goals Against</label>
                    <input v-model.number="form.goalsAgainst" type="number" min="0" class="input" />
                  </div>
                </div>
                <div class="mt-4">
                  <label class="block text-sm font-medium text-text-secondary mb-1">Match Report</label>
                  <textarea v-model="form.matchReport" class="input min-h-[100px]" placeholder="Brief summary of the game..."></textarea>
                </div>
                <div class="mt-4 flex items-center gap-3">
                  <input type="checkbox" v-model="form.isPublished" id="isPublished" class="rounded border-border bg-surface text-celtic-green focus:ring-celtic-green" />
                  <label for="isPublished" class="text-sm font-medium text-text-primary">Publish result to parents</label>
                </div>
                <div class="mt-4">
                  <label class="block text-sm font-medium text-text-secondary mb-1">Player of the Match</label>
                  <select v-model="form.playerOfTheMatchId" class="input">
                    <option :value="null">None selected</option>
                    <option v-for="p in players" :key="p.id" :value="p.id">{{ p.firstName }} {{ p.lastName }}</option>
                  </select>
                </div>
             </div>
          </div>

          <div v-if="formError" class="text-danger text-sm mt-2">
            {{ formError }}
          </div>

          <div class="flex justify-end gap-3 mt-6 pt-4 border-t border-border">
            <button type="button" @click="closeModal" class="btn-secondary">Cancel</button>
            <button type="submit" class="btn-primary" :disabled="saving">
              {{ saving ? 'Saving...' : (isEditing ? 'Update Match' : 'Add Match') }}
            </button>
          </div>
        </form>
      </div>
    </div>

    <!-- Match Squad & Live Match Assistant Modal -->
    <MatchSquadModal
      :is-open="isSquadModalOpen"
      :match-id="selectedSquadMatch?.id"
      :event="selectedSquadMatchEvent"
      @close="isSquadModalOpen = false"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useMatches, type Match } from '~/composables/useMatches'
import { useSeasons } from '~/composables/useSeasons'
import { usePlayers } from '~/composables/usePlayers'
import { useTeams } from '~/composables/useTeams'
import MatchSquadModal from '~/components/MatchSquadModal.vue'

definePageMeta({
  layout: 'app',
})

useHead({
  title: 'Matches - Stalybridge Celtic U7',
})

const { matches, loading, error, fetchMatches, createMatch, updateMatch, deleteMatch } = useMatches()
const { seasons, fetchSeasons } = useSeasons()
const { players, fetchPlayers } = usePlayers()
const { teams, fetchTeams } = useTeams()

const selectedTeamFilter = ref<string>('All')
const isModalOpen = ref(false)
const isEditing = ref(false)
const editingMatch = ref<Match | null>(null)
const saving = ref(false)
const formError = ref<string | null>(null)

const isSquadModalOpen = ref(false)
const selectedSquadMatch = ref<Match | null>(null)

const selectedSquadMatchEvent = computed(() => {
  if (!selectedSquadMatch.value) return undefined
  return {
    id: selectedSquadMatch.value.eventId,
    matchId: selectedSquadMatch.value.id,
    opposition: selectedSquadMatch.value.opposition,
    notes: `Match vs ${selectedSquadMatch.value.opposition}`,
    dateTime: selectedSquadMatch.value.date,
    location: selectedSquadMatch.value.location,
    halfDurationMinutes: selectedSquadMatch.value.halfDurationMinutes || 20,
    format: selectedSquadMatch.value.format || '5v5'
  }
})

function openSquadModal(match: Match) {
  selectedSquadMatch.value = match
  isSquadModalOpen.value = true
}

const filteredMatches = computed(() => {
  if (selectedTeamFilter.value === 'All') return matches.value
  if (selectedTeamFilter.value === 'AllTeamsOnly') return matches.value.filter(m => !m.teamId)
  return matches.value.filter(m => m.teamId === selectedTeamFilter.value)
})

const form = ref({
  matchType: 'friendly',
  seasonId: '',
  date: '',
  opposition: '',
  location: '',
  format: '5v5',
  halfDurationMinutes: 20,
  notes: '',
  goalsFor: 0,
  goalsAgainst: 0,
  matchReport: '',
  isPublished: false,
  playerOfTheMatchId: null as string | null,
  teamId: ''
})

onMounted(() => {
  fetchMatches()
  fetchSeasons()
  fetchPlayers()
  fetchTeams()
})

function openCreateModal() {
  isEditing.value = false
  editingMatch.value = null
  form.value = {
    matchType: 'friendly',
    seasonId: '',
    date: '',
    opposition: '',
    location: '',
    format: '5v5',
    halfDurationMinutes: 20,
    notes: '',
    goalsFor: 0,
    goalsAgainst: 0,
    matchReport: '',
    isPublished: false,
    playerOfTheMatchId: null,
    teamId: ''
  }
  formError.value = null
  isModalOpen.value = true
}

function openEditModal(match: Match) {
  isEditing.value = true
  editingMatch.value = match
  form.value = {
    matchType: match.seasonId ? 'season' : 'friendly',
    seasonId: match.seasonId || '',
    date: new Date(match.date).toISOString().slice(0, 16),
    opposition: match.opposition,
    location: match.location || '',
    format: match.format || '5v5',
    halfDurationMinutes: match.halfDurationMinutes || 20,
    notes: '',
    goalsFor: match.goalsFor,
    goalsAgainst: match.goalsAgainst,
    matchReport: match.matchReport || '',
    isPublished: match.isPublished,
    playerOfTheMatchId: match.playerOfTheMatchId || null,
    teamId: match.teamId || ''
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
    seasonId: form.value.matchType === 'season' ? form.value.seasonId : null,
    date: new Date(form.value.date).toISOString(),
    opposition: form.value.opposition,
    location: form.value.location,
    format: form.value.format || '5v5',
    halfDurationMinutes: form.value.halfDurationMinutes || 20,
    notes: form.value.notes,
    goalsFor: form.value.goalsFor,
    goalsAgainst: form.value.goalsAgainst,
    matchReport: form.value.matchReport,
    isPublished: form.value.isPublished,
    playerOfTheMatchId: form.value.playerOfTheMatchId,
    teamId: form.value.teamId ? form.value.teamId : null
  }

  let result
  if (isEditing.value && editingMatch.value) {
    result = await updateMatch(editingMatch.value.id, payload)
  } else {
    result = await createMatch(payload)
  }

  if (result.success) {
    closeModal()
  } else {
    formError.value = result.error || 'An error occurred'
  }
  
  saving.value = false
}

async function confirmDelete(match: Match) {
  if (confirm(`Are you sure you want to delete the match against ${match.opposition}? This will also delete the associated calendar event.`)) {
    await deleteMatch(match.id)
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
