<template>
  <div>
    <div class="flex flex-col sm:flex-row items-center justify-between gap-4 mb-8">
      <div>
        <h1 class="text-2xl font-bold text-text-primary">Squad Management</h1>
        <p class="text-text-secondary mt-1">Manage team roster, sub status, and team assignments</p>
      </div>
      <div class="flex items-center gap-3 w-full sm:w-auto">
        <select v-model="selectedTeamFilter" class="input text-sm py-2 bg-surface max-w-[160px]">
          <option value="All">All Teams</option>
          <option value="Unassigned">Unassigned</option>
          <option v-for="team in teams" :key="team.id" :value="team.id">
            {{ team.name }}
          </option>
        </select>
        <button @click="openCreateModal" class="btn-primary whitespace-nowrap">
          + Add Player
        </button>
      </div>
    </div>

    <div v-if="loading" class="flex justify-center py-12">
      <div class="animate-spin w-8 h-8 rounded-full border-4 border-celtic-green border-t-transparent"></div>
    </div>

    <div v-else-if="error" class="bg-danger/10 border border-danger/20 text-danger p-4 rounded-lg">
      {{ error }}
    </div>

    <!-- Squad Grid -->
    <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      <div v-for="player in filteredPlayers" :key="player.id" class="card p-5 relative group">

        <!-- Top badges row -->
        <div class="absolute top-4 right-4 flex items-center gap-2">
          <div v-if="player.teams && player.teams.length > 0" class="flex flex-wrap gap-1 items-center justify-end">
            <span v-for="team in player.teams" :key="team.id"
              class="badge text-xs font-semibold"
              :style="{
                backgroundColor: (team.colorHex || '#F59E0B') + '1A',
                color: team.colorHex || '#F59E0B',
                borderColor: (team.colorHex || '#F59E0B') + '50'
              }">
              {{ team.name }}
            </span>
          </div>
          <span v-else-if="player.teamName" class="badge bg-celtic-gold/10 text-celtic-gold border border-celtic-gold/30 text-xs font-semibold">
            {{ player.teamName }}
          </span>
          <!-- Subscription Status Badge (clickable to cycle) -->
          <button @click="cycleSubStatus(player)" :disabled="updatingSubStatus === player.id"
            :class="['badge text-xs font-semibold transition-all hover:opacity-80 cursor-pointer', subStatusClass(player.subscriptionStatus)]"
            :title="'Click to change subscription status'">
            <span v-if="updatingSubStatus === player.id">...</span>
            <span v-else>{{ player.subscriptionStatus }}</span>
          </button>
        </div>

        <h3 class="text-lg font-bold text-text-primary mb-1 pr-32">{{ player.firstName }} {{ player.lastName }}</h3>
        <div class="flex flex-wrap items-center gap-2 mt-1">
          <span v-if="player.fanNumber" class="text-xs font-semibold px-2 py-0.5 rounded bg-surface-hover border border-border/60 text-text-secondary">
            FAN: {{ player.fanNumber }}
          </span>
          <span v-if="player.shirtSize" class="text-xs font-semibold px-2 py-0.5 rounded bg-celtic-green/10 border border-celtic-green/30 text-celtic-green">
            Shirt: {{ player.shirtSize }}
          </span>
          <span v-if="player.shortSize" class="text-xs font-semibold px-2 py-0.5 rounded bg-celtic-green/10 border border-celtic-green/30 text-celtic-green">
            Short: {{ player.shortSize }}
          </span>
          <span v-if="player.sockSize !== null && player.sockSize !== undefined" class="text-xs font-semibold px-2 py-0.5 rounded bg-celtic-green/10 border border-celtic-green/30 text-celtic-green">
            Sock: {{ player.sockSize }}
          </span>
          <span :class="['text-xs font-semibold px-2 py-0.5 rounded inline-flex items-center gap-1 border', player.allowPhotos ? 'bg-success/10 border-success/30 text-success' : 'bg-danger/10 border-danger/30 text-danger']"
            :title="player.allowPhotos ? 'Photo & Social Media consent granted by parent' : 'No photo consent granted'">
            <component :is="player.allowPhotos ? CheckCircleIcon : XCircleIcon" class="w-3.5 h-3.5" />
            <span>Photos</span>
          </span>
          <button @click="toggleSigningFee(player)" :disabled="updatingSigningFee === player.id"
            :class="['text-xs font-semibold px-2 py-0.5 rounded inline-flex items-center gap-1 border transition-all cursor-pointer hover:opacity-80',
              player.signingFeePaid ? 'bg-emerald-500/10 border-emerald-500/30 text-emerald-400' : 'bg-amber-500/10 border-amber-500/30 text-amber-400']"
            :title="'Click to toggle signing fee status'">
            <span v-if="updatingSigningFee === player.id" class="w-3 h-3 border border-current border-t-transparent rounded-full animate-spin"></span>
            <component v-else :is="player.signingFeePaid ? CheckCircleIcon : XCircleIcon" class="w-3.5 h-3.5" />
            <span>Fee: {{ player.signingFeePaid ? 'Paid' : 'Unpaid' }}</span>
          </button>
        </div>

        <div class="mt-4 space-y-3">
          <div v-if="player.dateOfBirth">
            <span class="text-xs text-text-muted uppercase tracking-wide">DOB</span>
            <p class="text-sm text-text-secondary">{{ new Date(player.dateOfBirth).toLocaleDateString() }}</p>
          </div>
          <div v-if="player.emergencyContact || (player.parents && player.parents.length > 0)">
            <span class="text-xs text-text-muted uppercase tracking-wide">Emergency Contact</span>
            <div v-if="player.emergencyContact">
              <p class="text-sm text-text-secondary">{{ player.emergencyContact }} <span
                  v-if="player.emergencyPhone">({{
                    player.emergencyPhone }})</span></p>
            </div>
            <div v-for="parent in player.parents" :key="parent.userId" class="mt-1">
              <p class="text-sm text-text-secondary">
                {{ parent.fullName }} ({{ parent.relationship }})
                <span v-if="parent.phone">({{ parent.phone }})</span>
              </p>
            </div>
            <div v-if="player.emergencyContact2" class="mt-1 border-t border-border/50 pt-1">
              <p class="text-sm text-text-secondary">{{ player.emergencyContact2 }} <span
                  v-if="player.emergencyPhone2">({{
                    player.emergencyPhone2 }})</span></p>
            </div>
          </div>
          <div v-if="player.attendance">
            <span class="text-xs text-text-muted uppercase tracking-wide">Attendance (Last 10)</span>
            <div class="grid grid-cols-2 gap-2 mt-1">
              <div class="bg-surface-hover p-2 rounded border border-border/50">
                <span class="text-[10px] text-text-muted uppercase block mb-1">Matches</span>
                <p class="text-sm font-bold text-celtic-gold" data-testid="attendance-match">
                  {{ player.attendance.matchAttended }} / {{ player.attendance.matchTotal }}
                </p>
              </div>
              <div class="bg-surface-hover p-2 rounded border border-border/50">
                <span class="text-[10px] text-text-muted uppercase block mb-1">Training</span>
                <p class="text-sm font-bold text-celtic-green" data-testid="attendance-training">
                  {{ player.attendance.trainingAttended }} / {{ player.attendance.trainingTotal }}
                </p>
              </div>
            </div>
          </div>

          <!-- Training Cards Counter -->
          <div class="bg-surface-hover/50 p-2.5 rounded-lg border border-celtic-gold/20 flex items-center justify-between">
            <div class="flex items-center gap-2">
              <span class="text-base">🎴</span>
              <div>
                <span class="text-[10px] text-celtic-gold uppercase font-bold tracking-wider block">Training Charms</span>
                <span class="text-sm font-bold text-text-primary">{{ player.trainingCardsCount || 0 }} Charms</span>
              </div>
            </div>
            <div class="flex items-center gap-1">
              <button
                @click="changeCards(player, -1)"
                :disabled="updatingCardsPlayerId === player.id"
                class="w-7 h-7 rounded bg-surface hover:bg-surface-hover border border-border text-text-secondary font-bold text-xs flex items-center justify-center transition-colors disabled:opacity-50 cursor-pointer"
                title="Remove 1 card"
              >
                -
              </button>
              <button
                @click="changeCards(player, 1)"
                :disabled="updatingCardsPlayerId === player.id"
                class="px-2.5 py-1 rounded bg-celtic-gold/10 hover:bg-celtic-gold/20 text-celtic-gold border border-celtic-gold/30 font-bold text-xs flex items-center gap-1 transition-colors disabled:opacity-50 cursor-pointer"
                title="Award 1 card"
              >
                <span v-if="updatingCardsPlayerId === player.id" class="w-3 h-3 border border-celtic-gold border-t-transparent rounded-full animate-spin"></span>
                <span v-else>+1</span>
                <span>🎴</span>
              </button>
            </div>
          </div>
          <div v-if="player.allergies">
            <span class="text-xs text-text-muted uppercase tracking-wide">Allergies</span>
            <p class="text-sm text-danger mt-1 bg-danger/10 p-2 rounded font-medium">{{ player.allergies }}</p>
          </div>
          <div v-if="player.medicalNotes">
            <span class="text-xs text-text-muted uppercase tracking-wide">Medical Notes</span>
            <p class="text-sm text-warning mt-1 bg-warning/10 p-2 rounded">{{ player.medicalNotes }}</p>
          </div>
          <div v-if="player.coachNotes">
            <span class="text-xs text-text-muted uppercase tracking-wide">Coach Notes</span>
            <p class="text-sm text-celtic-green mt-1 bg-celtic-green/10 p-2 rounded">{{ player.coachNotes }}</p>
          </div>
        </div>

        <div class="pt-4 mt-4 border-t border-border flex justify-end">
          <button @click="openEditModal(player)"
            class="text-sm text-celtic-gold hover:text-celtic-gold-light font-medium transition-colors">
            Edit Details
          </button>
        </div>
      </div>

      <!-- Empty State -->
      <div v-if="players.length === 0" class="col-span-full card p-12 text-center border-dashed">
        <h3 class="text-lg font-medium text-text-primary mb-2">No players added yet</h3>
        <p class="text-text-muted text-sm mb-6">Add players to track match stats, subs, and RSVP to events.</p>
        <button @click="openCreateModal" class="btn-primary inline-flex items-center gap-2">
          Add the first player
        </button>
      </div>
    </div>

    <!-- Player Modal (Create / Edit) -->
    <div v-if="isModalOpen"
      class="fixed inset-0 z-[100] flex justify-center items-start overflow-y-auto bg-black/60 backdrop-blur-sm p-4 py-10 sm:py-20">
      <div class="card w-full max-w-lg p-6 animate-fade-in shadow-2xl border-celtic-green/30">
        <h2 class="text-xl font-bold text-text-primary mb-6">
          {{ editingPlayer ? 'Edit Player' : 'Add New Player' }}
        </h2>

        <form @submit.prevent="submitForm" class="space-y-4 text-left">
          <div class="grid grid-cols-2 gap-4">
            <div>
              <label class="block text-sm font-medium text-text-secondary mb-1">First Name *</label>
              <input v-model="form.firstName" type="text" class="input" required />
            </div>
            <div>
              <label class="block text-sm font-medium text-text-secondary mb-1">Last Name *</label>
              <input v-model="form.lastName" type="text" class="input" required />
            </div>
          </div>

          <div class="grid grid-cols-2 gap-4">
            <div>
              <label class="block text-sm font-medium text-text-secondary mb-1">Date of Birth</label>
              <input v-model="form.dateOfBirth" type="date" class="input" />
            </div>
            <div>
              <label class="block text-sm font-medium text-text-secondary mb-1">FAN Number</label>
              <input v-model="form.fanNumber" type="text" class="input" placeholder="e.g. 12345678" />
            </div>
          </div>

          <div class="grid grid-cols-3 gap-3">
            <div>
              <label class="block text-xs font-medium text-text-secondary mb-1">Shirt Size (Age)</label>
              <select v-model="form.shirtSize" class="input text-xs py-1.5" data-testid="admin-shirt-size-input">
                <option value="">Select</option>
                <option v-for="age in ageOptions" :key="age" :value="age">{{ age }}</option>
              </select>
            </div>
            <div>
              <label class="block text-xs font-medium text-text-secondary mb-1">Short Size (Age)</label>
              <select v-model="form.shortSize" class="input text-xs py-1.5" data-testid="admin-short-size-input">
                <option value="">Select</option>
                <option v-for="age in ageOptions" :key="age" :value="age">{{ age }}</option>
              </select>
            </div>
            <div>
              <label class="block text-xs font-medium text-text-secondary mb-1">Sock Size (Num)</label>
              <input v-model.number="form.sockSize" type="number" min="1" max="15" class="input text-xs py-1.5" placeholder="e.g. 12" data-testid="admin-sock-size-input" />
            </div>
          </div>

          <div>
            <label class="block text-sm font-medium text-text-secondary mb-1">Preferred Foot</label>
            <select v-model="form.preferredFoot" class="input">
              <option value="Right">Right</option>
              <option value="Left">Left</option>
              <option value="Both">Both</option>
            </select>
          </div>

          <div class="grid grid-cols-2 gap-4">
            <div>
              <label class="block text-sm font-medium text-text-secondary mb-1">Emergency Contact</label>
              <input v-model="form.emergencyContact" type="text" class="input" placeholder="Name (e.g. Mum)" />
            </div>
            <div>
              <label class="block text-sm font-medium text-text-secondary mb-1">Emergency Phone</label>
              <input v-model="form.emergencyPhone" type="tel" class="input" placeholder="07..." />
            </div>
          </div>

          <div class="grid grid-cols-2 gap-4">
            <div>
              <label class="block text-sm font-medium text-text-secondary mb-1">Emergency Contact 2</label>
              <input v-model="form.emergencyContact2" type="text" class="input" placeholder="Name (e.g. Dad)" />
            </div>
            <div>
              <label class="block text-sm font-medium text-text-secondary mb-1">Emergency Phone 2</label>
              <input v-model="form.emergencyPhone2" type="tel" class="input" placeholder="07..." />
            </div>
          </div>

          <div>
            <label class="block text-sm font-medium text-text-secondary mb-1">Sub-Team Assignments</label>
            <p class="text-xs text-text-muted mb-2">Select all teams this player plays for (e.g. Stripes, Hoops)</p>
            <div class="flex flex-wrap gap-2">
              <button
                v-for="team in teams"
                :key="team.id"
                type="button"
                @click="toggleTeam(team.id)"
                :class="[
                  'px-3 py-1.5 rounded-lg text-xs font-semibold border transition-all flex items-center gap-1.5 cursor-pointer',
                  form.teamIds.includes(team.id)
                    ? 'ring-2 ring-celtic-green shadow-sm'
                    : 'bg-surface hover:bg-surface-hover text-text-muted border-border'
                ]"
                :style="form.teamIds.includes(team.id) ? {
                  backgroundColor: (team.colorHex || '#006837') + '25',
                  color: team.colorHex || '#006837',
                  borderColor: team.colorHex || '#006837'
                } : {}"
              >
                <span class="w-2.5 h-2.5 rounded-full" :style="{ backgroundColor: team.colorHex || '#006837' }"></span>
                {{ team.name }}
                <span v-if="form.teamIds.includes(team.id)" class="text-xs font-bold ml-1">✓</span>
              </button>
              <span v-if="teams.length === 0" class="text-xs text-text-muted italic">No teams configured</span>
            </div>
          </div>

          <div v-if="editingPlayer">
            <label class="block text-sm font-medium text-text-secondary mb-1">Subscription Status</label>
            <select v-model="form.subscriptionStatus" class="input">
              <option value="Active">Active</option>
              <option value="Payment Due">Payment Due</option>
              <option value="Inactive">Inactive</option>
            </select>
          </div>

          <div>
            <label class="block text-sm font-medium text-text-secondary mb-1">Allergies</label>
            <input v-model="form.allergies" type="text" class="input" placeholder="e.g. Nuts, Dairy, Penicillin" />
          </div>

          <div>
            <label class="block text-sm font-medium text-text-secondary mb-1">Medical Notes</label>
            <textarea v-model="form.medicalNotes" class="input min-h-[80px]"
              placeholder="Medical conditions, inhalers..."></textarea>
          </div>

          <div>
            <label class="block text-sm font-medium text-text-secondary mb-1">Coach Notes (Parents only)</label>
            <textarea v-model="form.coachNotes" class="input min-h-[80px]"
              placeholder="Feedback for parents..."></textarea>
          </div>

          <div>
            <label class="block text-sm font-medium text-text-secondary mb-1">Training Charms Collected 🎴</label>
            <input v-model.number="form.trainingCardsCount" type="number" min="0" class="input" placeholder="0" />
          </div>

          <div v-if="editingPlayer" class="p-3 rounded-lg bg-surface-hover border border-border/50 flex items-center justify-between">
            <div>
              <span class="text-xs font-bold text-text-muted uppercase tracking-wider block">Photo & Social Media Consent</span>
              <span class="text-xs text-text-secondary">Managed by parent on parent dashboard</span>
            </div>
            <span :class="['text-xs font-bold px-2.5 py-1 rounded-md inline-flex items-center gap-1.5 border', form.allowPhotos ? 'bg-success/10 border-success/30 text-success' : 'bg-danger/10 border-danger/30 text-danger']">
              <component :is="form.allowPhotos ? CheckCircleIcon : XCircleIcon" class="w-4 h-4" />
              {{ form.allowPhotos ? 'Allowed' : 'Not Allowed' }}
            </span>
          </div>

          <div class="flex items-center gap-2 p-3 rounded-lg bg-surface-hover border border-border/50">
            <input type="checkbox" id="signingFeePaid" v-model="form.signingFeePaid"
              class="rounded border-border text-celtic-green focus:ring-celtic-green w-4 h-4 cursor-pointer" />
            <label for="signingFeePaid" class="text-sm font-medium text-text-primary cursor-pointer select-none">
              Signing on fee paid for the season
            </label>
          </div>

          <div v-if="editingPlayer" class="flex items-center gap-2 mt-2">
            <input type="checkbox" id="isActive" v-model="form.isActive"
              class="rounded border-border text-celtic-green focus:ring-celtic-green w-4 h-4" />
            <label for="isActive" class="text-sm font-medium text-text-secondary">Player is active and playing</label>
          </div>

          <div v-if="formError" class="text-danger text-sm">
            {{ formError }}
          </div>

          <div class="flex justify-end gap-3 mt-6 pt-4 border-t border-border">
            <button type="button" @click="closeModal" class="btn-secondary">Cancel</button>
            <button type="submit" class="btn-primary" :disabled="formSaving">
              {{ formSaving ? 'Saving...' : 'Save Player' }}
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { CheckCircleIcon, XCircleIcon } from '@heroicons/vue/24/solid'
import { usePlayers, type Player } from '~/composables/usePlayers'
import { useTeams } from '~/composables/useTeams'
import { useAuth } from '~/composables/useAuth'

