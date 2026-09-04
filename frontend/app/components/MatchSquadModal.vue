<template>
  <div v-if="isOpen" class="fixed inset-0 z-[100] flex items-center justify-center bg-black/75 backdrop-blur-md p-4 overflow-y-auto">
    <div class="card w-full max-w-5xl p-6 animate-fade-in shadow-2xl border-celtic-gold/30 my-8 max-h-[90vh] flex flex-col">
      
      <!-- Header -->
      <div class="flex items-start justify-between border-b border-border/60 pb-4 mb-4">
        <div>
          <div class="flex items-center gap-2">
            <span class="text-xs font-bold uppercase tracking-wider px-2.5 py-0.5 rounded bg-celtic-gold/20 text-celtic-gold border border-celtic-gold/30">
              Matchday Squad Plan
            </span>
            <span class="text-xs font-bold px-2 py-0.5 rounded" :class="selectedFormat === '3v3' ? 'bg-purple-500/20 text-purple-400 border border-purple-500/30' : 'bg-cyan-500/20 text-cyan-400 border border-cyan-500/30'">
              {{ selectedFormat === '3v3' ? '3v3 League (No GK)' : '5v5 League (With GK)' }}
            </span>
            <span class="text-xs text-text-muted font-medium">
              • 2 × {{ squad?.halfDurationMinutes || selectedHalfDuration }}m ({{ (squad?.halfDurationMinutes || selectedHalfDuration) * 2 }} mins) • {{ squad?.periodDurationMinutes || 5 }}-min intervals • Equal Playing Time
            </span>
          </div>
          <h2 class="text-2xl font-bold text-text-primary mt-1 flex items-center gap-2">
            ⚽ {{ event?.notes || matchOpposition ? `${matchOpposition}` : 'Match Squad & Substitutions' }}
          </h2>
          <p class="text-xs text-text-secondary mt-0.5">
            {{ formattedDateTime }} • {{ event?.location || 'Match Ground' }}
          </p>
        </div>

        <button @click="close" class="text-text-muted hover:text-text-primary p-2 rounded-lg hover:bg-surface-hover transition-colors">
          <svg xmlns="http://www.w3.org/2000/svg" class="w-5 h-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
        </button>
      </div>

      <!-- Loading / Error states -->
      <div v-if="loading" class="py-16 flex flex-col items-center justify-center gap-3">
        <div class="animate-spin w-10 h-10 rounded-full border-4 border-celtic-green border-t-transparent"></div>
        <p class="text-sm text-text-secondary font-medium">Generating optimal rotation schedule...</p>
      </div>

      <div v-else-if="error" class="p-4 bg-danger/10 border border-danger/20 text-danger rounded-xl text-sm mb-4">
        {{ error }}
      </div>

      <!-- Main Content -->
      <div v-else-if="squad" class="overflow-y-auto space-y-6 flex-1 pr-1">
        
        <!-- Controls: Format, Goalkeeper split, Half Duration & Generate button -->
        <div class="p-4 bg-surface-hover/60 border border-border rounded-xl flex flex-wrap items-center justify-between gap-4">
          <div class="flex flex-wrap items-center gap-4">
            <div>
              <label class="block text-[11px] font-bold text-text-muted uppercase tracking-wider mb-1">
                🛡️ Match Format
              </label>
              <select v-model="selectedFormat" @change="handleFormatChange" class="input text-xs py-1.5 min-w-[140px] bg-surface font-semibold text-text-primary">
                <option value="5v5">5v5 (with GK)</option>
                <option value="3v3">3v3 (no GK)</option>
              </select>
            </div>

            <div>
              <label class="block text-[11px] font-bold text-text-muted uppercase tracking-wider mb-1">
                ⏱️ Half Match Time
              </label>
              <select v-model.number="selectedHalfDuration" @change="handleHalfDurationChange" class="input text-xs py-1.5 min-w-[130px] bg-surface">
                <option :value="15">2 × 15 min (30m)</option>
                <option :value="18">2 × 18 min (36m)</option>
                <option :value="20">2 × 20 min (40m)</option>
                <option :value="25">2 × 25 min (50m)</option>
              </select>
            </div>

            <div v-if="selectedFormat !== '3v3'">
              <label class="block text-[11px] font-bold text-text-muted uppercase tracking-wider mb-1">
                🧤 1st Half GK (0 - {{ squad.halfDurationMinutes || selectedHalfDuration }}')
              </label>
              <select v-model="selectedGk1" class="input text-xs py-1.5 min-w-[140px] bg-surface">
                <option v-for="p in squad.registeredPlayers" :key="p.id" :value="p.id">
                  {{ p.name }}
                </option>
              </select>
            </div>

            <div v-if="selectedFormat !== '3v3'">
              <label class="block text-[11px] font-bold text-text-muted uppercase tracking-wider mb-1">
                🧤 2nd Half GK ({{ squad.halfDurationMinutes || selectedHalfDuration }} - {{ (squad.halfDurationMinutes || selectedHalfDuration) * 2 }}')
              </label>
              <select v-model="selectedGk2" class="input text-xs py-1.5 min-w-[140px] bg-surface">
                <option v-for="p in squad.registeredPlayers" :key="p.id" :value="p.id">
                  {{ p.name }}
                </option>
              </select>
            </div>

            <div v-else class="flex items-center gap-2 pt-4 text-xs font-semibold text-purple-400 bg-purple-500/10 px-3 py-1.5 rounded-lg border border-purple-500/20">
              <span>⚡ 3v3 League: Outfield rotation only (no GK)</span>
            </div>
          </div>

          <div class="flex items-center gap-2">
            <button 
              @click="handleRegenerate" 
              class="btn-secondary text-xs py-2 px-3 flex items-center gap-1.5"
              :disabled="loading"
            >
              <span>🔄</span>
              <span>Regenerate Rotation</span>
            </button>
            <button 
              @click="handleCopySchedule" 
              class="px-3 py-2 bg-surface hover:bg-surface-hover text-text-secondary text-xs font-semibold rounded-lg border border-border flex items-center gap-1.5 transition-colors"
            >
              <span>📋</span>
              <span>{{ copyText }}</span>
            </button>
          </div>
        </div>

        <!-- View Mode Switcher -->
        <div class="flex items-center justify-between border-b border-border/50 pb-2">
          <div class="flex gap-2">
            <button 
              @click="viewMode = 'periods'"
              :class="['px-3 py-1.5 rounded-lg text-xs font-bold transition-all flex items-center gap-1.5', 
                viewMode === 'periods' ? 'bg-celtic-green text-white shadow-md shadow-celtic-green/20' : 'text-text-muted hover:text-text-primary bg-surface']"
            >
              <span>⏱️</span>
              <span>Intervals View</span>
            </button>
            <button 
              @click="viewMode = 'matrix'"
              :class="['px-3 py-1.5 rounded-lg text-xs font-bold transition-all flex items-center gap-1.5', 
                viewMode === 'matrix' ? 'bg-celtic-green text-white shadow-md shadow-celtic-green/20' : 'text-text-muted hover:text-text-primary bg-surface']"
            >
              <span>📊</span>
              <span>Match Matrix</span>
            </button>
            <button 
              @click="viewMode = 'minutes'"
              :class="['px-3 py-1.5 rounded-lg text-xs font-bold transition-all flex items-center gap-1.5', 
                viewMode === 'minutes' ? 'bg-celtic-green text-white shadow-md shadow-celtic-green/20' : 'text-text-muted hover:text-text-primary bg-surface']"
            >
              <span>⚖️</span>
              <span>Playing Time Summary</span>
            </button>
          </div>

          <span class="text-xs text-text-muted">
            {{ squad.registeredPlayers.length }} Registered Players • {{ Math.max(0, squad.registeredPlayers.length - (selectedFormat === '3v3' ? 3 : 5)) }} Subs per interval
          </span>
        </div>

        <!-- 1. Intervals View -->
        <div v-if="viewMode === 'periods'" class="space-y-4">
          
          <!-- Period Selector Tabs -->
          <div class="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-6 gap-2">
            <button 
              v-for="(p, idx) in squad.periods" 
              :key="idx"
              @click="activePeriodIndex = idx"
              :class="['p-2.5 rounded-xl border text-left transition-all relative overflow-hidden',
                activePeriodIndex === idx 
                  ? 'bg-surface-hover border-celtic-gold shadow-md' 
                  : 'bg-surface border-border hover:border-celtic-gold/40 text-text-muted']"
            >
              <div class="flex items-center justify-between mb-1">
                <span :class="['text-[10px] font-bold uppercase px-1.5 py-0.2 rounded', 
                  p.half === 1 ? 'bg-celtic-green/10 text-celtic-green' : 'bg-celtic-gold/10 text-celtic-gold']">
                  H{{ p.half }}
                </span>
                <span class="text-[11px] font-bold text-text-primary">
                  {{ p.startMinute }}' - {{ p.endMinute }}'
                </span>
              </div>
              <div class="text-[11px] truncate text-text-secondary">
                <span v-if="p.goalkeeper">🧤 {{ p.goalkeeper.name }}</span>
                <span v-else>⚽ 3 Outfield</span>
              </div>
              <div v-if="p.substitutions.length > 0" class="text-[10px] text-celtic-gold font-semibold mt-0.5">
                {{ p.substitutions.length }} {{ p.substitutions.length === 1 ? 'sub' : 'subs' }}
              </div>
              <div v-else-if="idx === 0" class="text-[10px] text-text-muted mt-0.5">
                Starting Lineup
              </div>
            </button>
          </div>

          <!-- Active Period Detail Card -->
          <div v-if="currentPeriod" class="card p-5 border border-border/80 bg-surface/50 space-y-4">
            <div class="flex items-center justify-between border-b border-border/50 pb-3">
              <div>
                <h3 class="text-base font-bold text-text-primary flex items-center gap-2">
                  <span>Period {{ currentPeriod.periodNumber }} ({{ currentPeriod.startMinute }} - {{ currentPeriod.endMinute }} mins)</span>
                  <span class="text-xs px-2 py-0.5 rounded font-semibold bg-surface border border-border text-text-secondary">
                    Half {{ currentPeriod.half }}
                  </span>
                </h3>
                <p class="text-xs text-text-secondary mt-0.5">
                  {{ currentPeriod.periodNumber === 1 ? 'Starting Lineup' : `${currentPeriod.substitutions.length} player substitutions at ${currentPeriod.startMinute}th minute mark` }}
                </p>
              </div>

              <!-- Quick swap helper info -->
              <span class="text-[11px] text-text-muted bg-surface px-2.5 py-1 rounded-lg border border-border/50">
                💡 Click any player to swap with another
              </span>
            </div>

            <!-- Substitutions Banner (for interval marks > 0) -->
            <div v-if="currentPeriod.substitutions.length > 0" class="p-3 bg-celtic-gold/10 border border-celtic-gold/30 rounded-xl space-y-2">
              <div class="text-xs font-bold text-celtic-gold uppercase tracking-wider flex items-center gap-1.5">
                <span>🔄</span>
                <span>Substitutions at {{ currentPeriod.startMinute }}'</span>
              </div>
              <div class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-2">
                <div v-for="(sub, sIdx) in currentPeriod.substitutions" :key="sIdx" 
                  class="bg-surface/80 p-2 rounded-lg border border-border/60 text-xs flex items-center justify-between">
                  <div class="flex items-center gap-1.5 text-celtic-green font-semibold">
                    <span class="text-sm">🟢 ⬆️</span>
                    <span>{{ sub.playerInName }}</span>
                  </div>
                  <div class="text-text-muted text-[10px] font-bold">FOR</div>
                  <div class="flex items-center gap-1.5 text-danger font-semibold">
                    <span>{{ sub.playerOutName }}</span>
                    <span class="text-sm">🔴 ⬇️</span>
                  </div>
                </div>
              </div>
            </div>

            <!-- Pitch vs Bench -->
            <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
              
              <!-- Pitch Area -->
              <div class="md:col-span-2 p-4 bg-gradient-to-b from-celtic-green/10 via-surface to-surface border border-celtic-green/30 rounded-xl space-y-3">
                <div class="flex items-center justify-between border-b border-celtic-green/20 pb-2">
                  <span class="text-xs font-bold uppercase tracking-wider text-celtic-green flex items-center gap-1.5">
                    <span>⚽</span>
                    <span>On Pitch ({{ currentPeriod.goalkeeper ? '5 Players' : '3 Players' }})</span>
                  </span>
                  <span class="text-[11px] text-text-muted">
                    {{ currentPeriod.goalkeeper ? '1 GK + 4 Outfield' : '3 Outfield (No GK)' }}
                  </span>
                </div>

                <div class="space-y-2">
                  <!-- Goalkeeper Slot (5v5 only) -->
                  <div v-if="currentPeriod.goalkeeper" class="p-2.5 rounded-lg bg-celtic-gold/10 border border-celtic-gold/30 flex items-center justify-between">
                    <div class="flex items-center gap-2">
                      <span class="text-base">🧤</span>
                      <div>
                        <div class="text-xs font-bold text-text-primary">{{ currentPeriod.goalkeeper.name }}</div>
                        <div class="text-[10px] text-celtic-gold font-semibold uppercase">Goalkeeper</div>
                      </div>
                    </div>
                    <select 
                      :value="currentPeriod.goalkeeper.id" 
                      @change="handleSwap(currentPeriod.goalkeeper.id, ($event.target as HTMLSelectElement).value)"
                      class="input text-xs py-1 px-2 bg-surface max-w-[140px]"
                    >
                      <option disabled value="">Swap GK with...</option>
                      <option v-for="p in getAllNonGkInPeriod(currentPeriod)" :key="p.id" :value="p.id">
                        {{ p.name }} ({{ isPlayerOnBench(currentPeriod, p.id) ? 'Bench' : 'Outfield' }})
                      </option>
                    </select>
                  </div>

                  <!-- Outfield Player Slots (4 in 5v5, 3 in 3v3) -->
                  <div :class="['grid gap-2 pt-1', selectedFormat === '3v3' ? 'grid-cols-1 sm:grid-cols-3' : 'grid-cols-1 sm:grid-cols-2']">
                    <div v-for="player in currentPeriod.outfieldPlayers" :key="player.id" 
                      class="p-2.5 rounded-lg bg-surface hover:bg-surface-hover border border-border flex items-center justify-between transition-all">
                      <div class="flex items-center gap-2 truncate pr-2">
                        <div class="w-2 h-2 rounded-full bg-celtic-green shrink-0"></div>
                        <span class="text-xs font-bold text-text-primary truncate">{{ player.name }}</span>
                      </div>
                      <select 
                        :value="''"
                        @change="handleSwap(player.id, ($event.target as HTMLSelectElement).value); ($event.target as HTMLSelectElement).value = ''"
                        class="input text-[11px] py-0.5 px-1.5 bg-surface-hover max-w-[110px]"
                      >
                        <option value="" disabled>Swap with...</option>
                        <option v-for="benchPlayer in currentPeriod.benchPlayers" :key="benchPlayer.id" :value="benchPlayer.id">
                          {{ benchPlayer.name }} (Bench)
                        </option>
                        <option v-if="currentPeriod.goalkeeper" :value="currentPeriod.goalkeeper.id">
                          {{ currentPeriod.goalkeeper.name }} (GK)
                        </option>
                      </select>
                    </div>
                  </div>
                </div>
              </div>

              <!-- Bench Area -->
              <div class="p-4 bg-surface border border-border/80 rounded-xl space-y-3">
                <div class="flex items-center justify-between border-b border-border/50 pb-2">
                  <span class="text-xs font-bold uppercase tracking-wider text-text-secondary flex items-center gap-1.5">
                    <span>🪑</span>
                    <span>Bench ({{ currentPeriod.benchPlayers.length }})</span>
                  </span>
                  <span class="text-[11px] text-text-muted">Substitutes</span>
                </div>

                <div v-if="currentPeriod.benchPlayers.length === 0" class="text-xs text-text-muted py-6 text-center italic">
                  No substitutes
                </div>

                <div v-else class="space-y-2">
                  <div v-for="player in currentPeriod.benchPlayers" :key="player.id" 
                    class="p-2.5 rounded-lg bg-surface-hover border border-border/60 flex items-center justify-between">
                    <div class="flex items-center gap-2 truncate pr-2">
                      <div class="w-2 h-2 rounded-full bg-border shrink-0"></div>
                      <span class="text-xs font-medium text-text-secondary truncate">{{ player.name }}</span>
                    </div>
                    <select 
                      :value="''"
                      @change="handleSwap(player.id, ($event.target as HTMLSelectElement).value); ($event.target as HTMLSelectElement).value = ''"
                      class="input text-[11px] py-0.5 px-1.5 bg-surface max-w-[110px]"
                    >
                      <option value="" disabled>Sub for...</option>
                      <option v-for="pitchPlayer in currentPeriod.outfieldPlayers" :key="pitchPlayer.id" :value="pitchPlayer.id">
                        {{ pitchPlayer.name }}
                      </option>
                      <option v-if="currentPeriod.goalkeeper" :value="currentPeriod.goalkeeper.id">
                        {{ currentPeriod.goalkeeper.name }} (GK)
                      </option>
                    </select>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- 2. Full Match Matrix View -->
        <div v-else-if="viewMode === 'matrix'" class="overflow-x-auto border border-border rounded-xl">
          <table class="w-full text-left text-xs">
            <thead class="bg-surface-hover/80 border-b border-border text-text-muted uppercase text-[10px] tracking-wider">
              <tr>
                <th class="p-3 sticky left-0 bg-surface z-10 font-bold">Player</th>
                <th v-for="p in squad.periods" :key="p.periodNumber" class="p-3 text-center border-l border-border/40 min-w-[100px]">
                  <div>{{ p.startMinute }}' - {{ p.endMinute }}'</div>
                  <div class="text-[9px] text-text-muted font-medium">Period {{ p.periodNumber }}</div>
                </th>
                <th class="p-3 text-center border-l border-border/60 font-bold text-celtic-gold">Total Mins</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-border/40">
              <tr v-for="pm in squad.playerMinutes" :key="pm.playerId" class="hover:bg-surface-hover/30 transition-colors">
                <td class="p-3 font-semibold text-text-primary sticky left-0 bg-surface z-10 truncate max-w-[160px]">
                  {{ pm.playerName }}
                </td>
                <td v-for="p in squad.periods" :key="p.periodNumber" class="p-2 text-center border-l border-border/40">
                  <span v-if="p.goalkeeper?.id === pm.playerId" class="inline-flex items-center gap-1 px-2 py-0.5 rounded text-[10px] font-bold bg-celtic-gold/20 text-celtic-gold border border-celtic-gold/30">
                    🧤 GK
                  </span>
                  <span v-else-if="p.outfieldPlayers.some(o => o.id === pm.playerId)" class="inline-flex items-center gap-1 px-2 py-0.5 rounded text-[10px] font-semibold bg-celtic-green/20 text-celtic-green border border-celtic-green/30">
                    🟢 Pitch
                  </span>
                  <span v-else class="inline-flex items-center px-2 py-0.5 rounded text-[10px] font-medium text-text-muted bg-surface border border-border/40">
                    Bench
                  </span>
                </td>
                <td class="p-3 text-center border-l border-border/60 font-bold text-text-primary">
                  {{ pm.totalMinutes }}m
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <!-- 3. Playing Time Summary View -->
        <div v-else-if="viewMode === 'minutes'" class="card p-5 border border-border space-y-4">
          <div class="flex items-center justify-between border-b border-border/50 pb-2">
            <h3 class="text-sm font-bold text-text-primary">Equal Playing Time Distribution</h3>
            <span class="text-xs text-text-muted">Target: ~{{ Math.round(180 / squad.registeredPlayers.length) }} mins per player</span>
          </div>

          <div class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-3">
            <div v-for="pm in squad.playerMinutes" :key="pm.playerId" 
              class="p-3 bg-surface rounded-xl border border-border/80 space-y-2">
              <div class="flex justify-between items-start">
                <span class="text-xs font-bold text-text-primary">{{ pm.playerName }}</span>
                <span class="text-xs font-extrabold text-celtic-gold px-2 py-0.5 rounded bg-celtic-gold/10 border border-celtic-gold/20">
                  {{ pm.totalMinutes }} mins
                </span>
              </div>

              <!-- Minute distribution progress bar -->
              <div class="w-full bg-surface-hover rounded-full h-2 flex overflow-hidden">
                <div 
                  :style="{ width: `${(pm.goalkeeperMinutes / (squad.totalPeriods * squad.periodDurationMinutes)) * 100}%` }" 
                  class="bg-celtic-gold h-full" 
                  title="Goalkeeper minutes"
                ></div>
                <div 
                  :style="{ width: `${(pm.outfieldMinutes / (squad.totalPeriods * squad.periodDurationMinutes)) * 100}%` }" 
                  class="bg-celtic-green h-full" 
                  title="Outfield minutes"
                ></div>
              </div>

              <div class="flex justify-between text-[10px] text-text-muted">
                <span>🧤 GK: {{ pm.goalkeeperMinutes }}m</span>
                <span>🟢 Outfield: {{ pm.outfieldMinutes }}m</span>
                <span>🪑 Bench: {{ pm.benchMinutes }}m</span>
              </div>
            </div>
          </div>
        </div>

      </div>

      <!-- Footer / Actions -->
      <div class="flex items-center justify-between border-t border-border/60 pt-4 mt-4">
        <button type="button" @click="close" class="btn-secondary text-xs">
          Close
        </button>

        <div class="flex items-center gap-3">
          <button 
            type="button" 
            @click="isLiveMatchOpen = true" 
            class="px-4 py-2 bg-gradient-to-r from-emerald-600 to-teal-600 hover:from-emerald-500 hover:to-teal-500 text-white font-bold text-xs rounded-xl shadow-lg shadow-emerald-900/30 flex items-center gap-1.5 transition-all transform hover:scale-[1.02]" 
            :disabled="!squad"
          >
            <span>▶️</span>
            <span>Live Match Mode</span>
          </button>

          <button 
            type="button" 
            @click="handleSave" 
            class="btn-primary text-xs py-2 px-5" 
            :disabled="saving || !squad"
          >
            {{ saving ? 'Saving Plan...' : '💾 Save Rotation Plan' }}
          </button>
        </div>
      </div>

    </div>

    <!-- Live Match Assistant Modal -->
    <LiveMatchModal 
      :is-open="isLiveMatchOpen" 
      :event="event" 
      :match-id="matchId" 
      :initial-squad="squad" 
      @close="isLiveMatchOpen = false" 
      @updated="squad = $event" 
    />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import { useMatchSquad, type MatchSquad, type SquadPeriod, type SquadPlayer } from '~/composables/useMatchSquad'
