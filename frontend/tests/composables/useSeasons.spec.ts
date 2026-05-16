import { describe, it, expect, vi, beforeEach } from 'vitest'
import { ref } from 'vue'

vi.mock('~/composables/useAuth', () => ({
  useAuth: () => ({
    getAuthHeaders: () => ({ Authorization: 'Bearer test-token' })
  })
}))

vi.stubGlobal('$fetch', vi.fn())

import { useSeasons } from '~/composables/useSeasons'

describe('useSeasons', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('returns expected properties', () => {
    const result = useSeasons()
    expect(result).toHaveProperty('seasons')
    expect(result).toHaveProperty('loading')
    expect(result).toHaveProperty('error')
    expect(result).toHaveProperty('fetchSeasons')
    expect(result).toHaveProperty('createSeason')
    expect(result).toHaveProperty('updateSeason')
  })

  describe('fetchSeasons', () => {
    it('fetches and sets seasons', async () => {
      const mockSeasons = [
        { id: 's1', name: '2025-26', startDate: '2025-09-01', endDate: '2026-06-30', subAmount: 25, subFrequency: 'Monthly', isCurrent: true }
      ]
      vi.mocked($fetch).mockResolvedValue(mockSeasons)

      const { seasons, loading, fetchSeasons } = useSeasons()
      const promise = fetchSeasons()
      expect(loading.value).toBe(true)

      await promise

      expect(loading.value).toBe(false)
      expect(seasons.value).toEqual(mockSeasons)
    })

    it('sets error on failure', async () => {
      vi.mocked($fetch).mockRejectedValue(new Error('fail'))

      const { error, fetchSeasons } = useSeasons()
      await fetchSeasons()

      expect(error.value).toBe('Failed to fetch seasons')
    })
  })

  describe('createSeason', () => {
    it('creates season and prepends to list', async () => {
      const newSeason = { id: 's2', name: '2026-27', isCurrent: false, startDate: '2026-09-01', endDate: '2027-06-30', subAmount: 30, subFrequency: 'Monthly' }
      vi.mocked($fetch).mockResolvedValue(newSeason)

      const { seasons, createSeason } = useSeasons()
      const result = await createSeason({ name: '2026-27' })

      expect(result.success).toBe(true)
      expect(seasons.value[0].name).toBe('2026-27')
    })

    it('when new season is current, unsets other current seasons locally', async () => {
      const newSeason = { id: 's2', name: '2026-27', isCurrent: true, startDate: '2026-09-01', endDate: '2027-06-30', subAmount: 30, subFrequency: 'Monthly' }
      vi.mocked($fetch).mockResolvedValue(newSeason)

      const { seasons, createSeason } = useSeasons()
      seasons.value = [{ id: 's1', name: '2025-26', isCurrent: true, startDate: '2025-09-01', endDate: '2026-06-30', subAmount: 25, subFrequency: 'Monthly' }]

      await createSeason({ name: '2026-27', isCurrent: true })

      // Old season should be set to false locally
      const oldSeason = seasons.value.find(s => s.id === 's1')
      expect(oldSeason?.isCurrent).toBe(false)
    })

    it('returns error on failure', async () => {
      vi.mocked($fetch).mockRejectedValue({ data: { message: 'Duplicate name' } })

      const { createSeason } = useSeasons()
      const result = await createSeason({ name: 'dup' })

      expect(result.success).toBe(false)
      expect(result.error).toBe('Duplicate name')
    })
  })

  describe('updateSeason', () => {
    it('updates season in list', async () => {
      const updated = { id: 's1', name: 'Updated', isCurrent: false, startDate: '2025-09-01', endDate: '2026-06-30', subAmount: 30, subFrequency: 'Monthly' }
      vi.mocked($fetch).mockResolvedValue(updated)

      const { seasons, updateSeason } = useSeasons()
      seasons.value = [{ id: 's1', name: '2025-26', isCurrent: true, startDate: '2025-09-01', endDate: '2026-06-30', subAmount: 25, subFrequency: 'Monthly' }]

      const result = await updateSeason('s1', { name: 'Updated' })

      expect(result.success).toBe(true)
      expect(seasons.value[0].name).toBe('Updated')
    })

    it('when updated season is current, unsets others', async () => {
      const updated = { id: 's2', name: 'New Current', isCurrent: true, startDate: '2026-09-01', endDate: '2027-06-30', subAmount: 30, subFrequency: 'Monthly' }
      vi.mocked($fetch).mockResolvedValue(updated)

      const { seasons, updateSeason } = useSeasons()
      seasons.value = [
        { id: 's1', name: 'Old', isCurrent: true, startDate: '2025-09-01', endDate: '2026-06-30', subAmount: 25, subFrequency: 'Monthly' },
        { id: 's2', name: 'New', isCurrent: false, startDate: '2026-09-01', endDate: '2027-06-30', subAmount: 30, subFrequency: 'Monthly' }
      ]

      await updateSeason('s2', { isCurrent: true })

      expect(seasons.value.find(s => s.id === 's1')?.isCurrent).toBe(false)
      expect(seasons.value.find(s => s.id === 's2')?.isCurrent).toBe(true)
    })
  })
})
