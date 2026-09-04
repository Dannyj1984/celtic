import { ref, computed, onUnmounted } from 'vue'
import type { MatchSquad, SquadPeriod } from './useMatchSquad'

export function useLiveMatchTimer(squadRef: { value: MatchSquad | null }) {
  const isRunning = ref(false)
  const currentPeriodIndex = ref(0)
  const intervalSecondsRemaining = ref(360) // default 6 mins (360s)
  const totalElapsedSeconds = ref(0)
  const showSubAlert = ref(false)
  const hasSoundAlertedForCurrentEnd = ref(false)

  let timerInterval: any = null

  const currentPeriod = computed<SquadPeriod | null>(() => {
    if (!squadRef.value || !squadRef.value.periods[currentPeriodIndex.value]) return null
    return squadRef.value.periods[currentPeriodIndex.value] || null
  })

  const currentPeriodDurationSeconds = computed(() => {
    if (!currentPeriod.value) return 360
    const durMins = currentPeriod.value.endMinute > currentPeriod.value.startMinute
      ? currentPeriod.value.endMinute - currentPeriod.value.startMinute
      : (squadRef.value?.periodDurationMinutes || 6)
    return durMins * 60
  })

  const isLastPeriodOfHalf = computed(() => {
    if (!squadRef.value || !currentPeriod.value) return false
    const totalHalfPeriods = Math.ceil(squadRef.value.periods.length / 2)
    return currentPeriodIndex.value === totalHalfPeriods - 1
  })

  const isFullTime = computed(() => {
    if (!squadRef.value) return false
    return currentPeriodIndex.value >= squadRef.value.periods.length - 1 && intervalSecondsRemaining.value <= 0
  })

  const isHalfTime = computed(() => {
    return isLastPeriodOfHalf.value && intervalSecondsRemaining.value <= 0 && !isFullTime.value
  })

  const formattedIntervalTimer = computed(() => {
    const mins = Math.floor(Math.max(0, intervalSecondsRemaining.value) / 60)
    const secs = Math.max(0, intervalSecondsRemaining.value) % 60
    return `${mins.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`
  })

  const formattedTotalElapsed = computed(() => {
    const mins = Math.floor(totalElapsedSeconds.value / 60)
    const secs = totalElapsedSeconds.value % 60
    return `${mins.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`
  })

  const intervalProgressPercent = computed(() => {
    const total = currentPeriodDurationSeconds.value
    if (total <= 0) return 0
    const elapsed = total - intervalSecondsRemaining.value
    return Math.min(100, Math.max(0, (elapsed / total) * 100))
  })

  function playWhistleSound() {
    try {
      if (typeof window === 'undefined') return
      const AudioContextClass = window.AudioContext || (window as any).webkitAudioContext
      if (!AudioContextClass) return

      const ctx = new AudioContextClass()
      const now = ctx.currentTime

      // Whistle sound effect: 2 short bursts + 1 long burst
      const playBurst = (startTime: number, duration: number, freq: number) => {
        const osc = ctx.createOscillator()
        const gain = ctx.createGain()

        osc.type = 'sine'
        osc.frequency.setValueAtTime(freq, startTime)
        osc.frequency.exponentialRampToValueAtTime(freq + 150, startTime + duration * 0.5)
        osc.frequency.exponentialRampToValueAtTime(freq, startTime + duration)

        gain.gain.setValueAtTime(0, startTime)
        gain.gain.linearRampToValueAtTime(0.3, startTime + 0.02)
        gain.gain.linearRampToValueAtTime(0, startTime + duration)

        osc.connect(gain)
        gain.connect(ctx.destination)

        osc.start(startTime)
        osc.stop(startTime + duration)
      }

      playBurst(now, 0.15, 2600)
      playBurst(now + 0.22, 0.15, 2600)
      playBurst(now + 0.44, 0.45, 2800)
    } catch {}
  }

  function initTimerForCurrentPeriod() {
    intervalSecondsRemaining.value = currentPeriodDurationSeconds.value
    hasSoundAlertedForCurrentEnd.value = false
  }

  function startTimer() {
    if (isRunning.value) return
    isRunning.value = true

    if (intervalSecondsRemaining.value <= 0) {
      if (currentPeriodIndex.value < (squadRef.value?.periods.length || 1) - 1) {
        nextPeriod(true)
        return
      }
    }

    clearInterval(timerInterval)
    timerInterval = setInterval(() => {
      if (intervalSecondsRemaining.value > 0) {
        intervalSecondsRemaining.value--
        totalElapsedSeconds.value++
      }

      if (intervalSecondsRemaining.value <= 0) {
        pauseTimer()
        if (!hasSoundAlertedForCurrentEnd.value) {
          playWhistleSound()
          hasSoundAlertedForCurrentEnd.value = true
        }

        // Trigger substitution alert if there's a next period
        if (squadRef.value && currentPeriodIndex.value < squadRef.value.periods.length - 1) {
          showSubAlert.value = true
        }
      }
    }, 1000)
  }

  function pauseTimer() {
    isRunning.value = false
    if (timerInterval) {
      clearInterval(timerInterval)
      timerInterval = null
    }
  }

  function toggleTimer() {
    if (isRunning.value) {
      pauseTimer()
    } else {
      startTimer()
    }
  }

  function addSeconds(secs: number) {
    intervalSecondsRemaining.value = Math.max(0, intervalSecondsRemaining.value + secs)
    if (intervalSecondsRemaining.value > 0) {
      hasSoundAlertedForCurrentEnd.value = false
      showSubAlert.value = false
    }
  }

  function nextPeriod(autoStart: boolean = false) {
    if (!squadRef.value) return
    if (currentPeriodIndex.value < squadRef.value.periods.length - 1) {
      currentPeriodIndex.value++
      initTimerForCurrentPeriod()
      showSubAlert.value = false
      if (autoStart) {
        startTimer()
      } else {
        pauseTimer()
      }
    }
  }

  function prevPeriod() {
    if (currentPeriodIndex.value > 0) {
      currentPeriodIndex.value--
      initTimerForCurrentPeriod()
      showSubAlert.value = false
      pauseTimer()
    }
  }

  function goToPeriod(index: number) {
    if (!squadRef.value || index < 0 || index >= squadRef.value.periods.length) return
    currentPeriodIndex.value = index
    initTimerForCurrentPeriod()
    showSubAlert.value = false
    pauseTimer()
  }

  function confirmSubstitutions() {
    nextPeriod(true)
  }

  function resetMatch() {
    pauseTimer()
    currentPeriodIndex.value = 0
    totalElapsedSeconds.value = 0
    showSubAlert.value = false
    initTimerForCurrentPeriod()
  }

  onUnmounted(() => {
    if (timerInterval) {
      clearInterval(timerInterval)
    }
  })

  return {
    isRunning,
    currentPeriodIndex,
    currentPeriod,
    intervalSecondsRemaining,
    currentPeriodDurationSeconds,
    totalElapsedSeconds,
    showSubAlert,
    isHalfTime,
    isFullTime,
    formattedIntervalTimer,
    formattedTotalElapsed,
    intervalProgressPercent,
    startTimer,
    pauseTimer,
    toggleTimer,
    addSeconds,
    nextPeriod,
    prevPeriod,
    goToPeriod,
    confirmSubstitutions,
    resetMatch,
    playWhistleSound,
    initTimerForCurrentPeriod,
  }
}