import LiveMatchModal from './LiveMatchModal.vue'

const props = defineProps<{
  isOpen: boolean
  event?: any
  matchId?: string
}>()

const emit = defineEmits<{
  (e: 'close'): void
  (e: 'saved', squad: MatchSquad): void
}>()

function notify(title: string, color: string = 'emerald') {
  try {
    if (typeof useToast === 'function') {
      const toast = useToast()
      toast.add?.({ title, color })
    }
  } catch {}
}

const { squad, loading, saving, error, fetchSquad, generateSquad, saveSquad, swapPlayers } = useMatchSquad()

const viewMode = ref<'periods' | 'matrix' | 'minutes'>('periods')
const activePeriodIndex = ref(0)
const isLiveMatchOpen = ref(false)
const selectedFormat = ref<string>('5v5')
const selectedHalfDuration = ref<number>(18)
const selectedGk1 = ref<string>('')
const selectedGk2 = ref<string>('')
const copyText = ref('Copy Schedule')

const matchOpposition = computed(() => {
  return props.event?.opposition || props.event?.notes || 'Match'
})

const formattedDateTime = computed(() => {
  if (!props.event?.dateTime) return ''
  const d = new Date(props.event.dateTime)
  return `${d.toLocaleDateString('en-GB', { weekday: 'short', day: 'numeric', month: 'short' })} at ${d.toLocaleTimeString('en-GB', { hour: '2-digit', minute: '2-digit' })}`
})