definePageMeta({
  layout: 'app',
})

useHead({
  title: 'Squad - Stalybridge Celtic U7',
})

const { players, loading, error, fetchPlayers, createPlayer, updatePlayer, updatePlayerCards, updateSigningFee } = usePlayers()
const { teams, fetchTeams } = useTeams()
const { getAuthHeaders } = useAuth()

const selectedTeamFilter = ref<string>('All')
const isModalOpen = ref(false)
const editingPlayer = ref<Player | null>(null)
const formSaving = ref(false)
const formError = ref<string | null>(null)
const updatingSubStatus = ref<string | null>(null)
const updatingSigningFee = ref<string | null>(null)

const filteredPlayers = computed(() => {
  if (selectedTeamFilter.value === 'All') return players.value
  if (selectedTeamFilter.value === 'Unassigned') {
    return players.value.filter(p => (!p.teamIds || p.teamIds.length === 0) && (!p.teams || p.teams.length === 0) && !p.teamId)
  }
  return players.value.filter(p =>
    p.teamIds?.includes(selectedTeamFilter.value) ||
    p.teams?.some(t => t.id === selectedTeamFilter.value) ||
    p.teamId === selectedTeamFilter.value
  )
})

const SUB_STATUSES = ['Active', 'Payment Due', 'Inactive']

