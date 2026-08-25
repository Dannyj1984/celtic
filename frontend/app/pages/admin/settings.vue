<template>
  <div>
    <div class="mb-8">
      <h1 class="text-2xl font-bold text-text-primary">Club Settings</h1>
      <p class="text-text-secondary mt-1">Manage global club configurations.</p>
    </div>

    <div v-if="pending" class="text-center py-10">
      <p class="text-text-muted">Loading settings...</p>
    </div>

    <div v-else-if="error" class="text-center py-10 text-red-500">
      Failed to load settings.
    </div>

    <UCard v-else class="max-w-2xl bg-bg-card border-border-color shadow-sm">
      <UForm :state="state" class="space-y-6" @submit="saveSettings">

        <!-- Subscription Settings -->
        <div>
          <h2 class="text-lg font-semibold text-text-primary mb-4 border-b border-border-color pb-2">Subscription
            Settings</h2>
          <UFormGroup label="Next Payment Due Date" name="nextSubPaymentDate" class="mb-4">
            <UInput v-model="state.nextSubPaymentDate" type="date" />
          </UFormGroup>
        </div>

        <!-- Training Schedule Settings -->
        <div>
          <h2 class="text-lg font-semibold text-text-primary mb-4 border-b border-border-color pb-2">Weekly Training
            Schedule</h2>
          <div class="grid grid-cols-1 md:grid-cols-2 gap-4 mb-4">
            <UFormGroup label="Day of Week" name="trainingDay">
              <USelect v-model="state.trainingDay" :options="dayOptions" option-attribute="label"
                value-attribute="value" />
            </UFormGroup>
            <UFormGroup label="Location" name="trainingLocation">
              <UInput v-model="state.trainingLocation" placeholder="e.g. Riverside Pitch 4" />
            </UFormGroup>
          </div>
          <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
            <UFormGroup label="Start Time" name="trainingStartTime">
              <UInput v-model="state.trainingStartTime" type="time" />
            </UFormGroup>
            <UFormGroup label="End Time" name="trainingEndTime">
              <UInput v-model="state.trainingEndTime" type="time" />
            </UFormGroup>
          </div>
          <UFormGroup label="Next Training Focus" name="trainingFocus" class="mt-4"
            help="Visible to parents on the dashboard">
            <UTextarea v-model="state.trainingFocus" placeholder="e.g. Defensive positioning and 1v1 drills" />
          </UFormGroup>
          <UFormGroup label="Good to know details" name="goodToKnow" class="mt-4"
            help="Visible to parents on the dashboard">
            <UTextarea v-model="state.goodToKnow" placeholder="e.g. Please bring warm clothes"
              data-testid="goodToKnow-input" />
          </UFormGroup>
        </div>

        <!-- Card Milestone Rewards Settings -->
        <div>
          <h2 class="text-lg font-semibold text-text-primary mb-4 border-b border-border-color pb-2 flex items-center justify-between">
            <span>🎴 Card Milestone Rewards</span>
            <button type="button" @click="addCardReward" class="text-xs text-celtic-green font-bold hover:underline">+ Add Reward</button>
          </h2>
          <p class="text-xs text-text-muted mb-4">Set how many training cards collected unlocks each reward for players.</p>

          <div class="space-y-3">
            <div v-for="(reward, index) in state.cardRewards" :key="index" class="flex items-center gap-3 p-3 bg-surface-hover/40 border border-border-color rounded-lg">
              <div class="w-28 shrink-0">
                <label class="block text-[11px] font-medium text-text-secondary mb-1">Cards Required</label>
                <input v-model.number="reward.cardsRequired" type="number" min="1" class="input text-xs py-1.5" required placeholder="5" />
              </div>
              <div class="flex-1">
                <label class="block text-[11px] font-medium text-text-secondary mb-1">Reward Description</label>
                <input v-model="reward.rewardText" type="text" class="input text-xs py-1.5" required placeholder="e.g. Choose games in next session" />
              </div>
              <button type="button" @click="removeCardReward(index)" class="self-end p-2 text-danger hover:bg-danger/10 rounded-lg text-xs font-bold transition-colors">
                ✕
              </button>
            </div>
          </div>

          <div v-if="state.cardRewards.length === 0" class="text-center py-4 text-xs text-text-muted italic border border-dashed rounded-lg">
            No card milestone rewards defined. Click "+ Add Reward" to create one.
          </div>
        </div>

        <!-- Contact Settings -->
        <div>
          <h2 class="text-lg font-semibold text-text-primary mb-4 border-b border-border-color pb-2">Contact Info</h2>
          <UFormGroup label="Coach WhatsApp Number" name="coachWhatsAppNumber"
            help="Include country code, no + or spaces (e.g. 447123456789)">
            <UInput v-model="state.coachWhatsAppNumber" placeholder="447..." />
          </UFormGroup>
        </div>

        <div class="pt-4 flex justify-end">
          <UButton type="submit" color="primary" :loading="saving">Save Settings</UButton>
        </div>
      </UForm>
    </UCard>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'