const currentPeriod = computed<SquadPeriod | null>(() => {
  if (!squad.value || !squad.value.periods[activePeriodIndex.value]) return null
  return squad.value.periods[activePeriodIndex.value] || null
})

watch(() => props.isOpen, async (open) => {
  if (open) {
    activePeriodIndex.value = 0
    if (props.event?.format || props.event?.match?.format) {
      selectedFormat.value = props.event?.format || props.event?.match?.format
    }
    if (props.event?.halfDurationMinutes || props.event?.match?.halfDurationMinutes) {
      selectedHalfDuration.value = props.event?.halfDurationMinutes || props.event?.match?.halfDurationMinutes
    }
    await loadOrGenerateSquad()
  }
})

watch(() => squad.value, (newSquad) => {
  if (newSquad) {
    if (newSquad.format) selectedFormat.value = newSquad.format
    if (newSquad.firstHalfGoalkeeperPlayerId) selectedGk1.value = newSquad.firstHalfGoalkeeperPlayerId
    if (newSquad.secondHalfGoalkeeperPlayerId) selectedGk2.value = newSquad.secondHalfGoalkeeperPlayerId
    if (newSquad.halfDurationMinutes) selectedHalfDuration.value = newSquad.halfDurationMinutes
  }
})

async function loadOrGenerateSquad() {
  const matchId = props.matchId || props.event?.matchId
  const eventId = props.event?.id
  const halfDuration = props.event?.halfDurationMinutes || props.event?.match?.halfDurationMinutes || selectedHalfDuration.value
  const format = props.event?.format || props.event?.match?.format || selectedFormat.value

  selectedHalfDuration.value = halfDuration
  selectedFormat.value = format

  // Try fetching existing squad first
  const res = await fetchSquad(matchId, eventId)
  if (!res.success || !squad.value) {
    // Generate new squad from event attendance
    await generateSquad({
      matchId,
      eventId,
      format: selectedFormat.value,
      halfDurationMinutes: selectedHalfDuration.value,
    })
  }
}

