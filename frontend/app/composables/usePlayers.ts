import { ref } from 'vue'
import { useAuth } from './useAuth'

export interface Player {
  id: string
  firstName: string
  lastName: string
  dateOfBirth?: string | null
  medicalNotes?: string | null
  emergencyContact?: string | null
  emergencyPhone?: string | null
  isActive: boolean
}

export function usePlayers() {
  const { getAuthHeaders } = useAuth()
  const players = ref<Player[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function fetchPlayers() {
    loading.value = true
    error.value = null
    try {
      players.value = await $fetch<Player[]>('/api/players', {
        headers: getAuthHeaders(),
      })
    } catch (err: any) {
      error.value = err?.data?.message || 'Failed to fetch players'
    } finally {
      loading.value = false
    }
  }

  async function createPlayer(playerData: Partial<Player>) {
    try {
      const newPlayer = await $fetch<Player>('/api/players', {
        method: 'POST',
        headers: getAuthHeaders(),
        body: playerData,
      })
      players.value.push(newPlayer)
      return { success: true, player: newPlayer }
    } catch (err: any) {
      return { success: false, error: err?.data?.message || 'Failed to create player' }
    }
  }

  async function updatePlayer(id: string, playerData: Partial<Player>) {
    try {
      const updatedPlayer = await $fetch<Player>(`/api/players/${id}`, {
        method: 'PUT',
        headers: getAuthHeaders(),
        body: playerData,
      })
      const index = players.value.findIndex(p => p.id === id)
      if (index !== -1) {
        players.value[index] = updatedPlayer
      }
      return { success: true, player: updatedPlayer }
    } catch (err: any) {
      return { success: false, error: err?.data?.message || 'Failed to update player' }
    }
  }

  return {
    players,
    loading,
    error,
    fetchPlayers,
    createPlayer,
    updatePlayer,
  }
}
