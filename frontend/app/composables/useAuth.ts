import { ref, computed } from 'vue'

interface User {
  userId: string
  email: string
  fullName: string
  role: string
  children?: LinkedPlayer[]
}

interface LinkedPlayer {
  playerId: string
  firstName: string
  lastName: string
  relationship: string
}

interface LoginResponse {
  token: string
  userId: string
  email: string
  fullName: string
  role: string
}

const token = ref<string | null>(null)
const user = ref<User | null>(null)
const loading = ref(false)
const error = ref<string | null>(null)

export function useAuth() {
  const isAuthenticated = computed(() => !!token.value)
  const isAdmin = computed(() => user.value?.role === 'Admin')
  const isParent = computed(() => user.value?.role === 'Parent')

  // Initialize from localStorage (client-side only)
  function init() {
    if (import.meta.client) {
      const savedToken = localStorage.getItem('celtic_token')
      const savedUser = localStorage.getItem('celtic_user')
      if (savedToken && savedUser) {
        token.value = savedToken
        user.value = JSON.parse(savedUser)
      }
    }
  }

  async function login(email: string, password: string): Promise<boolean> {
    loading.value = true
    error.value = null

    try {
      const response = await $fetch<LoginResponse>('/api/auth/login', {
        method: 'POST',
        body: { email, password },
      })

      token.value = response.token
      user.value = {
        userId: response.userId,
        email: response.email,
        fullName: response.fullName,
        role: response.role,
      }

      // Persist
      if (import.meta.client) {
        localStorage.setItem('celtic_token', response.token)
        localStorage.setItem('celtic_user', JSON.stringify(user.value))
      }

      return true
    } catch (err: any) {
      error.value = err?.data?.message || 'Invalid email or password'
      return false
    } finally {
      loading.value = false
    }
  }

  async function fetchMe(): Promise<void> {
    if (!token.value) return

    try {
      const response = await $fetch<User & { children: LinkedPlayer[] }>('/api/auth/me', {
        headers: { Authorization: `Bearer ${token.value}` },
      })

      user.value = {
        userId: response.userId,
        email: response.email,
        fullName: response.fullName,
        role: response.role,
        children: response.children,
      }

      if (import.meta.client) {
        localStorage.setItem('celtic_user', JSON.stringify(user.value))
      }
    } catch {
      logout()
    }
  }

  function logout() {
    token.value = null
    user.value = null
    if (import.meta.client) {
      localStorage.removeItem('celtic_token')
      localStorage.removeItem('celtic_user')
    }
    navigateTo('/login')
  }

  function getAuthHeaders(): Record<string, string> {
    if (!token.value) return {}
    return { Authorization: `Bearer ${token.value}` }
  }

  return {
    token,
    user,
    loading,
    error,
    isAuthenticated,
    isAdmin,
    isParent,
    init,
    login,
    fetchMe,
    logout,
    getAuthHeaders,
  }
}
