import { describe, it, expect, vi, beforeEach } from 'vitest'
import { ref, computed } from 'vue'

// Mock Nuxt auto-imports before importing useAuth
const mockToken = ref<string | null>(null)
const mockUser = ref<any>(null)

vi.stubGlobal('useCookie', vi.fn((name: string) => {
  if (name === 'celtic_token') return mockToken
  if (name === 'celtic_user') return mockUser
  return ref(null)
}))

vi.stubGlobal('useState', vi.fn((_key: string, init: () => any) => ref(init())))
vi.stubGlobal('navigateTo', vi.fn())
vi.stubGlobal('$fetch', vi.fn())

import { useAuth } from '~/composables/useAuth'

describe('useAuth', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    mockToken.value = null
    mockUser.value = null
  })

  describe('initial state', () => {
    it('returns all expected properties', () => {
      const auth = useAuth()
      expect(auth).toHaveProperty('token')
      expect(auth).toHaveProperty('user')
      expect(auth).toHaveProperty('loading')
      expect(auth).toHaveProperty('error')
      expect(auth).toHaveProperty('isAuthenticated')
      expect(auth).toHaveProperty('isAdmin')
      expect(auth).toHaveProperty('isParent')
      expect(auth).toHaveProperty('login')
      expect(auth).toHaveProperty('fetchMe')
      expect(auth).toHaveProperty('logout')
      expect(auth).toHaveProperty('getAuthHeaders')
    })

    it('isAuthenticated is false when no token', () => {
      const { isAuthenticated } = useAuth()
      expect(isAuthenticated.value).toBe(false)
    })

    it('isAuthenticated is true when token exists', () => {
      mockToken.value = 'test-token'
      const { isAuthenticated } = useAuth()
      expect(isAuthenticated.value).toBe(true)
    })

    it('isAdmin is true when user role is Admin', () => {
      mockUser.value = { role: 'Admin' }
      const { isAdmin } = useAuth()
      expect(isAdmin.value).toBe(true)
    })

    it('isAdmin is false for non-admin users', () => {
      mockUser.value = { role: 'Parent' }
      const { isAdmin } = useAuth()
      expect(isAdmin.value).toBe(false)
    })

    it('isParent is true when user role is Parent', () => {
      mockUser.value = { role: 'Parent' }
      const { isParent } = useAuth()
      expect(isParent.value).toBe(true)
    })
  })

  describe('login', () => {
    it('sets token and user on successful login', async () => {
      const mockResponse = {
        token: 'jwt-token-123',
        userId: 'user-1',
        email: 'test@celtic.app',
        fullName: 'Test User',
        role: 'Parent'
      }
      vi.mocked($fetch).mockResolvedValue(mockResponse)

      const { login } = useAuth()
      const result = await login('test@celtic.app', 'password123')

      expect(result).toBe(true)
      expect(mockToken.value).toBe('jwt-token-123')
      expect(mockUser.value).toEqual({
        userId: 'user-1',
        email: 'test@celtic.app',
        fullName: 'Test User',
        role: 'Parent'
      })
    })

    it('calls $fetch with correct endpoint and body', async () => {
      vi.mocked($fetch).mockResolvedValue({ token: 't', userId: 'u', email: 'e', fullName: 'f', role: 'r' })

      const { login } = useAuth()
      await login('user@test.com', 'pass123')

      expect($fetch).toHaveBeenCalledWith('/api/auth/login', {
        method: 'POST',
        body: { email: 'user@test.com', password: 'pass123' }
      })
    })

    it('returns false and sets error on failed login', async () => {
      vi.mocked($fetch).mockRejectedValue({ data: { message: 'Invalid credentials' } })

      const { login, error } = useAuth()
      const result = await login('bad@email.com', 'wrong')

      expect(result).toBe(false)
      expect(error.value).toBe('Invalid credentials')
    })

    it('sets a default error message when server provides none', async () => {
      vi.mocked($fetch).mockRejectedValue(new Error('Network error'))

      const { login, error } = useAuth()
      await login('user@test.com', 'pass')

      expect(error.value).toBe('Invalid email or password')
    })
  })

  describe('logout', () => {
    it('clears token, user, and navigates to login', () => {
      mockToken.value = 'some-token'
      mockUser.value = { fullName: 'Test' }

      const { logout } = useAuth()
      logout()

      expect(mockToken.value).toBeNull()
      expect(mockUser.value).toBeNull()
      expect(navigateTo).toHaveBeenCalledWith('/login')
    })
  })

  describe('getAuthHeaders', () => {
    it('returns Authorization header when token exists', () => {
      mockToken.value = 'my-token'
      const { getAuthHeaders } = useAuth()
      expect(getAuthHeaders()).toEqual({ Authorization: 'Bearer my-token' })
    })

    it('returns empty object when no token', () => {
      mockToken.value = null
      const { getAuthHeaders } = useAuth()
      expect(getAuthHeaders()).toEqual({})
    })
  })

  describe('fetchMe', () => {
    it('fetches and updates user data', async () => {
      mockToken.value = 'valid-token'
      const meResponse = {
        userId: 'u1', email: 'me@test.com', fullName: 'Me', role: 'Parent',
        children: [{ playerId: 'p1', firstName: 'Kid', lastName: 'One', relationship: 'Father' }]
      }
      vi.mocked($fetch).mockResolvedValue(meResponse)

      const { fetchMe } = useAuth()
      await fetchMe()

      expect($fetch).toHaveBeenCalledWith('/api/auth/me', {
        headers: { Authorization: 'Bearer valid-token' }
      })
      expect(mockUser.value.children).toHaveLength(1)
    })

    it('does nothing when no token', async () => {
      mockToken.value = null
      const { fetchMe } = useAuth()
      await fetchMe()
      expect($fetch).not.toHaveBeenCalled()
    })

    it('calls logout on fetch error', async () => {
      mockToken.value = 'expired-token'
      vi.mocked($fetch).mockRejectedValue(new Error('401'))

      const { fetchMe } = useAuth()
      await fetchMe()

      expect(mockToken.value).toBeNull()
      expect(navigateTo).toHaveBeenCalledWith('/login')
    })
  })
})
