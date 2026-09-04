import { ref } from 'vue'
import { useAuth } from './useAuth'

export interface Team {
  id: string
  name: string
  colorHex?: string
  isActive: boolean
  playersCount: number
}

export function useTeams() {
  const { getAuthHeaders } = useAuth()
  const teams = ref<Team[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function fetchTeams() {
    loading.value = true
    error.value = null
    try {
      teams.value = await $fetch<Team[]>('/api/teams', {
        headers: getAuthHeaders(),
      })
    } catch (err: any) {
      error.value = err?.data?.message || 'Failed to fetch teams'
    } finally {
      loading.value = false
    }
  }

  async function createTeam(teamData: { name: string; colorHex?: string }) {
    try {
      const newTeam = await $fetch<Team>('/api/teams', {
        method: 'POST',
        headers: getAuthHeaders(),
        body: teamData,
      })
      teams.value.push(newTeam)
      return { success: true, team: newTeam }
    } catch (err: any) {
      return { success: false, error: err?.data?.message || 'Failed to create team' }
    }
  }

  async function updateTeam(id: string, teamData: { name: string; colorHex?: string; isActive: boolean }) {
    try {
      const updated = await $fetch<Team>(`/api/teams/${id}`, {
        method: 'PUT',
        headers: getAuthHeaders(),
        body: teamData,
      })
      const idx = teams.value.findIndex(t => t.id === id)
      if (idx !== -1) {
        teams.value[idx] = updated
      }
      return { success: true, team: updated }
    } catch (err: any) {
      return { success: false, error: err?.data?.message || 'Failed to update team' }
    }
  }

  async function deleteTeam(id: string) {
    try {
      await $fetch(`/api/teams/${id}`, {
        method: 'DELETE',
        headers: getAuthHeaders(),
      })
      teams.value = teams.value.filter(t => t.id !== id)
      return { success: true }
    } catch (err: any) {
      return { success: false, error: err?.data?.message || 'Failed to delete team' }
    }
  }

  return {
    teams,
    loading,
    error,
    fetchTeams,
    createTeam,
    updateTeam,
    deleteTeam,
  }
}
