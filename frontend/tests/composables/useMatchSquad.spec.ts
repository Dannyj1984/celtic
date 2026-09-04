import { describe, it, expect, vi, beforeEach } from 'vitest'

vi.mock('~/composables/useAuth', () => ({
  useAuth: () => ({
    getAuthHeaders: () => ({ Authorization: 'Bearer test-token' })
  })
}))

vi.stubGlobal('$fetch', vi.fn())

import {
  useMatchSquad,
  computePlayerMinutes,
  recomputePeriodSubstitutions,
  type SquadPlayer,
  type SquadPeriod,
  type MatchSquad,
} from '~/composables/useMatchSquad'

describe('useMatchSquad', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('returns expected properties', () => {
    const result = useMatchSquad()
    expect(result).toHaveProperty('squad')
    expect(result).toHaveProperty('loading')
    expect(result).toHaveProperty('saving')
    expect(result).toHaveProperty('error')
    expect(result).toHaveProperty('fetchSquad')
    expect(result).toHaveProperty('generateSquad')
    expect(result).toHaveProperty('saveSquad')
    expect(result).toHaveProperty('swapPlayers')
  })

  describe('fetchSquad', () => {
    it('fetches existing squad by matchId', async () => {
      const mockSquad: Partial<MatchSquad> = {
        id: 'squad-1',
        matchId: 'match-1',
        totalPeriods: 6,
        periodDurationMinutes: 6,
        registeredPlayers: [],
        periods: [],
        playerMinutes: [],
        updatedAt: '2026-09-04T10:00:00Z',
      }
      vi.mocked($fetch).mockResolvedValue(mockSquad)

      const { squad, loading, fetchSquad } = useMatchSquad()
      const promise = fetchSquad('match-1')
      expect(loading.value).toBe(true)

      const res = await promise
      expect(loading.value).toBe(false)
      expect(res.success).toBe(true)
      expect(squad.value).toEqual(mockSquad)
    })

    it('handles 404 when no squad exists yet', async () => {
      vi.mocked($fetch).mockRejectedValue({ statusCode: 404, data: { message: 'Not found' } })

      const { squad, fetchSquad } = useMatchSquad()
      const res = await fetchSquad('match-1')

      expect(res.success).toBe(false)
      expect(res.notFound).toBe(true)
      expect(squad.value).toBeNull()
    })
  })

  describe('generateSquad', () => {
    it('generates a new squad schedule with halfDurationMinutes and format', async () => {
      const mockGenerated: Partial<MatchSquad> = {
        id: 'squad-2',
        matchId: 'match-1',
        halfDurationMinutes: 25,
        format: '3v3',
        totalPeriods: 10,
        periodDurationMinutes: 5,
        firstHalfGoalkeeperPlayerId: null,
        secondHalfGoalkeeperPlayerId: null,
        registeredPlayers: [
          { id: 'p1', name: 'Player 1' },
          { id: 'p2', name: 'Player 2' },
          { id: 'p3', name: 'Player 3' },
          { id: 'p4', name: 'Player 4' },
          { id: 'p5', name: 'Player 5' },
          { id: 'p6', name: 'Player 6' },
        ],
        periods: [],
        playerMinutes: [],
      }
      vi.mocked($fetch).mockResolvedValue(mockGenerated)

      const { squad, generateSquad } = useMatchSquad()
      const res = await generateSquad({ matchId: 'match-1', format: '3v3', halfDurationMinutes: 25 })

      expect(res.success).toBe(true)
      expect(squad.value).toEqual(mockGenerated)
      expect($fetch).toHaveBeenCalledWith('/api/matches/match-1/squad/generate', expect.objectContaining({
        method: 'POST',
        body: expect.objectContaining({
          halfDurationMinutes: 25,
          format: '3v3',
          firstHalfGoalkeeperPlayerId: null,
          secondHalfGoalkeeperPlayerId: null,
        })
      }))
    })
  })

  describe('saveSquad', () => {
    it('saves squad with halfDurationMinutes and format', async () => {
      const mockSquad: MatchSquad = {
        id: 'squad-1',
        matchId: 'match-1',
        halfDurationMinutes: 15,
        format: '3v3',
        totalPeriods: 6,
        periodDurationMinutes: 5,
        firstHalfGoalkeeperPlayerId: null,
        secondHalfGoalkeeperPlayerId: null,
        registeredPlayers: [],
        periods: [],
        playerMinutes: [],
        updatedAt: '2026-09-04T10:00:00Z',
      }
      vi.mocked($fetch).mockResolvedValue(mockSquad)

      const { squad, saveSquad } = useMatchSquad()
      squad.value = mockSquad

      const res = await saveSquad('match-1')
      expect(res.success).toBe(true)
      expect($fetch).toHaveBeenCalledWith('/api/matches/match-1/squad', expect.objectContaining({
        method: 'PUT',
        body: expect.objectContaining({
          matchId: 'match-1',
          halfDurationMinutes: 15,
          format: '3v3',
          totalPeriods: 6,
          periodDurationMinutes: 5,
        })
      }))
    })
  })

  describe('computePlayerMinutes', () => {
    it('calculates exact playing minutes across GK, outfield, and bench', () => {
      const p1: SquadPlayer = { id: 'p1', name: 'Alice' }
      const p2: SquadPlayer = { id: 'p2', name: 'Bob' }

      const periods: SquadPeriod[] = [
        {
          periodNumber: 1,
          half: 1,
          startMinute: 0,
          endMinute: 6,
          goalkeeper: p1,
          outfieldPlayers: [p2],
          benchPlayers: [],
          substitutions: [],
        },
        {
          periodNumber: 2,
          half: 1,
          startMinute: 6,
          endMinute: 12,
          goalkeeper: p2,
          outfieldPlayers: [],
          benchPlayers: [p1],
          substitutions: [],
        },
      ]

      const minutes = computePlayerMinutes([p1, p2], periods, 6)
      const alice = minutes.find(m => m.playerId === 'p1')
      const bob = minutes.find(m => m.playerId === 'p2')

      expect(alice).toEqual({
        playerId: 'p1',
        playerName: 'Alice',
        totalMinutes: 6,
        goalkeeperMinutes: 6,
        outfieldMinutes: 0,
        benchMinutes: 6,
      })

      expect(bob).toEqual({
        playerId: 'p2',
        playerName: 'Bob',
        totalMinutes: 12,
        goalkeeperMinutes: 6,
        outfieldMinutes: 6,
        benchMinutes: 0,
      })
    })
  })

  describe('recomputePeriodSubstitutions', () => {
    it('diffs on-pitch players between consecutive periods', () => {
      const p1: SquadPlayer = { id: 'p1', name: 'Alice' }
      const p2: SquadPlayer = { id: 'p2', name: 'Bob' }
      const p3: SquadPlayer = { id: 'p3', name: 'Charlie' }

      const periods: SquadPeriod[] = [
        {
          periodNumber: 1,
          half: 1,
          startMinute: 0,
          endMinute: 6,
          goalkeeper: p1,
          outfieldPlayers: [p2],
          benchPlayers: [p3],
          substitutions: [],
        },
        {
          periodNumber: 2,
          half: 1,
          startMinute: 6,
          endMinute: 12,
          goalkeeper: p1,
          outfieldPlayers: [p3], // p3 comes on, p2 goes off
          benchPlayers: [p2],
          substitutions: [],
        },
      ]

      const updated = recomputePeriodSubstitutions(periods)
      expect(updated[1].substitutions).toEqual([
        {
          playerInId: 'p3',
          playerInName: 'Charlie',
          playerOutId: 'p2',
          playerOutName: 'Bob',
        },
      ])
    })
  })

  describe('swapPlayers', () => {
    it('swaps an outfield player with a bench player and recalculates minutes', () => {
      const p1: SquadPlayer = { id: 'p1', name: 'P1' }
      const p2: SquadPlayer = { id: 'p2', name: 'P2' }
      const p3: SquadPlayer = { id: 'p3', name: 'P3' }

      const { squad, swapPlayers } = useMatchSquad()
      squad.value = {
        id: 'sq-1',
        halfDurationMinutes: 18,
        totalPeriods: 2,
        periodDurationMinutes: 6,
        registeredPlayers: [p1, p2, p3],
        periods: [
          {
            periodNumber: 1,
            half: 1,
            startMinute: 0,
            endMinute: 6,
            goalkeeper: p1,
            outfieldPlayers: [p2],
            benchPlayers: [p3],
            substitutions: [],
          },
          {
            periodNumber: 2,
            half: 1,
            startMinute: 6,
            endMinute: 12,
            goalkeeper: p1,
            outfieldPlayers: [p2],
            benchPlayers: [p3],
            substitutions: [],
          },
        ],
        playerMinutes: [],
        updatedAt: '',
      }

      // Swap p2 (outfield) with p3 (bench) in period 0
      swapPlayers(0, 'p2', 'p3')

      expect(squad.value.periods[0].outfieldPlayers).toEqual([p3])
      expect(squad.value.periods[0].benchPlayers).toEqual([p2])
    })
  })
})
