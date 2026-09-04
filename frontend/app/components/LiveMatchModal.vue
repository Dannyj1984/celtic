<template>
  <div v-if="isOpen" class="fixed inset-0 z-[120] flex items-center justify-center bg-black/85 backdrop-blur-lg p-2 sm:p-4 overflow-y-auto">
    <div class="card w-full max-w-5xl bg-surface border-border/80 p-4 sm:p-6 my-auto max-h-[96vh] flex flex-col shadow-2xl overflow-hidden rounded-2xl">
      
      <!-- Top Bar: Match Info, Clock & Controls -->
      <div class="flex flex-wrap items-center justify-between gap-4 border-b border-border/60 pb-4">
        <div>
          <div class="flex items-center gap-2">
            <span class="inline-flex items-center gap-1.5 px-2.5 py-0.5 rounded-full text-[11px] font-bold bg-danger/20 text-danger border border-danger/30 animate-pulse">
              <span class="w-2 h-2 rounded-full bg-danger"></span>
              LIVE MATCHDAY
            </span>
            <span class="text-xs font-semibold px-2 py-0.5 rounded bg-surface-hover text-text-secondary border border-border">
              {{ is3v3 ? '3v3 (no GK)' : '5v5 (with GK)' }} • {{ currentPeriod?.half === 1 ? '1st Half' : '2nd Half' }} • Period {{ (currentPeriodIndex + 1) }} of {{ squad?.periods.length || 8 }}
            </span>
          </div>
          <h2 class="text-xl sm:text-2xl font-bold text-text-primary mt-1 flex items-center gap-2">
            ⚽ {{ matchOpposition }}
          </h2>
        </div>

        <!-- Digital Clocks & Controls -->
        <div class="flex items-center gap-3 sm:gap-6 bg-surface-hover/80 px-4 py-2.5 rounded-2xl border border-border shadow-inner">
          <!-- Interval Countdown -->
          <div class="text-center">
            <div class="text-[10px] uppercase font-bold text-text-muted tracking-wider">Interval Timer</div>
            <div :class="['text-2xl sm:text-3xl font-mono font-extrabold tracking-tight', 
              intervalSecondsRemaining <= 30 ? 'text-danger animate-pulse' : 'text-celtic-gold']">
              {{ formattedIntervalTimer }}
            </div>
          </div>

          <div class="h-8 w-[1px] bg-border/80"></div>

          <!-- Total Match Time -->
          <div class="text-center">
            <div class="text-[10px] uppercase font-bold text-text-muted tracking-wider">Match Clock</div>
            <div class="text-lg sm:text-xl font-mono font-bold text-text-primary">
              {{ formattedTotalElapsed }}
            </div>
          </div>

          <!-- Main Actions -->
          <div class="flex items-center gap-1.5">
            <button 
              @click="toggleTimer"
              :class="['px-4 py-2 rounded-xl text-xs font-bold transition-all shadow-md flex items-center gap-1.5',
                isRunning ? 'bg-amber-500 hover:bg-amber-600 text-slate-950 shadow-amber-500/20' : 'bg-celtic-green hover:bg-celtic-green-dark text-white shadow-celtic-green/30']"
            >
              <span>{{ isRunning ? '⏸️' : '▶️' }}</span>
              <span>{{ isRunning ? 'Pause' : 'Start' }}</span>
            </button>

            <button 
              @click="addSeconds(60)"
              title="Add 1 minute"
              class="px-2.5 py-2 bg-surface hover:bg-surface-hover text-text-secondary rounded-xl text-xs font-bold border border-border transition-colors"
            >
              +1m
            </button>

            <button 
              @click="handleNextInterval"
              title="Next Interval / Substitution"
              class="px-2.5 py-2 bg-surface hover:bg-surface-hover text-text-secondary rounded-xl text-xs font-bold border border-border transition-colors flex items-center gap-1"
            >
              <span>⏭️</span>
              <span class="hidden sm:inline">Next Sub</span>
            </button>
          </div>
        </div>

        <button @click="close" class="text-text-muted hover:text-text-primary p-2 rounded-lg hover:bg-surface-hover transition-colors">
          <svg xmlns="http://www.w3.org/2000/svg" class="w-5 h-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
        </button>
      </div>

      <!-- Main Tactical Display -->
      <div class="flex-1 overflow-y-auto py-4 space-y-4 pr-1">
        
        <!-- Live Interval Progress Bar -->
        <div class="w-full bg-surface-hover rounded-full h-2 overflow-hidden border border-border/60">
          <div 
            class="bg-gradient-to-r from-celtic-green via-celtic-gold to-amber-500 h-full transition-all duration-1000 ease-linear rounded-full"
            :style="{ width: `${intervalProgressPercent}%` }"
          ></div>
        </div>

        <!-- Tactical Pitch -->
        <div class="relative w-full rounded-2xl overflow-hidden border-2 border-emerald-900/60 bg-gradient-to-b from-[#0a2312] via-[#0d2e18] to-[#0a2312] shadow-2xl p-4 sm:p-6 min-h-[360px] flex flex-col justify-between">
          
          <!-- Pitch Grass Markings (SVG overlay) -->
          <div class="absolute inset-0 pointer-events-none opacity-35">
            <!-- Center Line -->
            <div class="absolute top-1/2 left-0 right-0 h-[2px] bg-white"></div>
            <!-- Center Circle -->
            <div class="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-28 h-28 rounded-full border-2 border-white"></div>
            <div class="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-2 h-2 rounded-full bg-white"></div>
            <!-- Top Penalty Box (Opponent) -->
            <div class="absolute top-0 left-1/2 -translate-x-1/2 w-48 h-20 border-b-2 border-l-2 border-r-2 border-white rounded-b-lg"></div>
            <!-- Bottom Penalty Box (Our GK) -->
            <div class="absolute bottom-0 left-1/2 -translate-x-1/2 w-48 h-20 border-t-2 border-l-2 border-r-2 border-white rounded-t-lg"></div>
            <div class="absolute bottom-10 left-1/2 -translate-x-1/2 w-2 h-2 rounded-full bg-white"></div>
          </div>

          <!-- 3v3 Pitch Layout (Triangle Formation: 1 FWD top, 2 MID/DEF bottom) -->
          <template v-if="is3v3">
            <!-- Top Striker -->
            <div class="relative z-10 flex justify-center py-4">
              <div v-if="forwardPlayer" class="flex flex-col items-center group cursor-pointer" @click="handleSelectPlayerToSwap(forwardPlayer.id)">
                <div :class="['w-14 h-14 sm:w-16 sm:h-16 rounded-full flex flex-col items-center justify-center text-white font-black text-sm shadow-xl border-2 transition-transform transform group-hover:scale-110',
                  selectedSwapPlayerId === forwardPlayer.id ? 'bg-amber-500 border-white ring-4 ring-amber-400/50' : 'bg-gradient-to-br from-purple-600 to-purple-900 border-purple-300']">
                  <span>{{ getInitials(forwardPlayer.name) }}</span>
                  <span class="text-[9px] font-bold text-purple-200 uppercase">FWD</span>
                </div>
                <div class="mt-1 bg-black/80 backdrop-blur-md px-3 py-0.5 rounded-full border border-white/20 text-center">
                  <span class="text-xs font-bold text-white block truncate max-w-[130px]">{{ forwardPlayer.name }}</span>
                  <span class="text-[10px] text-celtic-gold font-semibold">{{ getPlayerLiveMinutes(forwardPlayer.id) }}m played</span>
                </div>
              </div>
            </div>

            <!-- Bottom 2 Outfielders -->
            <div class="relative z-10 flex justify-around px-8 py-6">
              <div v-for="player in currentPeriod?.outfieldPlayers.slice(1) || []" :key="player.id" class="flex flex-col items-center group cursor-pointer" @click="handleSelectPlayerToSwap(player.id)">
                <div :class="['w-14 h-14 sm:w-16 sm:h-16 rounded-full flex flex-col items-center justify-center text-white font-black text-sm shadow-xl border-2 transition-transform transform group-hover:scale-110',
                  selectedSwapPlayerId === player.id ? 'bg-amber-500 border-white ring-4 ring-amber-400/50' : 'bg-gradient-to-br from-emerald-600 to-emerald-900 border-emerald-300']">
                  <span>{{ getInitials(player.name) }}</span>
                  <span class="text-[9px] font-bold text-emerald-200 uppercase">OUTFIELD</span>
                </div>
                <div class="mt-1 bg-black/80 backdrop-blur-md px-3 py-0.5 rounded-full border border-white/20 text-center">
                  <span class="text-xs font-bold text-white block truncate max-w-[130px]">{{ player.name }}</span>
                  <span class="text-[10px] text-celtic-gold font-semibold">{{ getPlayerLiveMinutes(player.id) }}m played</span>
                </div>
              </div>
            </div>
          </template>

          <!-- 5v5 Pitch Layout (1 GK, 2 DEF, 1 MID, 1 FWD) -->
          <template v-else>
            <!-- Forward Line (Top) -->
            <div class="relative z-10 flex justify-center py-2">
              <div v-if="forwardPlayer" class="flex flex-col items-center group cursor-pointer" @click="handleSelectPlayerToSwap(forwardPlayer.id)">
                <div :class="['w-12 h-12 sm:w-14 sm:h-14 rounded-full flex flex-col items-center justify-center text-white font-black text-sm shadow-xl border-2 transition-transform transform group-hover:scale-110',
                  selectedSwapPlayerId === forwardPlayer.id ? 'bg-amber-500 border-white ring-4 ring-amber-400/50' : 'bg-gradient-to-br from-emerald-600 to-emerald-900 border-emerald-300']">
                  <span>{{ getInitials(forwardPlayer.name) }}</span>
                  <span class="text-[9px] font-bold text-emerald-200 uppercase">FWD</span>
                </div>
                <div class="mt-1 bg-black/80 backdrop-blur-md px-2.5 py-0.5 rounded-full border border-white/20 text-center">
                  <span class="text-xs font-bold text-white block truncate max-w-[120px]">{{ forwardPlayer.name }}</span>
                  <span class="text-[10px] text-celtic-gold font-semibold">{{ getPlayerLiveMinutes(forwardPlayer.id) }}m played</span>
                </div>
              </div>
            </div>

            <!-- Midfield Line (Center) -->
            <div class="relative z-10 flex justify-center py-2">
              <div v-if="midfieldPlayer" class="flex flex-col items-center group cursor-pointer" @click="handleSelectPlayerToSwap(midfieldPlayer.id)">
                <div :class="['w-12 h-12 sm:w-14 sm:h-14 rounded-full flex flex-col items-center justify-center text-white font-black text-sm shadow-xl border-2 transition-transform transform group-hover:scale-110',
                  selectedSwapPlayerId === midfieldPlayer.id ? 'bg-amber-500 border-white ring-4 ring-amber-400/50' : 'bg-gradient-to-br from-teal-600 to-teal-900 border-teal-300']">
                  <span>{{ getInitials(midfieldPlayer.name) }}</span>
                  <span class="text-[9px] font-bold text-teal-200 uppercase">MID</span>
                </div>
                <div class="mt-1 bg-black/80 backdrop-blur-md px-2.5 py-0.5 rounded-full border border-white/20 text-center">
                  <span class="text-xs font-bold text-white block truncate max-w-[120px]">{{ midfieldPlayer.name }}</span>
                  <span class="text-[10px] text-celtic-gold font-semibold">{{ getPlayerLiveMinutes(midfieldPlayer.id) }}m played</span>
                </div>
              </div>
            </div>

            <!-- Defenders Line (Bottom-Mid) -->
            <div class="relative z-10 flex justify-around px-8 py-2">
              <div v-for="def in defenderPlayers" :key="def.id" class="flex flex-col items-center group cursor-pointer" @click="handleSelectPlayerToSwap(def.id)">
                <div :class="['w-12 h-12 sm:w-14 sm:h-14 rounded-full flex flex-col items-center justify-center text-white font-black text-sm shadow-xl border-2 transition-transform transform group-hover:scale-110',
                  selectedSwapPlayerId === def.id ? 'bg-amber-500 border-white ring-4 ring-amber-400/50' : 'bg-gradient-to-br from-sky-600 to-blue-900 border-sky-300']">
                  <span>{{ getInitials(def.name) }}</span>
                  <span class="text-[9px] font-bold text-sky-200 uppercase">DEF</span>
                </div>
                <div class="mt-1 bg-black/80 backdrop-blur-md px-2.5 py-0.5 rounded-full border border-white/20 text-center">
                  <span class="text-xs font-bold text-white block truncate max-w-[120px]">{{ def.name }}</span>
                  <span class="text-[10px] text-celtic-gold font-semibold">{{ getPlayerLiveMinutes(def.id) }}m played</span>
                </div>
              </div>
            </div>

            <!-- Goalkeeper Line (Goal Box) -->
            <div class="relative z-10 flex justify-center pt-2">
              <div v-if="currentPeriod?.goalkeeper" class="flex flex-col items-center group cursor-pointer" @click="handleSelectPlayerToSwap(currentPeriod.goalkeeper.id)">
                <div :class="['w-12 h-12 sm:w-14 sm:h-14 rounded-full flex flex-col items-center justify-center text-white font-black text-sm shadow-xl border-2 transition-transform transform group-hover:scale-110',
                  selectedSwapPlayerId === currentPeriod.goalkeeper.id ? 'bg-amber-500 border-white ring-4 ring-amber-400/50' : 'bg-gradient-to-br from-amber-500 to-yellow-600 border-amber-200 text-slate-950']">
                  <span class="text-slate-950 font-black">{{ getInitials(currentPeriod.goalkeeper.name) }}</span>
                  <span class="text-[9px] font-black text-slate-900 uppercase">GK 🧤</span>
                </div>
                <div class="mt-1 bg-black/80 backdrop-blur-md px-2.5 py-0.5 rounded-full border border-amber-400/40 text-center">
                  <span class="text-xs font-bold text-amber-300 block truncate max-w-[120px]">{{ currentPeriod.goalkeeper.name }}</span>
                  <span class="text-[10px] text-celtic-gold font-semibold">{{ getPlayerLiveMinutes(currentPeriod.goalkeeper.id) }}m played</span>
                </div>
              </div>
            </div>
          </template>

        </div>

        <!-- Substitutes (Bench) Section -->
        <div class="p-4 bg-surface-hover/70 border border-border rounded-2xl space-y-3">
          <div class="flex items-center justify-between">
            <div class="flex items-center gap-2">
              <span class="text-base">🪑</span>
              <h3 class="text-xs font-bold uppercase tracking-wider text-text-secondary">
                Substitutes ({{ currentPeriod?.benchPlayers.length || 0 }} on Bench)
              </h3>
            </div>
            <span class="text-[11px] text-text-muted">
              💡 Click any player on pitch or bench to swap positions
            </span>
          </div>

          <!-- Bench Cards Grid -->
          <div class="grid grid-cols-2 sm:grid-cols-4 gap-3">
            <div 
              v-for="sub in currentPeriod?.benchPlayers || []" 
              :key="sub.id"
              @click="handleSelectPlayerToSwap(sub.id)"
              :class="['p-3 rounded-xl border transition-all cursor-pointer flex items-center gap-3',
                selectedSwapPlayerId === sub.id 
                  ? 'bg-amber-500/20 border-amber-400 ring-2 ring-amber-400/50' 
                  : 'bg-surface border-border hover:border-celtic-gold/40 hover:bg-surface-hover']"
            >
              <div class="w-10 h-10 rounded-full bg-surface-hover border border-border flex items-center justify-center font-bold text-text-primary text-xs shrink-0">
                {{ getInitials(sub.name) }}
              </div>
              <div class="min-w-0 flex-1">
                <div class="text-xs font-bold text-text-primary truncate">{{ sub.name }}</div>
                <div class="text-[10px] text-text-muted flex items-center gap-1">
                  <span>🪑 Ready</span>
                  <span>•</span>
                  <span class="text-celtic-gold font-semibold">{{ getPlayerLiveMinutes(sub.id) }}m played</span>
                </div>
              </div>
            </div>
          </div>
        </div>

      </div>

      <!-- Substitution Alert Modal / Overlay -->
      <div v-if="showSubAlert" class="fixed inset-0 z-[150] flex items-center justify-center bg-black/80 backdrop-blur-md p-4 animate-fade-in">
        <div class="card w-full max-w-lg bg-surface border-2 border-celtic-gold p-6 rounded-2xl shadow-2xl space-y-5 animate-scale-up">
          
          <div class="text-center space-y-1">
            <span class="text-4xl">⏱️ 🔄</span>
            <h3 class="text-2xl font-black text-text-primary mt-2">
              Time For Substitutions!
            </h3>
            <p class="text-xs text-text-secondary">
              Period {{ currentPeriodIndex + 1 }} has ended. Rotate to the next slot:
            </p>
          </div>

          <!-- Next Period Lineup & Substitutions Preview -->
          <div v-if="nextPeriodPreview" class="p-4 bg-surface-hover rounded-xl border border-border space-y-3">
            <div class="flex items-center justify-between border-b border-border/50 pb-2">
              <span class="text-xs font-bold uppercase tracking-wider text-celtic-gold">
                Upcoming: Period {{ currentPeriodIndex + 2 }} (Half {{ nextPeriodPreview.half }})
              </span>
              <span class="text-xs text-text-muted">
                {{ nextPeriodPreview.startMinute }}' - {{ nextPeriodPreview.endMinute }}'
              </span>
            </div>

            <!-- Planned Subs List -->
            <div v-if="nextPeriodPreview.substitutions.length > 0" class="space-y-2">
              <div 
                v-for="(sub, sIdx) in nextPeriodPreview.substitutions" 
                :key="sIdx"
                class="flex items-center justify-between p-2.5 bg-surface rounded-lg border border-border text-xs"
              >
                <div class="flex items-center gap-1.5 text-emerald-400 font-bold">
                  <span>🟢 ⬆️</span>
                  <span>{{ sub.playerInName }}</span>
                </div>
                <span class="text-text-muted text-[11px]">for</span>
                <div class="flex items-center gap-1.5 text-rose-400 font-bold">
                  <span>🔴 ⬇️</span>
                  <span>{{ sub.playerOutName }}</span>
                </div>
              </div>
            </div>

            <!-- GK Change notice if applicable -->
            <div v-if="isNextPeriodGkChange" class="p-2.5 bg-celtic-gold/10 border border-celtic-gold/30 rounded-lg text-xs flex items-center gap-2 text-celtic-gold font-bold">
              <span>🧤</span>
              <span>Goalkeeper Switch: {{ nextPeriodPreview.goalkeeper?.name }} takes over in goal!</span>
            </div>
          </div>

          <!-- Actions -->
          <div class="flex flex-col sm:flex-row items-center gap-2 pt-2">
            <button 
              @click="handleConfirmSubstitutions"
              class="btn-primary w-full py-3 text-sm font-black flex items-center justify-center gap-2 shadow-lg shadow-celtic-green/30"
            >
              <span>✅</span>
              <span>Confirm & Rotate Squad</span>
            </button>
            <button 
              @click="handleSnoozeOneMinute"
              class="btn-secondary w-full sm:w-auto py-3 px-4 text-xs font-semibold"
            >
              +1m Extra
            </button>
          </div>

        </div>
      </div>

    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import { useMatchSquad, type MatchSquad, type SquadPlayer, type SquadPeriod } from '~/composables/useMatchSquad'
