import { ref } from 'vue'
import { useAuth } from './useAuth'

export interface SquadPlayer {
  id: string
  name: string
  position?: string | null
}

export interface SubstitutionInfo {
  playerInId: string
  playerInName: string
  playerOutId: string
  playerOutName: string
}

export interface SquadPeriod {
  periodNumber: number
  half: number
  startMinute: number
  endMinute: number
  goalkeeper: SquadPlayer | null
  outfieldPlayers: SquadPlayer[]
  benchPlayers: SquadPlayer[]
  substitutions: SubstitutionInfo[]
}

export interface PlayerMinutes {
  playerId: string
  playerName: string
  totalMinutes: number
  goalkeeperMinutes: number
  outfieldMinutes: number
  benchMinutes: number
}

export interface MatchSquad {
  id: string
  matchId?: string | null
  eventId?: string | null
  halfDurationMinutes: number
  format?: string
  totalPeriods: number
  periodDurationMinutes: number
  firstHalfGoalkeeperPlayerId?: string | null
  firstHalfGoalkeeperName?: string | null
  secondHalfGoalkeeperPlayerId?: string | null
  secondHalfGoalkeeperName?: string | null
  registeredPlayers: SquadPlayer[]
  periods: SquadPeriod[]
  playerMinutes: PlayerMinutes[]
  updatedAt: string
}

export function computePlayerMinutes(registeredPlayers: SquadPlayer[], periods: SquadPeriod[], defaultPeriodMinutes: number = 6): PlayerMinutes[] {
  return registeredPlayers.map(player => {
    let gkMin = 0
    let outfieldMin = 0
    let benchMin = 0

    for (const period of periods) {
      const dur = period.endMinute > period.startMinute 
        ? (period.endMinute - period.startMinute) 
        : defaultPeriodMinutes

      if (period.goalkeeper?.id === player.id) {
        gkMin += dur
      } else if (period.outfieldPlayers.some(o => o.id === player.id)) {
        outfieldMin += dur
      } else {
        benchMin += dur
      }
    }

    return {
      playerId: player.id,
      playerName: player.name,
      totalMinutes: gkMin + outfieldMin,
      goalkeeperMinutes: gkMin,
      outfieldMinutes: outfieldMin,
      benchMinutes: benchMin,
    }
  }).sort((a, b) => b.totalMinutes - a.totalMinutes || a.playerName.localeCompare(b.playerName))
}

export function recomputePeriodSubstitutions(periods: SquadPeriod[]): SquadPeriod[] {
  const updatedPeriods = [...periods]
  for (let i = 1; i < updatedPeriods.length; i++) {
    const prevPeriod = updatedPeriods[i - 1]
    const currPeriod = updatedPeriods[i]

    if (!prevPeriod || !currPeriod) continue

    const prevPitch = new Set<string>()
    if (prevPeriod.goalkeeper) prevPitch.add(prevPeriod.goalkeeper.id)
    prevPeriod.outfieldPlayers.forEach(p => prevPitch.add(p.id))

    const currPitch = new Set<string>()
    if (currPeriod.goalkeeper) currPitch.add(currPeriod.goalkeeper.id)
    currPeriod.outfieldPlayers.forEach(p => currPitch.add(p.id))

    const currPitchList = [
      ...(currPeriod.goalkeeper ? [currPeriod.goalkeeper] : []),
      ...currPeriod.outfieldPlayers
    ]
    const prevPitchList = [
      ...(prevPeriod.goalkeeper ? [prevPeriod.goalkeeper] : []),
      ...prevPeriod.outfieldPlayers
    ]

    const comingOn = currPitchList.filter(p => !prevPitch.has(p.id))
    const goingOff = prevPitchList.filter(p => !currPitch.has(p.id))

    const subs: SubstitutionInfo[] = []
    const subCount = Math.min(comingOn.length, goingOff.length)
    for (let s = 0; s < subCount; s++) {
      const pIn = comingOn[s]
      const pOut = goingOff[s]
      if (pIn && pOut) {
        subs.push({
          playerInId: pIn.id,
          playerInName: pIn.name,
          playerOutId: pOut.id,
          playerOutName: pOut.name,
        })
      }
    }

    updatedPeriods[i] = {
      ...currPeriod,
      substitutions: subs,
    }
  }
  return updatedPeriods
}

