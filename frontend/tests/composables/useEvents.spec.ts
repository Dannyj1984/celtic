import { describe, it, expect, vi, beforeEach } from 'vitest'
import { ref } from 'vue'

vi.mock('~/composables/useAuth', () => ({
  useAuth: () => ({
    getAuthHeaders: () => ({ Authorization: 'Bearer test-token' })
  })
}))

vi.stubGlobal('$fetch', vi.fn())

import { useEvents } from '~/composables/useEvents'

describe('useEvents', () => {
  beforeEach(() => { vi.clearAllMocks() })

  it('returns expected properties', () => {
    const r = useEvents()
    expect(r).toHaveProperty('events')
    expect(r).toHaveProperty('fetchEvents')
    expect(r).toHaveProperty('createEvent')
    expect(r).toHaveProperty('updateEvent')
    expect(r).toHaveProperty('deleteEvent')
  })

  describe('fetchEvents', () => {
    it('fetches and sets events', async () => {
      const mock = [{ id: 'e1', type: 'Training', dateTime: '2026-05-12T18:00:00Z', location: 'Pitch 1', isCancelled: false, attendingPlayers: [] }]
      vi.mocked($fetch).mockResolvedValue(mock)
      const { events, loading, fetchEvents } = useEvents()
      const p = fetchEvents()
      expect(loading.value).toBe(true)
      await p
      expect(loading.value).toBe(false)
      expect(events.value).toEqual(mock)
    })

    it('sets error on failure', async () => {
      vi.mocked($fetch).mockRejectedValue(new Error('fail'))
      const { error, fetchEvents } = useEvents()
      await fetchEvents()
      expect(error.value).toBe('Failed to fetch events')
    })
  })

  describe('createEvent', () => {
    it('creates event and appends', async () => {
      const newEvent = { id: 'e2', type: 'Training', dateTime: '2026-05-19T18:00:00Z', location: 'Pitch 2', isCancelled: false, attendingPlayers: [] }
      vi.mocked($fetch).mockResolvedValue(newEvent)
      const { events, createEvent } = useEvents()
      const result = await createEvent({ type: 'Training' })
      expect(result.success).toBe(true)
      expect(events.value).toContainEqual(newEvent)
    })
  })

  describe('deleteEvent', () => {
    it('removes event from list', async () => {
      vi.mocked($fetch).mockResolvedValue(undefined)
      const { events, deleteEvent } = useEvents()
      events.value = [{ id: 'e1', type: 'Training', dateTime: 'd', location: 'A', isCancelled: false, attendingPlayers: [] }, { id: 'e2', type: 'Match', dateTime: 'd', location: 'B', isCancelled: false, attendingPlayers: [] }]
      const result = await deleteEvent('e1')
      expect(result.success).toBe(true)
      expect(events.value).toHaveLength(1)
      expect(events.value[0].id).toBe('e2')
    })

    it('returns error on failure', async () => {
      vi.mocked($fetch).mockRejectedValue({ data: { message: 'Not found' } })
      const { deleteEvent } = useEvents()
      const result = await deleteEvent('x')
      expect(result.success).toBe(false)
      expect(result.error).toBe('Not found')
    })
  })
})
