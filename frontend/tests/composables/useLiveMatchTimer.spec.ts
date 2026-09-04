import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { ref } from 'vue'
import { useLiveMatchTimer } from '~/composables/useLiveMatchTimer'
import type { MatchSquad } from '~/composables/useMatchSquad'

describe('useLiveMatchTimer', () => {
  let squadRef: { value: MatchSquad | null }

  beforeEach(() => {
    vi.useFakeTimers()
    squadRef = ref<MatchSquad | null>({
      id: 'squad-1',
      halfDurationMinutes: 25,
      totalPeriods: 8,
      periodDurationMinutes: 6,
      registeredPlayers: [
        { id: 'p1', name: 'Player 1' },
        { id: 'p2', name: 'Player 2' },
        { id: 'p3', name: 'Player 3' },
        { id: 'p4', name: 'Player 4' },
        { id: 'p5', name: 'Player 5' },
        { id: 'p6', name: 'Player 6' },
      ],
      periods: [
        {
          periodNumber: 1,
          half: 1,
          startMinute: 0,
          endMinute: 6, // 6m
          goalkeeper: { id: 'p1', name: 'Player 1' },
          outfieldPlayers: [
            { id: 'p2', name: 'Player 2' },
            { id: 'p3', name: 'Player 3' },
            { id: 'p4', name: 'Player 4' },
            { id: 'p5', name: 'Player 5' },
          ],
          benchPlayers: [{ id: 'p6', name: 'Player 6' }],
          substitutions: [],
        },
        {
          periodNumber: 2,
          half: 1,
          startMinute: 6,
          endMinute: 12, // 6m
          goalkeeper: { id: 'p1', name: 'Player 1' },
          outfieldPlayers: [
            { id: 'p6', name: 'Player 6' },
            { id: 'p3', name: 'Player 3' },
            { id: 'p4', name: 'Player 4' },
            { id: 'p5', name: 'Player 5' },
          ],
          benchPlayers: [{ id: 'p2', name: 'Player 2' }],
          substitutions: [
            { playerInId: 'p6', playerInName: 'Player 6', playerOutId: 'p2', playerOutName: 'Player 2' },
          ],
        },
        {
          periodNumber: 3,
          half: 1,
          startMinute: 12,
          endMinute: 18, // 6m
          goalkeeper: { id: 'p1', name: 'Player 1' },
          outfieldPlayers: [],
          benchPlayers: [],
          substitutions: [],
        },
        {
          periodNumber: 4,
          half: 1,
          startMinute: 18,
          endMinute: 25, // 7m (final slot)
          goalkeeper: { id: 'p1', name: 'Player 1' },
          outfieldPlayers: [],
          benchPlayers: [],
          substitutions: [],
        },
      ],
      playerMinutes: [],
      updatedAt: '2026-09-04T10:00:00Z',
    })
  })

  afterEach(() => {
    vi.useRealTimers()
  })

  it('initializes with correct timer values for Period 1', () => {
    const timer = useLiveMatchTimer(squadRef)
    timer.initTimerForCurrentPeriod()

    expect(timer.currentPeriodIndex.value).toBe(0)
    expect(timer.intervalSecondsRemaining.value).toBe(360) // 6 * 60 = 360
    expect(timer.formattedIntervalTimer.value).toBe('06:00')
    expect(timer.formattedTotalElapsed.value).toBe('00:00')
    expect(timer.isRunning.value).toBe(false)
    expect(timer.showSubAlert.value).toBe(false)
  })

  it('runs timer on startTimer and increments totalElapsed while decrementing interval', () => {
    const timer = useLiveMatchTimer(squadRef)
    timer.initTimerForCurrentPeriod()

    timer.startTimer()
    expect(timer.isRunning.value).toBe(true)

    // Advance 5 seconds
    vi.advanceTimersByTime(5000)

    expect(timer.intervalSecondsRemaining.value).toBe(355)
    expect(timer.totalElapsedSeconds.value).toBe(5)
    expect(timer.formattedIntervalTimer.value).toBe('05:55')
    expect(timer.formattedTotalElapsed.value).toBe('00:05')

    timer.pauseTimer()
    expect(timer.isRunning.value).toBe(false)
  })

  it('triggers showSubAlert and pauses timer when interval hits 0', () => {
    const timer = useLiveMatchTimer(squadRef)
    timer.initTimerForCurrentPeriod()

    timer.startTimer()

    // Advance full 360 seconds (6 mins)
    vi.advanceTimersByTime(360000)

    expect(timer.intervalSecondsRemaining.value).toBe(0)
    expect(timer.totalElapsedSeconds.value).toBe(360)
    expect(timer.isRunning.value).toBe(false)
    expect(timer.showSubAlert.value).toBe(true)
  })

  it('adds 60 seconds with addSeconds', () => {
    const timer = useLiveMatchTimer(squadRef)
    timer.intervalSecondsRemaining.value = 10
    timer.addSeconds(60)

    expect(timer.intervalSecondsRemaining.value).toBe(70)
    expect(timer.formattedIntervalTimer.value).toBe('01:10')
  })

  it('confirms substitutions and advances to next period with correct duration', () => {
    const timer = useLiveMatchTimer(squadRef)
    timer.initTimerForCurrentPeriod()
    timer.startTimer()

    // Advance to end of Period 1
    vi.advanceTimersByTime(360000)
    expect(timer.showSubAlert.value).toBe(true)

    // Confirm substitutions
    timer.confirmSubstitutions()

    expect(timer.currentPeriodIndex.value).toBe(1) // Period 2
    expect(timer.showSubAlert.value).toBe(false)
    expect(timer.intervalSecondsRemaining.value).toBe(360)
    expect(timer.isRunning.value).toBe(true) // auto starts
  })

  it('handles 7-minute duration on the final slot of the half (Period 4)', () => {
    const timer = useLiveMatchTimer(squadRef)
    timer.goToPeriod(3) // Period 4 (18' - 25' = 7 mins)

    expect(timer.currentPeriodDurationSeconds.value).toBe(420) // 7 * 60 = 420
    expect(timer.intervalSecondsRemaining.value).toBe(420)
    expect(timer.formattedIntervalTimer.value).toBe('07:00')
  })

  it('resets match state cleanly', () => {
    const timer = useLiveMatchTimer(squadRef)
    timer.initTimerForCurrentPeriod()
    timer.startTimer()
    vi.advanceTimersByTime(120000)

    timer.resetMatch()

    expect(timer.currentPeriodIndex.value).toBe(0)
    expect(timer.totalElapsedSeconds.value).toBe(0)
    expect(timer.isRunning.value).toBe(false)
    expect(timer.intervalSecondsRemaining.value).toBe(360)
    expect(timer.showSubAlert.value).toBe(false)
  })
})
