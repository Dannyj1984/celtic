import { computed } from 'vue'

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

export function useAuth() {
  // Use cookies for token and user to allow SSR access
  const token = useCookie<string | null>('celtic_token', {
    maxAge: 60 * 60 * 24, // 24 hours
    sameSite: 'lax',
  })

  const user = useCookie<User | null>('celtic_user', {
    maxAge: 60 * 60 * 24,
    sameSite: 'lax',
  })

  const loading = useState<boolean>('auth_loading', () => false)
  const error = useState<string | null>('auth_error', () => null)

  const isAuthenticated = computed(() => !!token.value)
  const isAdmin = computed(() => user.value?.role === 'Admin')
  const isParent = computed(() => user.value?.role === 'Parent')

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
    } catch {
      logout()
    }
  }

  function logout() {
    token.value = null
    user.value = null
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
    login,
    fetchMe,
    logout,
    getAuthHeaders,
  }
}