const updatingCardsPlayerId = ref<string | null>(null)

function showToast(title: string, description: string, color: string) {
  try {
    const t = useToast()
    t?.add?.({ title, description, color })
  } catch {}
}

function subStatusClass(status: string) {
  if (status === 'Active') return 'bg-success/20 text-success border border-success/30'
  if (status === 'Payment Due') return 'bg-warning/20 text-warning border border-warning/30'
  return 'bg-danger/20 text-danger border border-danger/30'
}

async function toggleSigningFee(player: Player) {
  if (updatingSigningFee.value === player.id) return
  const newStatus = !player.signingFeePaid
  updatingSigningFee.value = player.id
  player.signingFeePaid = newStatus

  const result = await updateSigningFee(player.id, newStatus)
  if (result.success && result.player) {
    player.signingFeePaid = result.player.signingFeePaid
    showToast(
      newStatus ? 'Signing Fee Paid ✓' : 'Signing Fee Unpaid',
      `Updated signing fee status for ${player.firstName} ${player.lastName}.`,
      newStatus ? 'green' : 'amber'
    )
  } else {
    player.signingFeePaid = !newStatus
    showToast(
      'Failed to update signing fee',
      result.error || 'Could not update signing fee. Please try again.',
      'red'
    )
  }
  updatingSigningFee.value = null
}

