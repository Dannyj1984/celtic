import { ref } from 'vue'
import { useAuth } from './useAuth'

export interface ParentAccount {
  userId: string
  email: string
  fullName: string
  phone: string
  role: string
  children: {
    playerId: string
    firstName: string
    lastName: string
    relationship: string
  }[]
}

export function useParents() {
  const { getAuthHeaders } = useAuth()
  const parents = ref<ParentAccount[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function fetchParents() {
    loading.value = true
    error.value = null
    try {
      parents.value = await $fetch<ParentAccount[]>('/api/auth/parents', {
        headers: getAuthHeaders(),
      })
    } catch (err: any) {
      error.value = err?.data?.message || 'Failed to fetch parents'
    } finally {
      loading.value = false
    }
  }

  async function linkPlayer(userId: string, playerId: string, relationship: string) {
    try {
      await $fetch('/api/auth/link-player', {
        method: 'POST',
        headers: getAuthHeaders(),
        body: { userId, playerId, relationship },
      })
      await fetchParents()
      return { success: true }
    } catch (err: any) {
      return { success: false, error: err?.data?.message || 'Failed to link player' }
    }
  }

  return {
    parents,
    loading,
    error,
    fetchParents,
    linkPlayer,
  }
}