async function handleFormatChange() {
  await handleRegenerate()
}

async function handleHalfDurationChange() {
  await handleRegenerate()
}

async function handleRegenerate() {
  const matchId = props.matchId || props.event?.matchId
  const eventId = props.event?.id

  const res = await generateSquad({
    matchId,
    eventId,
    format: selectedFormat.value,
    gk1Id: selectedFormat.value === '3v3' ? undefined : (selectedGk1.value || undefined),
    gk2Id: selectedFormat.value === '3v3' ? undefined : (selectedGk2.value || undefined),
    halfDurationMinutes: selectedHalfDuration.value,
  })

  if (res.success) {
    notify(`Squad rotation regenerated for ${selectedFormat.value === '3v3' ? '3v3 (no GK)' : '5v5 (with GK)'}!`, 'emerald')
  }
}

function getAllNonGkInPeriod(period: SquadPeriod): SquadPlayer[] {
  return [...period.outfieldPlayers, ...period.benchPlayers]
}

function isPlayerOnBench(period: SquadPeriod, playerId: string): boolean {
  return period.benchPlayers.some(p => p.id === playerId)
}

function handleSwap(playerAId: string, playerBId: string) {
  if (!playerAId || !playerBId || playerAId === playerBId) return
  swapPlayers(activePeriodIndex.value, playerAId, playerBId)
  notify('Player positions swapped', 'emerald')
}