import { useLiveMatchTimer } from '~/composables/useLiveMatchTimer'

const props = defineProps<{
  isOpen: boolean
  event?: any
  matchId?: string
  initialSquad?: MatchSquad | null
}>()

const emit = defineEmits<{
  (e: 'close'): void
  (e: 'updated', squad: MatchSquad): void
}>()

const { squad, swapPlayers, fetchSquad, generateSquad } = useMatchSquad()

// If initialSquad passed in, initialize squad with it
watch(() => props.initialSquad, (newInitial) => {
  if (newInitial) squad.value = newInitial
}, { immediate: true })

// Timer composable
const {
  isRunning,
  currentPeriodIndex,
  currentPeriod,
  intervalSecondsRemaining,
  totalElapsedSeconds,
  showSubAlert,
  formattedIntervalTimer,
  formattedTotalElapsed,
  intervalProgressPercent,
  startTimer,
  pauseTimer,
  toggleTimer,
  addSeconds,
  nextPeriod,
  confirmSubstitutions,
  initTimerForCurrentPeriod,
  resetMatch,
} = useLiveMatchTimer(squad)

const selectedSwapPlayerId = ref<string | null>(null)

const matchOpposition = computed(() => {
  return props.event?.opposition || props.event?.notes || 'Match Day'
})

