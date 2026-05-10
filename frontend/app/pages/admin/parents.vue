<template>
  <div>
    <div class="flex items-center justify-between mb-8">
      <div>
        <h1 class="text-2xl font-bold text-text-primary">Parent Accounts</h1>
        <p class="text-text-secondary mt-1">Manage parent access and linked players</p>
      </div>
      <button @click="openCreateModal" class="btn-primary">
        + Create Account
      </button>
    </div>

    <div v-if="loading" class="flex justify-center py-12">
      <div class="animate-spin w-8 h-8 rounded-full border-4 border-celtic-green border-t-transparent"></div>
    </div>
    
    <div v-else-if="error" class="bg-danger/10 border border-danger/20 text-danger p-4 rounded-lg">
      {{ error }}
    </div>

    <!-- Parents List -->
    <div v-else class="grid grid-cols-1 md:grid-cols-2 gap-6">
      <div v-for="parent in parents" :key="parent.userId" class="card p-6 relative">
        <h3 class="text-lg font-bold text-text-primary mb-1">{{ parent.fullName }}</h3>
        
        <div class="absolute top-6 right-6">
          <button @click="openResetModal(parent)" class="text-[10px] text-text-muted hover:text-danger font-bold uppercase transition-colors">
            Reset Password
          </button>
        </div>
        
        <div class="mt-4 space-y-2">
          <div class="flex items-center gap-2 text-sm text-text-secondary">
            <svg xmlns="http://www.w3.org/2000/svg" class="w-4 h-4 shrink-0" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z" />
              <polyline points="22,6 12,13 2,6" />
            </svg>
            <a :href="'mailto:' + parent.email" class="hover:text-celtic-green transition-colors">{{ parent.email }}</a>
          </div>
          
          <div v-if="parent.phone" class="flex items-center gap-2 text-sm text-text-secondary">
            <svg xmlns="http://www.w3.org/2000/svg" class="w-4 h-4 shrink-0" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <path d="M22 16.92v3a2 2 0 0 1-2.18 2 19.79 19.79 0 0 1-8.63-3.07 19.5 19.5 0 0 1-6-6 19.79 19.79 0 0 1-3.07-8.67A2 2 0 0 1 4.11 2h3a2 2 0 0 1 2 1.72 12.84 12.84 0 0 0 .7 2.81 2 2 0 0 1-.45 2.11L8.09 9.91a16 16 0 0 0 6 6l1.27-1.27a2 2 0 0 1 2.11-.45 12.84 12.84 0 0 0 2.81.7A2 2 0 0 1 22 16.92z" />
            </svg>
            <a :href="'tel:' + parent.phone" class="hover:text-celtic-green transition-colors">{{ parent.phone }}</a>
          </div>
        </div>

        <div class="mt-6 pt-4 border-t border-border">
          <div class="flex items-center justify-between mb-2">
            <span class="text-xs text-text-muted uppercase tracking-wide">Linked Players</span>
            <button @click="openLinkModal(parent)" class="text-[10px] text-celtic-gold hover:text-celtic-gold-light font-bold uppercase transition-colors">
              + Link Player
            </button>
          </div>
          
          <div v-if="parent.children.length === 0" class="text-sm text-text-secondary italic">
            No players linked yet.
          </div>
          
          <div v-else class="space-y-2">
            <div v-for="child in parent.children" :key="child.playerId"
              class="flex items-center justify-between p-2 rounded-lg bg-surface-hover border border-border/50">
              <div class="flex items-center gap-2">
                <span class="text-sm font-medium text-text-primary">{{ child.firstName }} {{ child.lastName }}</span>
                <span class="text-text-muted text-[10px] uppercase">{{ child.relationship }}</span>
              </div>
              <button
                @click="cycleSubStatus(child)"
                :disabled="updatingSubStatus === child.playerId"
                :class="['text-[10px] font-bold px-2 py-0.5 rounded-full border transition-all hover:opacity-80', subStatusClass(child.subscriptionStatus)]"
              >
                {{ updatingSubStatus === child.playerId ? '...' : child.subscriptionStatus }}
              </button>
            </div>
          </div>

        </div>
      </div>

      <!-- Empty State -->
      <div v-if="parents.length === 0" class="col-span-full card p-12 text-center border-dashed">
        <h3 class="text-lg font-medium text-text-primary mb-2">No parent accounts</h3>
        <p class="text-text-muted text-sm mb-6">Create accounts for parents so they can log in, view results, and RSVP.</p>
        <button @click="openCreateModal" class="btn-primary inline-flex items-center gap-2">
          Create first account
        </button>
      </div>
    </div>

    <!-- Create Account Modal -->
    <div v-if="isModalOpen" class="fixed inset-0 z-[100] flex items-center justify-center bg-black/60 backdrop-blur-sm p-4">
      <div class="card w-full max-w-md p-6 animate-fade-in shadow-2xl border-celtic-green/30">
        <h2 class="text-xl font-bold text-text-primary mb-6">Create Parent Account</h2>
        <p class="text-sm text-text-secondary mb-4 tracking-wide">
          Parents cannot register themselves. You must create an account and share the details with them.
        </p>

        <form @submit.prevent="submitForm" class="space-y-4 text-left">
          <div>
            <label class="block text-sm font-medium text-text-secondary mb-1">Full Name *</label>
            <input v-model="form.fullName" type="text" class="input" placeholder="e.g. Jane Doe" required />
          </div>

          <div>
            <label class="block text-sm font-medium text-text-secondary mb-1">Email Address *</label>
            <input v-model="form.email" type="email" class="input" placeholder="jane@example.com" required />
          </div>
          
          <div>
            <label class="block text-sm font-medium text-text-secondary mb-1">Phone Number</label>
            <input v-model="form.phone" type="tel" class="input" placeholder="07..." />
          </div>

          <div>
            <label class="block text-sm font-medium text-text-secondary mb-1">Temporary Password *</label>
            <input v-model="form.password" type="password" class="input" placeholder="••••••••" required minlength="6" />
            <p class="text-xs text-text-muted mt-1">Must be at least 6 characters with a number.</p>
          </div>

          <div v-if="formError" class="text-danger text-sm mt-2">
            {{ formError }}
          </div>

          <div class="flex justify-end gap-3 mt-6 pt-4 border-t border-border">
            <button type="button" @click="closeModal" class="btn-secondary">Cancel</button>
            <button type="submit" class="btn-primary" :disabled="formSaving">
              {{ formSaving ? 'Creating...' : 'Create Account' }}
            </button>
          </div>
        </form>
      </div>
    </div>

    <!-- Link Player Modal -->
    <div v-if="isLinkModalOpen" class="fixed inset-0 z-[100] flex items-center justify-center bg-black/60 backdrop-blur-sm p-4">
      <div class="card w-full max-w-md p-6 animate-fade-in shadow-2xl border-celtic-green/30">
        <h2 class="text-xl font-bold text-text-primary mb-2">Link Player</h2>
        <p class="text-sm text-text-secondary mb-6">
          Link a player to <span class="font-bold text-text-primary">{{ selectedParentForLink?.fullName }}</span>
        </p>

        <form @submit.prevent="submitLinkForm" class="space-y-4 text-left">
          <div>
            <label class="block text-sm font-medium text-text-secondary mb-1">Select Player *</label>
            <select v-model="linkForm.playerId" class="input" required>
              <option value="" disabled>Choose a player...</option>
              <option v-for="player in availablePlayers" :key="player.id" :value="player.id">
                {{ player.firstName }} {{ player.lastName }}
              </option>
            </select>
          </div>

          <div>
            <label class="block text-sm font-medium text-text-secondary mb-1">Relationship *</label>
            <select v-model="linkForm.relationship" class="input" required>
              <option value="Father">Father</option>
              <option value="Mother">Mother</option>
              <option value="Guardian">Guardian</option>
              <option value="Step-parent">Step-parent</option>
              <option value="Other">Other</option>
            </select>
          </div>

          <div v-if="linkError" class="text-danger text-sm mt-2">
            {{ linkError }}
          </div>

          <div class="flex justify-end gap-3 mt-6 pt-4 border-t border-border">
            <button type="button" @click="closeLinkModal" class="btn-secondary">Cancel</button>
            <button type="submit" class="btn-primary" :disabled="linkSaving">
              {{ linkSaving ? 'Linking...' : 'Link Player' }}
            </button>
          </div>
        </form>
      </div>
    </div>

    <!-- Reset Password Modal -->
    <div v-if="isResetModalOpen" class="fixed inset-0 z-[100] flex items-center justify-center bg-black/60 backdrop-blur-sm p-4">
      <div class="card w-full max-w-md p-6 animate-fade-in shadow-2xl border-danger/30">
        <h2 class="text-xl font-bold text-text-primary mb-2">Reset Password</h2>
        <p class="text-sm text-text-secondary mb-6">
          Set a new temporary password for <span class="font-bold text-text-primary">{{ selectedParentForReset?.fullName }}</span>
        </p>

        <form @submit.prevent="submitResetForm" class="space-y-4 text-left">
          <div>
            <label class="block text-sm font-medium text-text-secondary mb-1">New Password *</label>
            <input v-model="resetForm.password" type="password" class="input" placeholder="••••••••" required minlength="6" />
            <p class="text-xs text-text-muted mt-1">Must be at least 6 characters with a number.</p>
          </div>

          <div v-if="resetError" class="text-danger text-sm mt-2">
            {{ resetError }}
          </div>

          <div class="flex justify-end gap-3 mt-6 pt-4 border-t border-border">
            <button type="button" @click="isResetModalOpen = false" class="btn-secondary">Cancel</button>
            <button type="submit" class="btn-primary" :disabled="resetSaving">
              {{ resetSaving ? 'Resetting...' : 'Update Password' }}
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useParents, type ParentAccount } from '~/composables/useParents'
import { usePlayers } from '~/composables/usePlayers'
import { useAuth } from '~/composables/useAuth'

