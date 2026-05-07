<template>
  <div>
    <div class="flex items-center justify-between mb-8">
      <div>
        <h1 class="text-2xl font-bold text-text-primary">Season Settings</h1>
        <p class="text-text-secondary mt-1">Manage team seasons, sub amounts, and active dates</p>
      </div>
      <button @click="openCreateModal" class="btn-primary">
        + Add Season
      </button>
    </div>

    <div v-if="loading" class="flex justify-center py-12">
      <div class="animate-spin w-8 h-8 rounded-full border-4 border-celtic-green border-t-transparent"></div>
    </div>
    
    <div v-else-if="error" class="bg-danger/10 border border-danger/20 text-danger p-4 rounded-lg">
      {{ error }}
    </div>

    <!-- Seasons List -->
    <div v-else class="space-y-4">
      <div v-for="season in seasons" :key="season.id" class="card p-6 flex flex-col md:flex-row md:items-center justify-between gap-4">
        <div class="flex items-center gap-4">
          <div :class="['w-12 h-12 rounded-xl flex items-center justify-center shrink-0', season.isCurrent ? 'bg-celtic-green/20 text-celtic-green border border-celtic-green/30' : 'bg-surface-light text-text-muted border border-border']">
            <svg xmlns="http://www.w3.org/2000/svg" class="w-6 h-6" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <rect x="3" y="4" width="18" height="18" rx="2" ry="2" />
              <line x1="16" y1="2" x2="16" y2="6" />
              <line x1="8" y1="2" x2="8" y2="6" />
              <line x1="3" y1="10" x2="21" y2="10" />
            </svg>
          </div>
          <div>
            <div class="flex items-center gap-2">
              <h3 class="text-lg font-bold text-text-primary">{{ season.name }}</h3>
              <span v-if="season.isCurrent" class="badge badge-success">Current Season</span>
            </div>
            <p class="text-sm text-text-secondary mt-1">
              {{ new Date(season.startDate).toLocaleDateString() }} 
              &rarr; 
              {{ new Date(season.endDate).toLocaleDateString() }}
            </p>
          </div>
        </div>

        <div class="flex items-center gap-6">
          <div class="text-right hidden sm:block">
            <span class="text-xs text-text-muted uppercase tracking-wide">Subs</span>
            <p class="font-bold text-celtic-gold">£{{ season.subAmount.toFixed(2) }} <span class="text-xs font-normal text-text-secondary">/ {{ season.subFrequency.toLowerCase() }}</span></p>
          </div>
          <button @click="openEditModal(season)" class="btn-secondary whitespace-nowrap">
            Edit
          </button>
        </div>
      </div>

      <!-- Empty State -->
      <div v-if="seasons.length === 0" class="card p-12 text-center border-dashed">
        <h3 class="text-lg font-medium text-text-primary mb-2">No seasons configured</h3>
        <p class="text-text-muted text-sm mb-6">Create your first season to start tracking finances and events.</p>
        <button @click="openCreateModal" class="btn-primary inline-flex items-center gap-2">
          Create Season
        </button>
      </div>
    </div>

    <!-- Season Modal -->
    <div v-if="isModalOpen" class="fixed inset-0 z-[100] flex items-center justify-center bg-black/60 backdrop-blur-sm p-4">
      <div class="card w-full max-w-md p-6 animate-fade-in shadow-2xl border-celtic-green/30">
        <h2 class="text-xl font-bold text-text-primary mb-6">
          {{ editingSeason ? 'Edit Season' : 'Create Season' }}
        </h2>

        <form @submit.prevent="submitForm" class="space-y-4 text-left">
          <div>
            <label class="block text-sm font-medium text-text-secondary mb-1">Season Name *</label>
            <input v-model="form.name" type="text" class="input" placeholder="e.g. 2026-27" required />
          </div>

          <div class="grid grid-cols-2 gap-4">
            <div>
              <label class="block text-sm font-medium text-text-secondary mb-1">Start Date *</label>
              <input v-model="form.startDate" type="date" class="input" required />
            </div>
            <div>
              <label class="block text-sm font-medium text-text-secondary mb-1">End Date *</label>
              <input v-model="form.endDate" type="date" class="input" required />
            </div>
          </div>

          <div class="grid grid-cols-2 gap-4">
            <div>
              <label class="block text-sm font-medium text-text-secondary mb-1">Sub Amount (£) *</label>
              <input v-model.number="form.subAmount" type="number" min="0" step="0.01" class="input" required />
            </div>
            <div>
              <label class="block text-sm font-medium text-text-secondary mb-1">Frequency *</label>
              <select v-model="form.subFrequency" class="input bg-surface-light text-text-primary" required>
                <option value="Weekly">Weekly</option>
                <option value="Monthly">Monthly</option>
                <option value="Termly">Termly</option>
              </select>
            </div>
          </div>

          <div class="flex items-center gap-2 mt-4 p-3 bg-surface-hover rounded-lg border border-border">
            <input type="checkbox" id="isCurrent" v-model="form.isCurrent" class="rounded border-border text-celtic-green focus:ring-celtic-green w-4 h-4" />
            <label for="isCurrent" class="text-sm font-medium text-text-secondary w-full cursor-pointer">
              Set as current active season
              <p class="text-xs text-text-muted mt-1">This will automatically un-set any other current seasons.</p>
            </label>
          </div>

          <div v-if="formError" class="text-danger text-sm mt-2">
            {{ formError }}
          </div>

          <div class="flex justify-end gap-3 mt-6 pt-4 border-t border-border">
            <button type="button" @click="closeModal" class="btn-secondary">Cancel</button>
            <button type="submit" class="btn-primary" :disabled="formSaving">
              {{ formSaving ? 'Saving...' : 'Save Season' }}
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useSeasons, type Season } from '~/composables/useSeasons'

