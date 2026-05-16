import { describe, it, expect, vi, beforeEach } from 'vitest'
import { ref } from 'vue'

vi.mock('~/composables/useAuth', () => ({
  useAuth: () => ({
    getAuthHeaders: () => ({ Authorization: 'Bearer test-token' })
  })
}))

vi.stubGlobal('$fetch', vi.fn())

import { useParents } from '~/composables/useParents'

describe('useParents', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('returns expected properties', () => {
    const result = useParents()
    expect(result).toHaveProperty('parents')
    expect(result).toHaveProperty('loading')
    expect(result).toHaveProperty('error')
    expect(result).toHaveProperty('fetchParents')
    expect(result).toHaveProperty('linkPlayer')
  })

  describe('fetchParents', () => {
    it('fetches and sets parents list', async () => {
      const mockParents = [
        { userId: 'u1', email: 'parent@test.com', fullName: 'Jane Doe', phone: '07000', role: 'Parent', children: [] }
      ]
      vi.mocked($fetch).mockResolvedValue(mockParents)

      const { parents, loading, fetchParents } = useParents()
      const promise = fetchParents()
      expect(loading.value).toBe(true)
      
      await promise
      
      expect(loading.value).toBe(false)
      expect(parents.value).toEqual(mockParents)
      expect($fetch).toHaveBeenCalledWith('/api/auth/parents', {
        headers: { Authorization: 'Bearer test-token' }
      })
    })

    it('sets error on failure', async () => {
      vi.mocked($fetch).mockRejectedValue({ data: { message: 'Forbidden' } })

      const { error, fetchParents } = useParents()
      await fetchParents()

      expect(error.value).toBe('Forbidden')
    })
  })

  describe('linkPlayer', () => {
    it('links a player and re-fetches parents', async () => {
      // First call is linkPlayer POST, second is fetchParents GET
      vi.mocked($fetch)
        .mockResolvedValueOnce(undefined) // POST link-player
        .mockResolvedValueOnce([]) // GET parents refresh

      const { linkPlayer } = useParents()
      const result = await linkPlayer('user-1', 'player-1', 'Father')

      expect(result.success).toBe(true)
      expect($fetch).toHaveBeenCalledWith('/api/auth/link-player', {
        method: 'POST',
        headers: { Authorization: 'Bearer test-token' },
        body: { userId: 'user-1', playerId: 'player-1', relationship: 'Father' }
      })
    })

    it('returns error on failure', async () => {
      vi.mocked($fetch).mockRejectedValue({ data: { message: 'Already linked' } })

      const { linkPlayer } = useParents()
      const result = await linkPlayer('u1', 'p1', 'Mother')

      expect(result.success).toBe(false)
      expect(result.error).toBe('Already linked')
    })
  })
})