const is3v3 = computed(() => {
  return squad.value?.format === '3v3' || props.event?.format === '3v3' || props.event?.match?.format === '3v3'
})

// Formation positioning: 1 GK, 2 DEF, 1 MID, 1 FWD (or 1 FWD + 2 Outfield for 3v3)
const forwardPlayer = computed<SquadPlayer | null>(() => {
  if (!currentPeriod.value || currentPeriod.value.outfieldPlayers.length === 0) return null
  return currentPeriod.value.outfieldPlayers[0] || null
})

const midfieldPlayer = computed<SquadPlayer | null>(() => {
  if (!currentPeriod.value || currentPeriod.value.outfieldPlayers.length < 2) return null
  return currentPeriod.value.outfieldPlayers[1] || null
})

const defenderPlayers = computed<SquadPlayer[]>(() => {
  if (!currentPeriod.value) return []
  return currentPeriod.value.outfieldPlayers.slice(2)
})

const nextPeriodPreview = computed<SquadPeriod | null>(() => {
  if (!squad.value || currentPeriodIndex.value >= squad.value.periods.length - 1) return null
  return squad.value.periods[currentPeriodIndex.value + 1] || null
})

const isNextPeriodGkChange = computed(() => {
  if (is3v3.value) return false
  if (!currentPeriod.value || !nextPeriodPreview.value) return false
  return currentPeriod.value.goalkeeper?.id !== nextPeriodPreview.value.goalkeeper?.id
})

