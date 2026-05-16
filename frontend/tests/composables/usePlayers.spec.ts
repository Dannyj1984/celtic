import { describe, it, expect, vi, beforeEach } from 'vitest'
import { ref } from 'vue'

// Mock useAuth before importing usePlayers
vi.mock('~/composables/useAuth', () => ({
  useAuth: () => ({
    getAuthHeaders: () => ({ Authorization: 'Bearer test-token' })
  })
}))

vi.stubGlobal('$fetch', vi.fn())

import { usePlayers } from '~/composables/usePlayers'

describe('usePlayers', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('returns expected properties', () => {
    const result = usePlayers()
    expect(result).toHaveProperty('players')
    expect(result).toHaveProperty('loading')
    expect(result).toHaveProperty('error')
    expect(result).toHaveProperty('fetchPlayers')
    expect(result).toHaveProperty('createPlayer')
    expect(result).toHaveProperty('updatePlayer')
  })

  describe('fetchPlayers', () => {
    it('fetches players and sets them', async () => {
      const mockPlayers = [
        { id: '1', firstName: 'John', lastName: 'Terry', isActive: true, subscriptionStatus: 'Active' }
      ]
      vi.mocked($fetch).mockResolvedValue(mockPlayers)

      const { players, loading, error, fetchPlayers } = usePlayers()
      
      expect(loading.value).toBe(false)
      const fetchPromise = fetchPlayers()
      expect(loading.value).toBe(true)
      
      await fetchPromise
      
      expect(loading.value).toBe(false)
      expect(error.value).toBeNull()
      expect(players.value).toEqual(mockPlayers)
      expect($fetch).toHaveBeenCalledWith('/api/players', {
        headers: { Authorization: 'Bearer test-token' }
      })
    })

    it('sets error on failure', async () => {
      vi.mocked($fetch).mockRejectedValue({ data: { message: 'Server error' } })

      const { error, fetchPlayers } = usePlayers()
      await fetchPlayers()

      expect(error.value).toBe('Server error')
    })

    it('uses default error message when server provides none', async () => {
      vi.mocked($fetch).mockRejectedValue(new Error('Network'))

      const { error, fetchPlayers } = usePlayers()
      await fetchPlayers()

      expect(error.value).toBe('Failed to fetch players')
    })
  })

  describe('createPlayer', () => {
    it('creates a player and adds to list', async () => {
      const newPlayer = { id: '2', firstName: 'Frank', lastName: 'Lampard', isActive: true, subscriptionStatus: 'Active' }
      vi.mocked($fetch).mockResolvedValue(newPlayer)

      const { players, createPlayer } = usePlayers()
      const result = await createPlayer({ firstName: 'Frank', lastName: 'Lampard' })

      expect(result.success).toBe(true)
      expect(result.player).toEqual(newPlayer)
      expect(players.value).toContainEqual(newPlayer)
      expect($fetch).toHaveBeenCalledWith('/api/players', {
        method: 'POST',
        headers: { Authorization: 'Bearer test-token' },
        body: { firstName: 'Frank', lastName: 'Lampard' }
      })
    })

    it('returns error on failure', async () => {
      vi.mocked($fetch).mockRejectedValue({ data: { message: 'Validation failed' } })

      const { createPlayer } = usePlayers()
      const result = await createPlayer({ firstName: '' })

      expect(result.success).toBe(false)
      expect(result.error).toBe('Validation failed')
    })
  })

  describe('updatePlayer', () => {
    it('updates an existing player in the list', async () => {
      const updated = { id: '1', firstName: 'John', lastName: 'Updated', isActive: true, subscriptionStatus: 'Active' }
      vi.mocked($fetch).mockResolvedValue(updated)

      const { players, updatePlayer } = usePlayers()
      players.value = [{ id: '1', firstName: 'John', lastName: 'Terry', isActive: true, subscriptionStatus: 'Active' }]

      const result = await updatePlayer('1', { lastName: 'Updated' })

      expect(result.success).toBe(true)
      expect(players.value[0].lastName).toBe('Updated')
      expect($fetch).toHaveBeenCalledWith('/api/players/1', {
        method: 'PUT',
        headers: { Authorization: 'Bearer test-token' },
        body: { lastName: 'Updated' }
      })
    })

    it('returns error on failure', async () => {
      vi.mocked($fetch).mockRejectedValue({ data: { message: 'Not found' } })

      const { updatePlayer } = usePlayers()
      const result = await updatePlayer('999', {})

      expect(result.success).toBe(false)
      expect(result.error).toBe('Not found')
    })
  })
})
