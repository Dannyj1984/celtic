<template>
  <div>
    <div class="flex items-center justify-between mb-8">
      <div>
        <h1 class="text-2xl font-bold text-text-primary">Squad Management</h1>
        <p class="text-text-secondary mt-1">Manage team players, status, and emergency contacts</p>
      </div>
      <button @click="openCreateModal" class="btn-primary">
        + Add Player
      </button>
    </div>

    <div v-if="loading" class="flex justify-center py-12">
      <div class="animate-spin w-8 h-8 rounded-full border-4 border-celtic-green border-t-transparent"></div>
    </div>

    <div v-else-if="error" class="bg-danger/10 border border-danger/20 text-danger p-4 rounded-lg">
      {{ error }}
    </div>

    <!-- Squad Grid -->
    <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      <div v-for="player in players" :key="player.id" class="card p-5 relative group">

        <!-- Top badges row -->
        <div class="absolute top-4 right-4 flex items-center gap-2">
          <!-- Subscription Status Badge (clickable to cycle) -->
          <button @click="cycleSubStatus(player)" :disabled="updatingSubStatus === player.id"
            :class="['badge text-xs font-semibold transition-all hover:opacity-80 cursor-pointer', subStatusClass(player.subscriptionStatus)]"
            :title="'Click to change subscription status'">
            <span v-if="updatingSubStatus === player.id">...</span>
            <span v-else>{{ player.subscriptionStatus }}</span>
          </button>
        </div>

        <h3 class="text-lg font-bold text-text-primary mb-1 pr-32">{{ player.firstName }} {{ player.lastName }}</h3>
        <div v-if="player.fanNumber || player.shirtSize" class="flex flex-wrap gap-2 mt-1">
          <span v-if="player.fanNumber" class="text-xs font-semibold px-2 py-0.5 rounded bg-surface-hover border border-border/60 text-text-secondary">
            FAN: {{ player.fanNumber }}
          </span>
          <span v-if="player.shirtSize" class="text-xs font-semibold px-2 py-0.5 rounded bg-celtic-green/10 border border-celtic-green/30 text-celtic-green">
            Shirt: {{ player.shirtSize }}
          </span>
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

          <div class="grid grid-cols-2 gap-4">
            <div>
              <label class="block text-sm font-medium text-text-secondary mb-1">Shirt Size</label>
              <input v-model="form.shirtSize" type="text" class="input" placeholder="e.g. YS, YM, S, M" />
            </div>
            <div>
              <label class="block text-sm font-medium text-text-secondary mb-1">Preferred Foot</label>
              <select v-model="form.preferredFoot" class="input">
                <option value="Right">Right</option>
                <option value="Left">Left</option>
                <option value="Both">Both</option>
              </select>
            </div>
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
import { ref, onMounted } from 'vue'
import { usePlayers, type Player } from '~/composables/usePlayers'
import { useAuth } from '~/composables/useAuth'

definePageMeta({
  layout: 'app',
})

useHead({
  title: 'Squad - Celtic FC',
})

const { players, loading, error, fetchPlayers, createPlayer, updatePlayer } = usePlayers()
const { getAuthHeaders } = useAuth()

const isModalOpen = ref(false)
const editingPlayer = ref<Player | null>(null)
const formSaving = ref(false)
const formError = ref<string | null>(null)
const updatingSubStatus = ref<string | null>(null)

const SUB_STATUSES = ['Active', 'Payment Due', 'Inactive']

function subStatusClass(status: string) {
  if (status === 'Active') return 'bg-success/20 text-success border border-success/30'
  if (status === 'Payment Due') return 'bg-warning/20 text-warning border border-warning/30'
  return 'bg-danger/20 text-danger border border-danger/30'
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
    // Update local state immediately
    player.subscriptionStatus = next ?? ''
  } catch (e) {
    console.error('Failed to update subscription status', e)
  } finally {
    updatingSubStatus.value = null
  }
}

// Format date for date-input value
function formatDateForInput(dateStr?: string | null): string {
  if (!dateStr) return ''
  const d = new Date(dateStr)
  return d.toISOString().split('T')[0] ?? ''
}

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
  allergies: ''
})

onMounted(() => {
  fetchPlayers()
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
    allergies: ''
  }
  formError.value = null
  isModalOpen.value = true
}

function openEditModal(player: Player) {
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
    allergies: player.allergies || ''
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
    dateOfBirth: form.value.dateOfBirth ? new Date(form.value.dateOfBirth).toISOString() : null
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
