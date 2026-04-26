<template>
  <div>
    <div class="flex items-center justify-between mb-8">
      <div>
        <h1 class="text-2xl font-bold text-text-primary">Team Roster</h1>
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

    <!-- Roster Grid -->
    <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      <div v-for="player in players" :key="player.id" class="card p-5 relative group">
        <div class="absolute top-4 right-4">
          <span :class="['badge', player.isActive ? 'badge-success' : 'badge-warning']">
            {{ player.isActive ? 'Active' : 'Inactive' }}
          </span>
        </div>
        
        <h3 class="text-lg font-bold text-text-primary mb-1">{{ player.firstName }} {{ player.lastName }}</h3>
        
        <div class="mt-4 space-y-3">
          <div v-if="player.dateOfBirth">
            <span class="text-xs text-text-muted uppercase tracking-wide">DOB</span>
            <p class="text-sm text-text-secondary">{{ new Date(player.dateOfBirth).toLocaleDateString() }}</p>
          </div>
          <div v-if="player.emergencyContact">
            <span class="text-xs text-text-muted uppercase tracking-wide">Emergency Contact</span>
            <p class="text-sm text-text-secondary">{{ player.emergencyContact }} <span v-if="player.emergencyPhone">({{ player.emergencyPhone }})</span></p>
          </div>
          <div v-if="player.medicalNotes">
            <span class="text-xs text-text-muted uppercase tracking-wide">Medical Notes</span>
            <p class="text-sm text-warning mt-1 bg-warning/10 p-2 rounded">{{ player.medicalNotes }}</p>
          </div>
        </div>

        <div class="pt-4 mt-4 border-t border-border flex justify-end">
          <button @click="openEditModal(player)" class="text-sm text-celtic-gold hover:text-celtic-gold-light font-medium transition-colors">
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
    <div v-if="isModalOpen" class="fixed inset-0 z-[100] flex items-center justify-center bg-black/60 backdrop-blur-sm p-4">
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

          <div>
            <label class="block text-sm font-medium text-text-secondary mb-1">Date of Birth</label>
            <input v-model="form.dateOfBirth" type="date" class="input" />
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

          <div>
            <label class="block text-sm font-medium text-text-secondary mb-1">Medical Notes</label>
            <textarea v-model="form.medicalNotes" class="input min-h-[80px]" placeholder="Allergies, conditions..."></textarea>
          </div>

          <div v-if="editingPlayer" class="flex items-center gap-2 mt-2">
            <input type="checkbox" id="isActive" v-model="form.isActive" class="rounded border-border text-celtic-green focus:ring-celtic-green w-4 h-4" />
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

definePageMeta({
  layout: 'app',
  middleware: 'auth', // we should have an admin middleware, but doing this via layout
})

useHead({
  title: 'Roster - Celtic FC',
})

const { players, loading, error, fetchPlayers, createPlayer, updatePlayer } = usePlayers()

const isModalOpen = ref(false)
const editingPlayer = ref<Player | null>(null)
const formSaving = ref(false)
const formError = ref<string | null>(null)

// Format date for date-input value
function formatDateForInput(dateStr?: string | null) {
  if (!dateStr) return ''
  const d = new Date(dateStr)
  return d.toISOString().split('T')[0]
}

const form = ref({
  firstName: '',
  lastName: '',
  dateOfBirth: '',
  emergencyContact: '',
  emergencyPhone: '',
  medicalNotes: '',
  isActive: true
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
    medicalNotes: '',
    isActive: true
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
    medicalNotes: player.medicalNotes || '',
    isActive: player.isActive
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
  
  // Format payload
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
  from { opacity: 0; transform: scale(0.95); }
  to { opacity: 1; transform: scale(1); }
}
</style>