async function handleSave() {
  const matchId = props.matchId || props.event?.matchId
  const eventId = props.event?.id

  const res = await saveSquad(matchId, eventId)
  if (res.success && res.squad) {
    notify('Match squad plan saved successfully!', 'emerald')
    emit('saved', res.squad)
  }
}

function handleCopySchedule() {
  if (!squad.value) return

  let text = `⚽ MATCH SQUAD & ROTATION SCHEDULE (${selectedFormat.value === '3v3' ? '3v3 League - No GK' : '5v5 League'})\n`
  text += `${matchOpposition.value} - ${formattedDateTime.value}\n\n`
  if (selectedFormat.value !== '3v3') {
    text += `🧤 1st Half GK: ${squad.value.firstHalfGoalkeeperName || 'TBD'}\n`
    text += `🧤 2nd Half GK: ${squad.value.secondHalfGoalkeeperName || 'TBD'}\n\n`
  }

  squad.value.periods.forEach(p => {
    text += `⏱️ [${p.startMinute}' - ${p.endMinute}'] (Half ${p.half})\n`
    if (p.goalkeeper) {
      text += `  🧤 GK: ${p.goalkeeper.name}\n`
    }
    text += `  🟢 Pitch: ${p.outfieldPlayers.map(o => o.name).join(', ')}\n`
    text += `  🪑 Bench: ${p.benchPlayers.map(b => b.name).join(', ')}\n`
    if (p.substitutions.length > 0) {
      text += `  🔄 Subs: ${p.substitutions.map(s => `${s.playerInName} ON for ${s.playerOutName}`).join(' | ')}\n`
    }
    text += `\n`
  })

  text += `📊 PLAYING TIME SUMMARY:\n`
  squad.value.playerMinutes.forEach(pm => {
    text += `• ${pm.playerName}: ${pm.totalMinutes}m (${selectedFormat.value !== '3v3' ? `GK: ${pm.goalkeeperMinutes}m, ` : ''}Pitch: ${pm.outfieldMinutes}m, Bench: ${pm.benchMinutes}m)\n`
  })

  navigator.clipboard.writeText(text)
  copyText.value = 'Copied! ✅'
  setTimeout(() => {
    copyText.value = 'Copy Schedule'
  }, 2500)
  notify('Schedule copied to clipboard!', 'emerald')
}

function close() {
  emit('close')
}
</script>
