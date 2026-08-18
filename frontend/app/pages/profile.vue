<template>
  <div class="pb-20 md:pb-8">
    <div v-if="loading" class="flex justify-center py-12">
      <div class="animate-spin w-8 h-8 rounded-full border-4 border-celtic-green border-t-transparent"></div>
    </div>
    
    <div v-else-if="error" class="bg-danger/10 border border-danger/20 text-danger p-4 rounded-lg">
      {{ error }}
    </div>

    <div v-else-if="profile" class="space-y-8">
      <!-- Profile Header -->
      <div class="card p-8 bg-gradient-to-br from-surface to-surface-hover border-celtic-green/20 relative overflow-hidden">
        <div class="absolute top-0 right-0 p-4 opacity-10">
          <UIcon name="i-heroicons-user-20-solid" class="w-32 h-32" />
        </div>
        
        <div class="relative z-10 flex flex-col md:flex-row items-center gap-8">
          <div class="w-32 h-32 rounded-full bg-celtic-green/10 border-4 border-celtic-green/30 flex items-center justify-center text-5xl font-bold text-celtic-green">
            {{ profile.fullName.charAt(0) }}
          </div>
          
          <div class="text-center md:text-left">
            <h1 class="text-3xl font-black text-text-primary mb-2 uppercase tracking-tight">{{ profile.fullName }}</h1>
            <div class="flex flex-wrap justify-center md:justify-start gap-4">
              <div @click="openFootModal" class="foot-toggle flex items-center gap-2 text-text-secondary bg-surface px-3 py-1 rounded-full border border-border shadow-sm cursor-pointer hover:border-celtic-gold/50 transition-all select-none" title="Double click to change foot">
                <UIcon name="i-heroicons-sparkles-20-solid" class="w-4 h-4 text-celtic-gold" />
                <span class="text-sm font-bold uppercase tracking-wider">{{ profile.preferredFoot }} {{  profile.preferredFoot.toLowerCase() === 'both' ? 'Feet' : 'Foot' }}</span>
              </div>
              <div class="flex items-center gap-2 text-text-secondary bg-surface px-3 py-1 rounded-full border border-border shadow-sm">
                <UIcon name="i-heroicons-calendar-days-20-solid" class="w-4 h-4 text-celtic-green" />
                <span class="text-sm font-bold uppercase tracking-wider">Class of {{ createdYear }}</span>
              </div>
            </div>
          </div>
        </div>
      </div>

      <div class="grid grid-cols-1 lg:grid-cols-12 gap-8">
        <!-- Left Side: Stats & Badges -->
        <div class="lg:col-span-4 space-y-6">
          <!-- Seasonal Attendance -->
          <UCard class="bg-bg-card border-border-color shadow-md">
            <h3 class="text-sm font-black text-text-muted uppercase tracking-widest mb-6">Season Attendance</h3>
            <div class="flex flex-col items-center">
              <div class="relative w-32 h-32 mb-4">
                <svg class="w-full h-full transform -rotate-90" viewBox="0 0 36 36">
                  <path class="text-border-color" stroke-width="3" stroke="currentColor" fill="none"
                    d="M18 2.0845 a 15.9155 15.9155 0 0 1 0 31.831 a 15.9155 15.9155 0 0 1 0 -31.831" />
                  <path class="text-celtic-green transition-all duration-1000 ease-out" stroke-width="3"
                    :stroke-dasharray="attendancePercentage + ', 100'" stroke="currentColor" fill="none"
                    d="M18 2.0845 a 15.9155 15.9155 0 0 1 0 31.831 a 15.9155 15.9155 0 0 1 0 -31.831" />
                </svg>
                <div class="absolute inset-0 flex items-center justify-center">
                  <span class="text-2xl font-black text-text-primary">{{ attendancePercentage }}%</span>
                </div>
              </div>
              <p class="text-sm text-text-secondary font-medium">
                {{ profile.matchAttendance.attendedSessions }} / {{ profile.matchAttendance.totalSessions }} Matches
              </p>
            </div>
          </UCard>

          <!-- Badges & Achievements -->
          <UCard class="bg-bg-card border-border-color shadow-md">
            <h3 class="text-sm font-black text-text-muted uppercase tracking-widest mb-6">Achievements</h3>
            <div class="grid grid-cols-2 gap-4">
              <div v-for="badge in profile.badges" :key="badge.name" class="flex flex-col items-center text-center p-3 rounded-xl bg-surface-hover border border-border/50 group hover:border-celtic-green/30 transition-all">
                <div :class="['w-12 h-12 rounded-full mb-2 flex items-center justify-center shadow-lg', 
                  badge.tier === 'Gold' ? 'bg-gradient-to-br from-yellow-300 to-yellow-600 text-yellow-900' :
                  badge.tier === 'Silver' ? 'bg-gradient-to-br from-gray-300 to-gray-500 text-gray-800' :
                  badge.tier === 'Bronze' ? 'bg-gradient-to-br from-amber-600 to-amber-800 text-amber-100' :
                  'bg-gradient-to-br from-celtic-green to-celtic-green-light text-white']">
                  <UIcon :name="badge.type === 'PotM' ? 'i-heroicons-star-20-solid' : 'i-heroicons-fire-20-solid'" class="w-6 h-6" />
                </div>
                <span class="text-[10px] font-black uppercase tracking-tighter text-text-muted mb-1">{{ badge.tier }}</span>
                <span class="text-xs font-bold text-text-primary leading-tight">{{ badge.name }}</span>
              </div>
              
              <div v-if="profile.badges.length === 0" class="col-span-2 py-4 text-center">
                <p class="text-xs text-text-muted italic">Keep playing to earn badges!</p>
              </div>
            </div>
          </UCard>
        </div>

        <!-- Right Side: Recent Matches -->
        <div class="lg:col-span-8 space-y-6">
          <div class="flex items-center justify-between mb-4">
            <h2 class="text-xl font-black text-text-primary uppercase tracking-tight">Recent Form</h2>
            <NuxtLink to="/season" class="text-sm font-bold text-celtic-green hover:underline">View All Matches &rarr;</NuxtLink>
          </div>

          <div class="space-y-4">
            <div v-for="match in profile.recentMatches" :key="match.id" class="card p-5 group hover:border-celtic-green/30 transition-all">
              <div class="flex items-center justify-between">
                <div class="flex items-center gap-6">
                  <div :class="['w-12 h-12 rounded-xl flex flex-col items-center justify-center text-white font-black',
                    match.result === 'Win' ? 'bg-celtic-green shadow-[0_4px_12px_rgba(4,120,87,0.3)]' :
                    match.result === 'Loss' ? 'bg-danger shadow-[0_4px_12px_rgba(220,38,38,0.3)]' :
                    'bg-text-muted']">
                    <span class="text-xs leading-none">{{ match.result.charAt(0) }}</span>
                    <span class="text-sm">{{ match.score }}</span>
                  </div>
                  
                  <div>
                    <div class="flex items-center gap-2 mb-1">
                      <span class="text-[10px] font-black text-text-muted uppercase tracking-widest">{{ new Date(match.date).toLocaleDateString('en-GB', { day: 'numeric', month: 'short', year: 'numeric' }) }}</span>
                      <UBadge v-if="match.wasPlayerOfTheMatch" color="yellow" variant="solid" size="xs" class="font-black uppercase tracking-tighter">POTM</UBadge>
                    </div>
                    <div class="text-lg font-black text-text-primary uppercase tracking-tight">
                      Stalybridge Celtic U7 <span class="text-text-muted mx-2">vs</span> {{ match.opposition }}
                    </div>
                  </div>
                </div>
                
                <div class="hidden sm:block">
                  <UIcon name="i-heroicons-chevron-right-20-solid" class="w-5 h-5 text-text-muted group-hover:text-celtic-green transition-colors" />
                </div>
              </div>
            </div>

            <div v-if="profile.recentMatches.length === 0" class="card p-12 text-center border-dashed">
              <p class="text-text-muted font-medium">No match history available for this season yet.</p>
            </div>
          </div>
          
          <!-- PotM Stats -->
          <div class="bg-surface-hover rounded-2xl p-8 border border-border/50 flex items-center justify-between overflow-hidden relative">
            <div class="absolute -right-4 bottom-0 opacity-5">
               <UIcon name="i-heroicons-star-20-solid" class="w-32 h-32" />
            </div>
            <div>
              <h4 class="text-sm font-black text-text-muted uppercase tracking-widest mb-1">Player of the Match</h4>
              <p class="text-4xl font-black text-celtic-gold">{{ profile.playerOfTheMatchCount }}</p>
              <p class="text-xs text-text-secondary mt-1 font-medium italic">Awards this season</p>
            </div>
            <div class="flex -space-x-3">
               <div class="w-12 h-12 rounded-full bg-gradient-to-br from-yellow-300 to-yellow-600 border-4 border-surface shadow-xl flex items-center justify-center text-yellow-900 font-bold z-30">
                  <UIcon name="i-heroicons-star-20-solid" class="w-6 h-6" />
               </div>
               <div class="w-12 h-12 rounded-full bg-gradient-to-br from-gray-300 to-gray-500 border-4 border-surface shadow-xl flex items-center justify-center text-gray-800 font-bold z-20">
                  <UIcon name="i-heroicons-star-20-solid" class="w-6 h-6" />
               </div>
               <div class="w-12 h-12 rounded-full bg-gradient-to-br from-amber-600 to-amber-800 border-4 border-surface shadow-xl flex items-center justify-center text-amber-100 font-bold z-10">
                  <UIcon name="i-heroicons-star-20-solid" class="w-6 h-6" />
               </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Preferred Foot Modal -->
    <div v-if="showFootModal" class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm">
      <div class="card p-6 max-w-sm w-full bg-surface border border-border shadow-xl space-y-4">
        <div class="flex items-center justify-between">
          <h3 class="text-lg font-black text-text-primary uppercase tracking-tight">Select foot</h3>
          <button @click="showFootModal = false" class="text-text-muted hover:text-text-primary text-xl font-bold">&times;</button>
        </div>
        <p class="text-xs text-text-secondary font-medium">Choose the player's preferred foot:</p>
        <div class="grid grid-cols-3 gap-3">
          <button v-for="option in ['Right', 'Left', 'Both']" :key="option"
            @click="selectedFoot = option"
            :class="['py-2 px-3 rounded-lg text-sm font-bold border transition-all', 
              selectedFoot === option ? 'bg-celtic-green text-white border-celtic-green shadow-md' : 'bg-surface-hover border-border text-text-primary hover:border-celtic-green/50']">
            {{ option }}
          </button>
        </div>
        <div class="flex justify-end gap-2 pt-2">
          <button @click="showFootModal = false" class="px-4 py-2 text-xs font-bold uppercase text-text-muted hover:text-text-primary">Cancel</button>
          <button @click="saveFoot" :disabled="savingFoot" class="px-4 py-2 text-xs font-bold uppercase rounded-lg bg-celtic-green text-white hover:bg-celtic-green-light transition-all shadow-md disabled:opacity-50">
            {{ savingFoot ? 'Saving...' : 'Save' }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'

definePageMeta({
  layout: 'app',
})

useHead({
  title: 'Player Profile - Stalybridge Celtic U7',
})

const { getAuthHeaders } = useAuth()
const profile = ref<any>(null)
const loading = ref(false)
const error = ref<string | null>(null)

const attendancePercentage = computed(() => {
  if (!profile.value?.matchAttendance?.totalSessions) return 0
  const { attendedSessions, totalSessions } = profile.value.matchAttendance
  return Math.round((attendedSessions / totalSessions) * 100)
})

const createdYear = computed(() => profile.value?.createdYear || profile.value?.joinedYear || 2024)

const showFootModal = ref(false)
const selectedFoot = ref('Right')
const savingFoot = ref(false)
const toast = useToast()

function openFootModal() {
  selectedFoot.value = profile.value?.preferredFoot || 'Right'
  showFootModal.value = true
}

async function saveFoot() {
  if (!profile.value) return
  savingFoot.value = true
  try {
    await $fetch('/api/parent/preferred-foot', {
      method: 'PUT',
      headers: getAuthHeaders(),
      body: { preferredFoot: selectedFoot.value }
    })
    profile.value.preferredFoot = selectedFoot.value
    showFootModal.value = false
    toast.add({ title: 'Success', description: 'Preferred foot updated.', color: 'green' })
  } catch (err: any) {
    console.error('Failed to update preferred foot:', err)
  } finally {
    savingFoot.value = false
  }
}

onMounted(async () => {
  loading.value = true
  try {
    const data = await $fetch<any>('/api/parent/profile', {
      headers: getAuthHeaders()
    })
    profile.value = data
  } catch (err: any) {
    error.value = 'Failed to load player profile.'
  } finally {
    loading.value = false
  }
})
</script>