watch(() => props.isOpen, async (open) => {
  if (open) {
    if (!squad.value) {
      const matchId = props.matchId || props.event?.matchId
      const eventId = props.event?.id
      const res = await fetchSquad(matchId, eventId)
      if (!res.success || !squad.value) {
        await generateSquad({
          matchId,
          eventId,
          format: props.event?.format || props.event?.match?.format || '5v5',
          halfDurationMinutes: props.event?.halfDurationMinutes || props.event?.match?.halfDurationMinutes || 25,
        })
      }
    }
    initTimerForCurrentPeriod()
  } else {
    pauseTimer()
  }
})

function getInitials(name?: string): string {
  if (!name) return '??'
  const parts = name.trim().split(' ')
  if (parts.length >= 2 && parts[0] && parts[1]) {
    return `${parts[0][0]}${parts[1][0]}`.toUpperCase()
  }
  return name.slice(0, 2).toUpperCase()
}

function getPlayerLiveMinutes(playerId: string): number {
  if (!squad.value) return 0
  const pm = squad.value.playerMinutes.find(p => p.playerId === playerId)
  return pm?.totalMinutes || 0
}

function handleSelectPlayerToSwap(playerId: string) {
  if (!selectedSwapPlayerId.value) {
    selectedSwapPlayerId.value = playerId
    return
  }

  if (selectedSwapPlayerId.value === playerId) {
    selectedSwapPlayerId.value = null
    return
  }

  // Swap the two selected players
  swapPlayers(currentPeriodIndex.value, selectedSwapPlayerId.value, playerId)
  selectedSwapPlayerId.value = null
  if (squad.value) emit('updated', squad.value)
}

function handleNextInterval() {
  if (currentPeriodIndex.value < (squad.value?.periods.length || 1) - 1) {
    showSubAlert.value = true
  }
}

function handleConfirmSubstitutions() {
  confirmSubstitutions()
}

function handleSnoozeOneMinute() {
  addSeconds(60)
  startTimer()
}

function close() {
  pauseTimer()
  emit('close')
}
</script>
