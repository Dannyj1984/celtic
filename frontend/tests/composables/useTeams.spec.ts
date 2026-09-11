import { describe, it, expect, vi, beforeEach } from 'vitest'
import { ref } from 'vue'
import { useTeams } from '~/composables/useTeams'

const mockAuthHeaders = { Authorization: 'Bearer test-token' }
vi.mock('~/composables/useAuth', () => ({
  useAuth: () => ({
    getAuthHeaders: () => mockAuthHeaders,
  }),
}))

const mockFetch = vi.fn()
vi.stubGlobal('$fetch', mockFetch)

describe('useTeams', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('fetches teams successfully', async () => {
    const mockTeams = [
      { id: 't1', name: 'U7 Whites', colorHex: '#ffffff', isActive: true, playersCount: 8 },
      { id: 't2', name: 'U7 Blues', colorHex: '#0000ff', isActive: true, playersCount: 7 },
    ]
    mockFetch.mockResolvedValueOnce(mockTeams)

    const { teams, loading, error, fetchTeams } = useTeams()

    await fetchTeams()

    expect(mockFetch).toHaveBeenCalledWith('/api/teams', { headers: mockAuthHeaders })
    expect(teams.value).toEqual(mockTeams)
    expect(loading.value).toBe(false)
    expect(error.value).toBeNull()
  })

  it('handles error when fetching teams fails', async () => {
    mockFetch.mockRejectedValueOnce({ data: { message: 'Server error' } })

    const { teams, loading, error, fetchTeams } = useTeams()

    await fetchTeams()

    expect(teams.value).toEqual([])
    expect(loading.value).toBe(false)
    expect(error.value).toBe('Server error')
  })

  it('creates a new team and appends it to state', async () => {
    const newTeam = { id: 't3', name: 'U7 Hoops', colorHex: '#008000', isActive: true, playersCount: 0 }
    mockFetch.mockResolvedValueOnce(newTeam)

    const { teams, createTeam } = useTeams()

    const res = await createTeam({ name: 'U7 Hoops', colorHex: '#008000' })

    expect(res.success).toBe(true)
    expect(res.team).toEqual(newTeam)
    expect(teams.value).toContainEqual(newTeam)
  })

  it('updates an existing team in state', async () => {
    const initialTeams = [{ id: 't1', name: 'U7 Whites', colorHex: '#ffffff', isActive: true, playersCount: 8 }]
    mockFetch.mockResolvedValueOnce(initialTeams)

    const { teams, fetchTeams, updateTeam } = useTeams()
    await fetchTeams()

    const updatedTeam = { id: 't1', name: 'U7 First Team', colorHex: '#ffffff', isActive: true, playersCount: 8 }
    mockFetch.mockResolvedValueOnce(updatedTeam)

    const res = await updateTeam('t1', { name: 'U7 First Team', colorHex: '#ffffff', isActive: true })

    expect(res.success).toBe(true)
    expect(teams.value[0].name).toBe('U7 First Team')
  })

  it('deletes a team and removes it from state', async () => {
    const initialTeams = [{ id: 't1', name: 'U7 Whites', colorHex: '#ffffff', isActive: true, playersCount: 8 }]
    mockFetch.mockResolvedValueOnce(initialTeams)

    const { teams, fetchTeams, deleteTeam } = useTeams()
    await fetchTeams()

    mockFetch.mockResolvedValueOnce({})

    const res = await deleteTeam('t1')

    expect(res.success).toBe(true)
    expect(teams.value).toHaveLength(0)
  })
})
