import { ref } from 'vue'
import { useAuth } from './useAuth'

export interface Season {
  id: string
  name: string
  startDate: string
  endDate: string
  subAmount: number
  subFrequency: 'Weekly' | 'Monthly' | 'Termly'
  isCurrent: boolean
}

export function useSeasons() {
  const { getAuthHeaders } = useAuth()
  const seasons = ref<Season[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function fetchSeasons() {
    loading.value = true
    error.value = null
    try {
      seasons.value = await $fetch<Season[]>('/api/seasons', {
        headers: getAuthHeaders(),
      })
    } catch (err: any) {
      error.value = err?.data?.message || 'Failed to fetch seasons'
    } finally {
      loading.value = false
    }
  }

  async function createSeason(seasonData: Partial<Season>) {
    try {
      const newSeason = await $fetch<Season>('/api/seasons', {
        method: 'POST',
        headers: getAuthHeaders(),
        body: seasonData,
      })
      seasons.value.unshift(newSeason)
      
      // If it's current, we need to manually update local state since backend sets others to false
      if (newSeason.isCurrent) {
        seasons.value.forEach(s => {
          if (s.id !== newSeason.id) s.isCurrent = false
        })
      }
      
      return { success: true, season: newSeason }
    } catch (err: any) {
      return { success: false, error: err?.data?.message || 'Failed to create season' }
    }
  }

  async function updateSeason(id: string, seasonData: Partial<Season>) {
    try {
      const updatedSeason = await $fetch<Season>(`/api/seasons/${id}`, {
        method: 'PUT',
        headers: getAuthHeaders(),
        body: seasonData,
      })
      
      // If it's current, others become false
      if (updatedSeason.isCurrent) {
        seasons.value.forEach(s => s.isCurrent = false)
      }

      const index = seasons.value.findIndex(s => s.id === id)
      if (index !== -1) {
        seasons.value[index] = updatedSeason
      }
      
      return { success: true, season: updatedSeason }
    } catch (err: any) {
      return { success: false, error: err?.data?.message || 'Failed to update season' }
    }
  }

  return {
    seasons,
    loading,
    error,
    fetchSeasons,
    createSeason,
    updateSeason,
  }
}
