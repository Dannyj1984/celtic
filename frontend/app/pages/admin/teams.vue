<template>
  <div>
    <div class="flex items-center justify-between mb-8">
      <div>
        <h1 class="text-2xl font-bold text-text-primary">Sub-Teams Management</h1>
        <p class="text-text-secondary mt-1">Setup and manage sub-teams (e.g. Stripes & Hoops) for squad players</p>
      </div>
      <button @click="openCreateModal" class="btn-primary flex items-center gap-2">
        + Add Team
      </button>
    </div>

    <div v-if="loading" class="flex justify-center py-12">
      <div class="animate-spin w-8 h-8 rounded-full border-4 border-celtic-green border-t-transparent"></div>
    </div>
    
    <div v-else-if="error" class="bg-danger/10 border border-danger/20 text-danger p-4 rounded-lg">
      {{ error }}
    </div>

    <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      <div v-for="team in teams" :key="team.id" class="card p-6 border-l-4 relative group" :style="{ borderLeftColor: team.colorHex || '#006837' }">
        <div class="flex items-center justify-between mb-4">
          <div class="flex items-center gap-3">
            <div class="w-8 h-8 rounded-full flex items-center justify-center font-bold text-white text-xs shadow-inner" :style="{ backgroundColor: team.colorHex || '#006837' }">
              {{ team.name.substring(0, 2).toUpperCase() }}
            </div>
            <div>
              <h3 class="text-lg font-bold text-text-primary">{{ team.name }}</h3>
              <span v-if="team.isActive" class="badge badge-success text-[10px]">Active</span>
              <span v-else class="badge bg-text-muted/10 text-text-muted text-[10px]">Inactive</span>
            </div>
          </div>
          <button @click="openEditModal(team)" class="btn-secondary text-xs py-1 px-3">
            Edit
          </button>
        </div>

        <div class="bg-surface-hover p-4 rounded-xl flex items-center justify-between text-sm">
          <span class="text-text-muted font-medium">Assigned Squad Players</span>
          <span class="font-bold text-text-primary text-base">{{ team.playersCount }} Players</span>
        </div>
      </div>

      <div v-if="teams.length === 0" class="col-span-full card p-12 text-center border-dashed">
        <h3 class="text-lg font-medium text-text-primary mb-2">No teams configured</h3>
        <p class="text-text-muted text-sm mb-6">Create sub-teams like Stripes and Hoops to organize squad players and match schedules.</p>
        <button @click="openCreateModal" class="btn-primary inline-flex items-center gap-2">
          Create First Team
        </button>
      </div>
    </div>

    <!-- Team Modal -->
    <div v-if="isModalOpen" class="fixed inset-0 z-[100] flex items-center justify-center bg-black/60 backdrop-blur-sm p-4">
      <div class="card w-full max-w-md p-6 animate-fade-in shadow-2xl border-celtic-green/30">
        <h2 class="text-xl font-bold text-text-primary mb-6">
          {{ editingTeam ? 'Edit Team' : 'Create Team' }}
        </h2>

        <form @submit.prevent="submitForm" class="space-y-4 text-left">
          <div>
            <label class="block text-sm font-medium text-text-secondary mb-1">Team Name *</label>
            <input v-model="form.name" type="text" class="input" placeholder="e.g. Stripes or Hoops" required />
          </div>

          <div>
            <label class="block text-sm font-medium text-text-secondary mb-1">Team Color Tag</label>
            <div class="flex items-center gap-3">
              <input v-model="form.colorHex" type="color" class="w-10 h-10 rounded-lg cursor-pointer border border-border bg-transparent p-0.5" />
              <input v-model="form.colorHex" type="text" class="input flex-1 text-sm uppercase font-mono" placeholder="#006837" />
            </div>
          </div>

          <div v-if="editingTeam" class="flex items-center gap-3 py-2">
            <input type="checkbox" v-model="form.isActive" id="isActive" class="rounded border-border text-celtic-green focus:ring-celtic-green w-4 h-4" />
            <label for="isActive" class="text-sm font-medium text-text-secondary cursor-pointer">Active Team</label>
          </div>

          <div v-if="formError" class="text-danger text-sm mt-2">
            {{ formError }}
          </div>

          <div class="flex justify-end gap-3 mt-6 pt-4 border-t border-border">
            <button type="button" @click="isModalOpen = false" class="btn-secondary">Cancel</button>
            <button type="submit" class="btn-primary" :disabled="formSaving">
              {{ formSaving ? 'Saving...' : 'Save Team' }}
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useTeams, type Team } from '~/composables/useTeams'

definePageMeta({
  layout: 'app',
})

useHead({
  title: 'Teams - Stalybridge Celtic U7',
})

const { teams, loading, error, fetchTeams, createTeam, updateTeam } = useTeams()

const isModalOpen = ref(false)
const editingTeam = ref<Team | null>(null)
const formSaving = ref(false)
const formError = ref<string | null>(null)

const form = ref({
  name: '',
  colorHex: '#006837',
  isActive: true,
})

onMounted(() => {
  fetchTeams()
})

function openCreateModal() {
  editingTeam.value = null
  form.value = {
    name: '',
    colorHex: '#006837',
    isActive: true,
  }
  formError.value = null
  isModalOpen.value = true
}

function openEditModal(team: Team) {
  editingTeam.value = team
  form.value = {
    name: team.name,
    colorHex: team.colorHex || '#006837',
    isActive: team.isActive,
  }
  formError.value = null
  isModalOpen.value = true
}

async function submitForm() {
  formSaving.value = true
  formError.value = null

  const res = editingTeam.value
    ? await updateTeam(editingTeam.value.id, form.value)
    : await createTeam({ name: form.value.name, colorHex: form.value.colorHex })

  if (res.success) {
    isModalOpen.value = false
    await fetchTeams()
  } else {
    formError.value = res.error || 'Failed to save team'
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