definePageMeta({
  layout: 'app',
})

useHead({
  title: 'Parents - Celtic FC',
})

const { parents, loading, error, fetchParents, linkPlayer } = useParents()
const { players: squad, fetchPlayers } = usePlayers()
const { getAuthHeaders } = useAuth()

// Subscription status helpers
const SUB_STATUSES = ['Active', 'Payment Due', 'Inactive']
const updatingSubStatus = ref<string | null>(null)

function subStatusClass(status: string) {
  if (status === 'Active') return 'bg-success/20 text-success border-success/30'
  if (status === 'Payment Due') return 'bg-warning/20 text-warning border-warning/30'
  return 'bg-danger/20 text-danger border-danger/30'
}

async function cycleSubStatus(child: { playerId: string; subscriptionStatus: string }) {
  const current = SUB_STATUSES.indexOf(child.subscriptionStatus)
  const next = SUB_STATUSES[(current + 1) % SUB_STATUSES.length]
  updatingSubStatus.value = child.playerId
  try {
    await $fetch(`/api/players/${child.playerId}/subscription-status`, {
      method: 'PATCH',
      headers: getAuthHeaders(),
      body: { subscriptionStatus: next }
    })
    child.subscriptionStatus = next
  } catch (e) {
    console.error('Failed to update subscription status', e)
  } finally {
    updatingSubStatus.value = null
  }
}