export function useMatchSquad() {
  const { getAuthHeaders } = useAuth()
  const squad = ref<MatchSquad | null>(null)
  const loading = ref(false)
  const saving = ref(false)
  const error = ref<string | null>(null)

  async function fetchSquad(matchId?: string, eventId?: string) {
    loading.value = true
    error.value = null
    try {
      const url = matchId 
        ? `/api/matches/${matchId}/squad` 
        : `/api/events/${eventId}/squad`
      
      const data = await $fetch<MatchSquad>(url, {
        headers: getAuthHeaders(),
      })
      squad.value = data
      return { success: true, squad: data }
    } catch (err: any) {
      if (err?.statusCode === 404) {
        squad.value = null
        return { success: false, notFound: true }
      }
      error.value = err?.data?.message || 'Failed to fetch squad'
      return { success: false, error: error.value }
    } finally {
      loading.value = false
    }
  }

  async function generateSquad(params: {
    matchId?: string
    eventId?: string
    gk1Id?: string
    gk2Id?: string
    halfDurationMinutes?: number
    format?: string
    totalPeriods?: number
    periodMinutes?: number
    customPlayerIds?: string[]
  }) {
    loading.value = true
    error.value = null
    try {
      const url = params.matchId 
        ? `/api/matches/${params.matchId}/squad/generate` 
        : `/api/events/${params.eventId}/squad/generate`

      const payload = {
        matchId: params.matchId,
        eventId: params.eventId,
        firstHalfGoalkeeperPlayerId: params.gk1Id || null,
        secondHalfGoalkeeperPlayerId: params.gk2Id || null,
        halfDurationMinutes: params.halfDurationMinutes || null,
        format: params.format || null,
        totalPeriods: params.totalPeriods || null,
        periodDurationMinutes: params.periodMinutes || null,
        customPlayerIds: params.customPlayerIds || null,
      }

      const generated = await $fetch<MatchSquad>(url, {
        method: 'POST',
        headers: getAuthHeaders(),
        body: payload,
      })
      squad.value = generated
      return { success: true, squad: generated }
    } catch (err: any) {
      error.value = err?.data?.message || 'Failed to generate squad rotation'
      return { success: false, error: error.value }
    } finally {
      loading.value = false
    }
  }

  async function saveSquad(matchId?: string, eventId?: string) {
    if (!squad.value) return { success: false, error: 'No squad to save' }
    saving.value = true
    error.value = null
    try {
      const url = matchId 
        ? `/api/matches/${matchId}/squad` 
        : `/api/events/${eventId}/squad`

      const payload = {
        matchId,
        eventId,
        halfDurationMinutes: squad.value.halfDurationMinutes,
        format: squad.value.format || '5v5',
        totalPeriods: squad.value.totalPeriods,
        periodDurationMinutes: squad.value.periodDurationMinutes,
        firstHalfGoalkeeperPlayerId: squad.value.firstHalfGoalkeeperPlayerId,
        secondHalfGoalkeeperPlayerId: squad.value.secondHalfGoalkeeperPlayerId,
        periods: squad.value.periods,
        registeredPlayers: squad.value.registeredPlayers,
      }

      const saved = await $fetch<MatchSquad>(url, {
        method: 'PUT',
        headers: getAuthHeaders(),
        body: payload,
      })
      squad.value = saved
      return { success: true, squad: saved }
    } catch (err: any) {
      error.value = err?.data?.message || 'Failed to save squad'
      return { success: false, error: error.value }
    } finally {
      saving.value = false
    }
  }

  function swapPlayers(periodIndex: number, playerAId: string, playerBId: string) {
    if (!squad.value || !squad.value.periods[periodIndex]) return

    const period = squad.value.periods[periodIndex]
    if (!period) return

    let gk = period.goalkeeper
    let outfield = [...period.outfieldPlayers]
    let bench = [...period.benchPlayers]

    const allInPeriod = [
      ...(gk ? [gk] : []),
      ...outfield,
      ...bench,
    ]

    const pA = allInPeriod.find(p => p.id === playerAId)
    const pB = allInPeriod.find(p => p.id === playerBId)

    if (!pA || !pB) return

    // Swap roles
    const isAGk = gk?.id === playerAId
    const isBGk = gk?.id === playerBId
    const isAOutfield = outfield.some(p => p.id === playerAId)
    const isBOutfield = outfield.some(p => p.id === playerBId)
    const isABench = bench.some(p => p.id === playerAId)
    const isBBench = bench.some(p => p.id === playerBId)

    if (isAGk && isBOutfield) {
      gk = pB
      outfield = outfield.map(p => p.id === playerBId ? pA : p)
    } else if (isAGk && isBBench) {
      gk = pB
      bench = bench.map(p => p.id === playerBId ? pA : p)
    } else if (isBGk && isAOutfield) {
      gk = pA
      outfield = outfield.map(p => p.id === playerAId ? pB : p)
    } else if (isBGk && isABench) {
      gk = pA
      bench = bench.map(p => p.id === playerAId ? pB : p)
    } else if (isAOutfield && isBBench) {
      outfield = outfield.map(p => p.id === playerAId ? pB : p)
      bench = bench.map(p => p.id === playerBId ? pA : p)
    } else if (isBOutfield && isABench) {
      outfield = outfield.map(p => p.id === playerBId ? pA : p)
      bench = bench.map(p => p.id === playerAId ? pB : p)
    }

    const updatedPeriod: SquadPeriod = {
      ...period,
      goalkeeper: gk,
      outfieldPlayers: outfield,
      benchPlayers: bench,
    }

    const newPeriods = [...squad.value.periods]
    newPeriods[periodIndex] = updatedPeriod

    const recalculatedPeriods = recomputePeriodSubstitutions(newPeriods)
    const recalculatedMinutes = computePlayerMinutes(
      squad.value.registeredPlayers,
      recalculatedPeriods,
      squad.value.periodDurationMinutes
    )

    squad.value = {
      ...squad.value,
      periods: recalculatedPeriods,
      playerMinutes: recalculatedMinutes,
    }
  }

  return {
    squad,
    loading,
    saving,
    error,
    fetchSquad,
    generateSquad,
    saveSquad,
    swapPlayers,
  }
}
