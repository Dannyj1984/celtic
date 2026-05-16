import { describe, it, expect, vi, beforeEach } from 'vitest'
import { ref } from 'vue'

vi.mock('~/composables/useAuth', () => ({
  useAuth: () => ({
    getAuthHeaders: () => ({ Authorization: 'Bearer test-token' })
  })
}))

vi.stubGlobal('$fetch', vi.fn())

import { useMatches } from '~/composables/useMatches'

describe('useMatches', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('returns expected properties', () => {
    const result = useMatches()
    expect(result).toHaveProperty('matches')
    expect(result).toHaveProperty('loading')
    expect(result).toHaveProperty('error')
    expect(result).toHaveProperty('fetchMatches')
    expect(result).toHaveProperty('createMatch')
    expect(result).toHaveProperty('updateMatch')
    expect(result).toHaveProperty('deleteMatch')
  })

  describe('fetchMatches', () => {
    it('fetches and sets matches', async () => {
      const mockMatches = [
        { id: 'm1', date: '2026-05-01', opposition: 'Rangers', goalsFor: 3, goalsAgainst: 1, result: 'Win', isPublished: true }
      ]
      vi.mocked($fetch).mockResolvedValue(mockMatches)

      const { matches, loading, fetchMatches } = useMatches()
      const promise = fetchMatches()
      expect(loading.value).toBe(true)
      
      await promise
      
      expect(loading.value).toBe(false)
      expect(matches.value).toEqual(mockMatches)
    })

    it('sets error on failure', async () => {
      vi.mocked($fetch).mockRejectedValue(new Error('fail'))

      const { error, fetchMatches } = useMatches()
      await fetchMatches()

      expect(error.value).toBe('Failed to fetch matches')
    })
  })

  describe('createMatch', () => {
    it('creates a match and prepends to list', async () => {
      const newMatch = { id: 'm2', opposition: 'Celtic B', goalsFor: 0, goalsAgainst: 0, result: 'Draw', isPublished: false, date: '2026-06-01' }
      vi.mocked($fetch).mockResolvedValue(newMatch)

      const { matches, createMatch } = useMatches()
      matches.value = [{ id: 'm1', opposition: 'Old', goalsFor: 1, goalsAgainst: 0, result: 'Win', isPublished: true, date: '2026-05-01' }] as any[]

      const result = await createMatch({ opposition: 'Celtic B' })

      expect(result.success).toBe(true)
      expect(matches.value[0].id).toBe('m2') // prepended via unshift
      expect(matches.value).toHaveLength(2)
    })

    it('returns error on failure', async () => {
      vi.mocked($fetch).mockRejectedValue({ data: { message: 'Missing fields' } })

      const { createMatch } = useMatches()
      const result = await createMatch({})

      expect(result.success).toBe(false)
      expect(result.error).toBe('Missing fields')
    })
  })

  describe('updateMatch', () => {
    it('updates match in list', async () => {
      const updated = { id: 'm1', opposition: 'Rangers', goalsFor: 4, goalsAgainst: 0, result: 'Win', isPublished: true, date: '2026-05-01' }
      vi.mocked($fetch).mockResolvedValue(updated)

      const { matches, updateMatch } = useMatches()
      matches.value = [{ id: 'm1', opposition: 'Rangers', goalsFor: 3, goalsAgainst: 1, result: 'Win', isPublished: false, date: '2026-05-01' }] as any[]

      const result = await updateMatch('m1', { goalsFor: 4, goalsAgainst: 0 })

      expect(result.success).toBe(true)
      expect(matches.value[0].goalsFor).toBe(4)
    })
  })

  describe('deleteMatch', () => {
    it('removes match from list', async () => {
      vi.mocked($fetch).mockResolvedValue(undefined)

      const { matches, deleteMatch } = useMatches()
      matches.value = [
        { id: 'm1', opposition: 'A' },
        { id: 'm2', opposition: 'B' }
      ] as any[]

      const result = await deleteMatch('m1')

      expect(result.success).toBe(true)
      expect(matches.value).toHaveLength(1)
      expect(matches.value[0].id).toBe('m2')
      expect($fetch).toHaveBeenCalledWith('/api/matches/m1', {
        method: 'DELETE',
        headers: { Authorization: 'Bearer test-token' }
      })
    })

    it('returns error on failure', async () => {
      vi.mocked($fetch).mockRejectedValue({ data: { message: 'Cannot delete' } })

      const { deleteMatch } = useMatches()
      const result = await deleteMatch('m1')

      expect(result.success).toBe(false)
      expect(result.error).toBe('Cannot delete')
    })
  })
})