async function changeCards(player: Player, delta: number) {
  if (updatingCardsPlayerId.value === player.id) return
  const current = player.trainingCardsCount || 0
  const newCount = Math.max(0, current + delta)
  if (newCount === current && delta < 0) return

  updatingCardsPlayerId.value = player.id
  player.trainingCardsCount = newCount

  const result = await updatePlayerCards(player.id, newCount)
  if (result.success && result.player) {
    player.trainingCardsCount = result.player.trainingCardsCount
    showToast(
      delta > 0 ? 'Charm Awarded! 🎴' : 'Charm Removed',
      `${delta > 0 ? 'Awarded 1 charm to' : 'Updated charms for'} ${player.firstName} (Total: ${result.player.trainingCardsCount}).`,
      'amber'
    )
  } else {
    player.trainingCardsCount = current
    showToast(
      'Failed to update cards',
      result.error || 'Could not update training charms. Please try again.',
      'red'
    )
  }
  updatingCardsPlayerId.value = null
}

async function cycleSubStatus(player: Player) {
  const current = SUB_STATUSES.indexOf(player.subscriptionStatus)
  const next = SUB_STATUSES[(current + 1) % SUB_STATUSES.length]

  updatingSubStatus.value = player.id
  try {
    await $fetch(`/api/players/${player.id}/subscription-status`, {
      method: 'PATCH',
      headers: getAuthHeaders(),
      body: { subscriptionStatus: next }
    })
    player.subscriptionStatus = next ?? ''
  } catch (e) {
    console.error('Failed to update subscription status', e)
  } finally {
    updatingSubStatus.value = null
  }
}

