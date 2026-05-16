import { describe, it, expect, vi, beforeEach } from 'vitest'
import { ref } from 'vue'

const mockUser = ref({ userId: 'u1', email: 'test@test.com', fullName: 'Test', role: 'Parent' })

vi.mock('~/composables/useAuth', () => ({
  useAuth: () => ({
    getAuthHeaders: () => ({ Authorization: 'Bearer test-token' }),
    user: mockUser
  })
}))

vi.stubGlobal('useRuntimeConfig', () => ({ public: { vapidPublicKey: 'BEl62iUYgUivxIkv69yViEuiBIa-Ib9-SkvMeAtA3LFgDzkOs-qy0KNkUW' } }))
vi.stubGlobal('$fetch', vi.fn())

// Mock process.client
vi.stubGlobal('process', { client: false })

import { useNotifications } from '~/composables/useNotifications'

describe('useNotifications', () => {
  beforeEach(() => { vi.clearAllMocks() })

  it('returns expected properties', () => {
    const result = useNotifications()
    expect(result).toHaveProperty('isSupported')
    expect(result).toHaveProperty('isSubscribed')
    expect(result).toHaveProperty('loading')
    expect(result).toHaveProperty('checkSubscription')
    expect(result).toHaveProperty('subscribe')
    expect(result).toHaveProperty('unsubscribe')
  })

  it('isSupported is false when not in browser (process.client = false)', () => {
    const { isSupported } = useNotifications()
    expect(isSupported.value).toBe(false)
  })

  it('isSubscribed starts as false', () => {
    const { isSubscribed } = useNotifications()
    expect(isSubscribed.value).toBe(false)
  })

  it('loading starts as false', () => {
    const { loading } = useNotifications()
    expect(loading.value).toBe(false)
  })

  it('checkSubscription does nothing when not supported', async () => {
    const { checkSubscription } = useNotifications()
    const result = await checkSubscription()
    expect(result).toBeUndefined()
  })

  it('subscribe does nothing when not supported', async () => {
    const { subscribe } = useNotifications()
    await subscribe()
    expect($fetch).not.toHaveBeenCalled()
  })

  it('unsubscribe does nothing when not supported', async () => {
    const { unsubscribe } = useNotifications()
    await unsubscribe()
    expect($fetch).not.toHaveBeenCalled()
  })

  it('subscribe does nothing when no user', async () => {
    mockUser.value = null as any
    const { subscribe } = useNotifications()
    await subscribe()
    expect($fetch).not.toHaveBeenCalled()
    mockUser.value = { userId: 'u1', email: 'test@test.com', fullName: 'Test', role: 'Parent' }
  })
})