definePageMeta({
  layout: 'app',
})

useHead({
  title: 'Settings - Admin',
})

const { getAuthHeaders } = useAuth()

const { data: settings, pending, error } = useFetch<any>('/api/settings', {
  server: false,
  headers: getAuthHeaders()
})

const toast = useToast()
const saving = ref(false)

const dayOptions = [
  { label: 'Sunday', value: 0 },
  { label: 'Monday', value: 1 },
  { label: 'Tuesday', value: 2 },
  { label: 'Wednesday', value: 3 },
  { label: 'Thursday', value: 4 },
  { label: 'Friday', value: 5 },
  { label: 'Saturday', value: 6 },
]

interface CardReward {
  cardsRequired: number
  rewardText: string
}

const state = ref({
  nextSubPaymentDate: '',
  trainingDay: 3,
  trainingStartTime: '17:00',
  trainingEndTime: '18:30',
  trainingLocation: '',
  coachWhatsAppNumber: '',
  trainingFocus: '',
  goodToKnow: '',
  cardRewards: [] as CardReward[]
})

watch(settings, (newVal) => {
  if (newVal) {
    state.value.nextSubPaymentDate = newVal.nextSubPaymentDate ? newVal.nextSubPaymentDate.split('T')[0] : ''
    state.value.trainingDay = newVal.trainingDay
    state.value.trainingStartTime = newVal.trainingStartTime?.substring(0, 5) || '17:00'
    state.value.trainingEndTime = newVal.trainingEndTime?.substring(0, 5) || '18:30'
    state.value.trainingLocation = newVal.trainingLocation || ''
    state.value.coachWhatsAppNumber = newVal.coachWhatsAppNumber || ''
    state.value.trainingFocus = newVal.trainingFocus || ''
    state.value.goodToKnow = newVal.goodToKnow || ''
    try {
      state.value.cardRewards = newVal.cardRewardsJson ? JSON.parse(newVal.cardRewardsJson) : [
        { cardsRequired: 5, rewardText: 'Choose what games to play in the next session' }
      ]
    } catch {
      state.value.cardRewards = [{ cardsRequired: 5, rewardText: 'Choose what games to play in the next session' }]
    }
  }
}, { immediate: true })

function addCardReward() {
  const nextCount = (state.value.cardRewards.length + 1) * 5
  state.value.cardRewards.push({
    cardsRequired: nextCount,
    rewardText: ''
  })
}

function removeCardReward(index: number) {
  state.value.cardRewards.splice(index, 1)
}

const saveSettings = async () => {
  saving.value = true
  try {
    const sortedRewards = [...state.value.cardRewards].sort((a, b) => a.cardsRequired - b.cardsRequired)
    const payload = {
      nextSubPaymentDate: state.value.nextSubPaymentDate ? new Date(state.value.nextSubPaymentDate).toISOString() : new Date().toISOString(),
      trainingDay: parseInt(state.value.trainingDay.toString()),
      trainingStartTime: state.value.trainingStartTime + ':00',
      trainingEndTime: state.value.trainingEndTime + ':00',
      trainingLocation: state.value.trainingLocation,
      coachWhatsAppNumber: state.value.coachWhatsAppNumber,
      trainingFocus: state.value.trainingFocus,
      goodToKnow: state.value.goodToKnow,
      cardRewardsJson: JSON.stringify(sortedRewards)
    }

    await $fetch('/api/settings', {
      method: 'PUT',
      headers: getAuthHeaders(),
      body: payload
    })

    toast.add({ title: 'Success', description: 'Settings updated successfully.', color: 'green' })
  } catch (e) {
    toast.add({ title: 'Error', description: 'Failed to update settings.', color: 'red' })
  } finally {
    saving.value = false
  }
}
</script>