function formatDateForInput(dateStr?: string | null): string {
  if (!dateStr) return ''
  const d = new Date(dateStr)
  return d.toISOString().split('T')[0] ?? ''
}

const ageOptions = ['4-5 yrs', '5-6 yrs', '7-8 yrs', '9-10 yrs', '11-12 yrs', '13-14 yrs']

const form = ref({
  firstName: '',
  lastName: '',
  dateOfBirth: '',
  emergencyContact: '',
  emergencyPhone: '',
  emergencyContact2: '',
  emergencyPhone2: '',
  medicalNotes: '',
  isActive: true,
  subscriptionStatus: 'Active',
  preferredFoot: 'Right',
  coachNotes: '',
  fanNumber: '',
  shirtSize: '',
  shortSize: '',
  sockSize: null as number | null,
  allergies: '',
  allowPhotos: false,
  trainingCardsCount: 0,
  signingFeePaid: false,
  teamId: '',
  teamIds: [] as string[]
})

function toggleTeam(teamId: string) {
  const idx = form.value.teamIds.indexOf(teamId)
  if (idx === -1) {
    form.value.teamIds.push(teamId)
  } else {
    form.value.teamIds.splice(idx, 1)
  }
  form.value.teamId = form.value.teamIds[0] || ''
}

onMounted(() => {
  fetchPlayers()
  fetchTeams()
})