definePageMeta({
  layout: 'app',
})

useHead({
  title: 'Seasons - Celtic FC',
})

const { seasons, loading, error, fetchSeasons, createSeason, updateSeason } = useSeasons()

const isModalOpen = ref(false)
const editingSeason = ref<Season | null>(null)
const formSaving = ref(false)
const formError = ref<string | null>(null)

function formatDateForInput(dateStr: string) {
  const d = new Date(dateStr)
  return d.toISOString().split('T')[0]
}

const form = ref({
  name: '',
  startDate: '',
  endDate: '',
  subAmount: 0,
  subFrequency: 'Monthly' as 'Weekly'|'Monthly'|'Termly',
  isCurrent: false
})

onMounted(() => {
  fetchSeasons()
})

function openCreateModal() {
  editingSeason.value = null
  const now = new Date()
  const nextYear = new Date(now.getFullYear() + 1, now.getMonth(), now.getDate())
  
  form.value = {
    name: `${now.getFullYear()}-${(now.getFullYear() + 1).toString().slice(2)}`,
    startDate: formatDateForInput(now.toISOString()),
    endDate: formatDateForInput(nextYear.toISOString()),
    subAmount: 25,
    subFrequency: 'Monthly',
    isCurrent: seasons.value.length === 0
  }
  formError.value = null
  isModalOpen.value = true
}

function openEditModal(season: Season) {
  editingSeason.value = season
  form.value = {
    name: season.name,
    startDate: formatDateForInput(season.startDate),
    endDate: formatDateForInput(season.endDate),
    subAmount: season.subAmount,
    subFrequency: season.subFrequency,
    isCurrent: season.isCurrent
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
    startDate: new Date(form.value.startDate).toISOString(),
    endDate: new Date(form.value.endDate).toISOString(),
  }

  const result = editingSeason.value
    ? await updateSeason(editingSeason.value.id, payload)
    : await createSeason(payload)

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