// Create Account State
const isModalOpen = ref(false)
const formSaving = ref(false)
const formError = ref<string | null>(null)
const form = ref({
  fullName: '',
  email: '',
  password: '',
  phone: ''
})

// Linking State
const isLinkModalOpen = ref(false)
const selectedParentForLink = ref<ParentAccount | null>(null)
const linkSaving = ref(false)
const linkError = ref<string | null>(null)
const linkForm = ref({
  playerId: '',
  relationship: 'Father'
})

// Reset Password State
const isResetModalOpen = ref(false)
const selectedParentForReset = ref<ParentAccount | null>(null)
const resetSaving = ref(false)
const resetError = ref<string | null>(null)
const resetForm = ref({
  password: ''
})

const availablePlayers = computed(() => {
  if (!selectedParentForLink.value) return squad.value
  const linkedIds = selectedParentForLink.value.children.map(c => c.playerId)
  return squad.value.filter(p => !linkedIds.includes(p.id))
})

onMounted(() => {
  fetchParents()
  fetchPlayers()
})

function openCreateModal() {
  form.value = {
    fullName: '',
    email: '',
    password: '',
    phone: ''
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
  
  try {
    const payload = {
      fullName: form.value.fullName,
      email: form.value.email,
      password: form.value.password,
      phone: form.value.phone || null,
      role: 'Parent'
    }

    await $fetch('/api/auth/create-account', {
      method: 'POST',
      headers: getAuthHeaders(),
      body: payload
    })

    await fetchParents()
    closeModal()
  } catch (err: any) {
    formError.value = err?.data?.message || 'Failed to create parent account'
  } finally {
    formSaving.value = false
  }
}

// Linking Functions
function openLinkModal(parent: ParentAccount) {
  selectedParentForLink.value = parent
  linkForm.value = {
    playerId: '',
    relationship: 'Father'
  }
  linkError.value = null
  isLinkModalOpen.value = true
}

function closeLinkModal() {
  isLinkModalOpen.value = false
}

async function submitLinkForm() {
  if (!selectedParentForLink.value) return
  
  linkSaving.value = true
  linkError.value = null
  
  const result = await linkPlayer(
    selectedParentForLink.value.userId, 
    linkForm.value.playerId, 
    linkForm.value.relationship
  )
  
  if (result.success) {
    closeLinkModal()
  } else {
    linkError.value = result.error || 'An error occurred'
  }
  
  linkSaving.value = false
}

// Reset Password Functions
function openResetModal(parent: ParentAccount) {
  selectedParentForReset.value = parent
  resetForm.value.password = ''
  resetError.value = null
  isResetModalOpen.value = true
}

async function submitResetForm() {
  if (!selectedParentForReset.value) return
  
  resetSaving.value = true
  resetError.value = null
  
  try {
    await $fetch('/api/auth/reset-password', {
      method: 'POST',
      headers: getAuthHeaders(),
      body: {
        userId: selectedParentForReset.value.userId,
        newPassword: resetForm.value.password
      }
    })
    
    isResetModalOpen.value = false
    // You could add a toast notification here if you have one
  } catch (err: any) {
    resetError.value = err?.data?.message || 'Failed to reset password'
  } finally {
    resetSaving.value = false
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