function openCreateModal() {
  editingPlayer.value = null
  form.value = {
    firstName: '',
    lastName: '',
    dateOfBirth: '',
    emergencyContact: '',
    emergencyPhone: '',
    emergencyContact2: '',
    emergencyPhone2: '',
    medicalNotes: '',
    isActive: true,
    subscriptionStatus: 'Active',
    preferredFoot: 'Right',
    coachNotes: '',
    fanNumber: '',
    shirtSize: '',
    shortSize: '',
    sockSize: null,
    allergies: '',
    allowPhotos: false,
    trainingCardsCount: 0,
    signingFeePaid: false,
    teamId: '',
    teamIds: []
  }
  formError.value = null
  isModalOpen.value = true
}

function openEditModal(player: Player) {
  const playerTeamIds = player.teamIds && player.teamIds.length > 0
    ? [...player.teamIds]
    : (player.teams && player.teams.length > 0
      ? player.teams.map(t => t.id)
      : (player.teamId ? [player.teamId] : []))

  editingPlayer.value = player
  form.value = {
    firstName: player.firstName,
    lastName: player.lastName,
    dateOfBirth: formatDateForInput(player.dateOfBirth),
    emergencyContact: player.emergencyContact || '',
    emergencyPhone: player.emergencyPhone || '',
    emergencyContact2: player.emergencyContact2 || '',
    emergencyPhone2: player.emergencyPhone2 || '',
    medicalNotes: player.medicalNotes || '',
    isActive: player.isActive,
    subscriptionStatus: player.subscriptionStatus || 'Active',
    preferredFoot: player.preferredFoot || 'Right',
    coachNotes: player.coachNotes || '',
    fanNumber: player.fanNumber || '',
    shirtSize: player.shirtSize || '',
    shortSize: player.shortSize || '',
    sockSize: player.sockSize !== null && player.sockSize !== undefined ? player.sockSize : null,
    allergies: player.allergies || '',
    allowPhotos: player.allowPhotos ?? false,
    trainingCardsCount: player.trainingCardsCount || 0,
    signingFeePaid: player.signingFeePaid ?? false,
    teamId: playerTeamIds[0] || '',
    teamIds: playerTeamIds
  }
  formError.value = null
  isModalOpen.value = true
}

function closeModal() {
  isModalOpen.value = false
}

async function submitForm() {
  formSaving.value = true
  formError.value = null

  const payload = {
    ...form.value,
    dateOfBirth: form.value.dateOfBirth ? new Date(form.value.dateOfBirth).toISOString() : null,
    trainingCardsCount: Math.max(0, form.value.trainingCardsCount || 0),
    signingFeePaid: form.value.signingFeePaid,
    teamIds: form.value.teamIds,
    teamId: form.value.teamIds.length > 0 ? form.value.teamIds[0] : null
  }

  const result = editingPlayer.value
    ? await updatePlayer(editingPlayer.value.id, payload)
    : await createPlayer(payload)

  if (result.success) {
    closeModal()
  } else {
    formError.value = result.error || 'An error occurred'
  }

  formSaving.value = false
}
</script>

<style scoped>
.animate-fade-in {
  animation: fadeIn 0.2s ease-out forwards;
}

@keyframes fadeIn {
  from {
    opacity: 0;
    transform: scale(0.95);
  }

  to {
    opacity: 1;
    transform: scale(1);
  }
}
</style>
