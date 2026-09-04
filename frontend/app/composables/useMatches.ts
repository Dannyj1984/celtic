import { ref } from 'vue'
import { useAuth } from './useAuth'

export interface Match {
  id: string
  seasonId?: string | null
  seasonName?: string | null
  date: string
  opposition: string
  location?: string | null
  halfDurationMinutes?: number
  format?: string
  goalsFor: number
  goalsAgainst: number
  matchReport?: string | null
  isPublished: boolean
  result: string
  eventId?: string | null
  playerOfTheMatchId?: string | null
  playerOfTheMatchName?: string | null
  teamId?: string | null
  teamName?: string | null
}

export function useMatches() {
  const { getAuthHeaders } = useAuth()
  const matches = ref<Match[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function fetchMatches() {
    loading.value = true
    error.value = null
    try {
      matches.value = await $fetch<Match[]>('/api/matches', {
        headers: getAuthHeaders(),
      })
    } catch (err: any) {
      error.value = err?.data?.message || 'Failed to fetch matches'
    } finally {
      loading.value = false
    }
  }

  async function createMatch(matchData: any) {
    try {
      const newMatch = await $fetch<Match>('/api/matches', {
        method: 'POST',
        headers: getAuthHeaders(),
        body: matchData,
      })
      matches.value.unshift(newMatch)
      return { success: true, match: newMatch }
    } catch (err: any) {
      return { success: false, error: err?.data?.message || 'Failed to create match' }
    }
  }

  async function updateMatch(id: string, matchData: any) {
    try {
      const updatedMatch = await $fetch<Match>(`/api/matches/${id}`, {
        method: 'PUT',
        headers: getAuthHeaders(),
        body: matchData,
      })
      const index = matches.value.findIndex(m => m.id === id)
      if (index !== -1) {
        matches.value[index] = updatedMatch
      }
      return { success: true, match: updatedMatch }
    } catch (err: any) {
      return { success: false, error: err?.data?.message || 'Failed to update match' }
    }
  }

  async function deleteMatch(id: string) {
    try {
      await $fetch(`/api/matches/${id}`, {
        method: 'DELETE',
        headers: getAuthHeaders(),
      })
      matches.value = matches.value.filter(m => m.id !== id)
      return { success: true }
    } catch (err: any) {
      return { success: false, error: err?.data?.message || 'Failed to delete match' }
    }
  }

  return {
    matches,
    loading,
    error,
    fetchMatches,
    createMatch,
    updateMatch,
    deleteMatch,
  }
}
