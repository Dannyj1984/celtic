import { describe, it, expect, vi, beforeEach } from 'vitest'
import { ref } from 'vue'

const mockIsAuthenticated = ref(false)

vi.stubGlobal('useAuth', () => ({
  isAuthenticated: mockIsAuthenticated
}))

vi.stubGlobal('navigateTo', vi.fn())

// defineNuxtRouteMiddleware just returns the handler function
vi.stubGlobal('defineNuxtRouteMiddleware', (handler: any) => handler)

// Now import - the global stubs are set before this import is resolved
const { default: authMiddleware } = await import('~/middleware/auth.global')

describe('auth.global middleware', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    mockIsAuthenticated.value = false
  })

  it('redirects to /login when not authenticated on protected route', () => {
    authMiddleware({ path: '/dashboard' } as any)
    expect(navigateTo).toHaveBeenCalledWith('/login')
  })

  it('allows authenticated users to access protected routes', () => {
    mockIsAuthenticated.value = true
    const result = authMiddleware({ path: '/dashboard' } as any)
    expect(navigateTo).not.toHaveBeenCalled()
    expect(result).toBeUndefined()
  })

  it('allows unauthenticated users to access /login', () => {
    const result = authMiddleware({ path: '/login' } as any)
    expect(navigateTo).not.toHaveBeenCalled()
    expect(result).toBeUndefined()
  })

  it('redirects authenticated users away from /login to /dashboard', () => {
    mockIsAuthenticated.value = true
    authMiddleware({ path: '/login' } as any)
    expect(navigateTo).toHaveBeenCalledWith('/dashboard')
  })

  it('redirects to login for admin routes when not authenticated', () => {
    authMiddleware({ path: '/admin/players' } as any)
    expect(navigateTo).toHaveBeenCalledWith('/login')
  })
})
