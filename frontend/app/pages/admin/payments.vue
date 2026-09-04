<template>
  <div class="space-y-8">
    <!-- Header Section -->
    <div class="flex flex-col md:flex-row md:items-center justify-between gap-4">
      <div>
        <h1 class="text-2xl font-bold text-text-primary">Income & Expenditure</h1>
        <p class="text-text-secondary mt-1">Track player monthly subs and team outgoing payments</p>
      </div>

      <!-- Controls: Season Select & Add Expense -->
      <div class="flex items-center gap-3">
        <div class="flex items-center gap-2 bg-surface border border-border px-3 py-1.5 rounded-xl shadow-sm">
          <svg xmlns="http://www.w3.org/2000/svg" class="w-4 h-4 text-celtic-green" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
          </svg>
          <span class="text-xs text-text-muted font-medium">Season:</span>
          <select v-model="selectedSeasonId" @change="onSeasonChange" class="bg-transparent text-sm font-semibold text-text-primary focus:outline-none cursor-pointer">
            <option v-for="s in seasons" :key="s.id" :value="s.id">
              {{ s.name }} {{ s.isCurrent ? '(Current)' : '' }}
            </option>
          </select>
        </div>

        <button v-if="activeTab === 'expenses'" @click="openExpenseModal()" class="btn-primary flex items-center gap-2 text-sm whitespace-nowrap">
          <svg xmlns="http://www.w3.org/2000/svg" class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
          </svg>
          Add Expense
        </button>
      </div>
    </div>

    <!-- Financial Summary Cards -->
    <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
      <!-- Income Card -->
      <div class="card p-5 border-l-4 border-l-celtic-green flex flex-col justify-between">
        <div class="flex items-center justify-between">
          <span class="text-xs font-semibold uppercase tracking-wider text-text-muted">Total Income</span>
          <div class="w-8 h-8 rounded-lg bg-celtic-green/10 text-celtic-green flex items-center justify-center">
            <svg xmlns="http://www.w3.org/2000/svg" class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
          </div>
        </div>
        <div class="mt-4">
          <p class="text-2xl font-bold text-text-primary">£{{ (summary?.totalIncome ?? 0).toFixed(2) }}</p>
          <p class="text-xs text-text-secondary mt-1">
            Monthly Subs Collected
          </p>
        </div>
      </div>

      <!-- Expenditure Card -->
      <div class="card p-5 border-l-4 border-l-danger flex flex-col justify-between">
        <div class="flex items-center justify-between">
          <span class="text-xs font-semibold uppercase tracking-wider text-text-muted">Total Expenditure</span>
          <div class="w-8 h-8 rounded-lg bg-danger/10 text-danger flex items-center justify-center">
            <svg xmlns="http://www.w3.org/2000/svg" class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12H9m12 0a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
          </div>
        </div>
        <div class="mt-4">
          <p class="text-2xl font-bold text-text-primary">£{{ (summary?.totalExpenses ?? 0).toFixed(2) }}</p>
          <p class="text-xs text-text-secondary mt-1">
            {{ expenses.length }} Outgoing Payments
          </p>
        </div>
      </div>

      <!-- Net Balance Card -->
      <div class="card p-5 border-l-4 border-l-celtic-gold flex flex-col justify-between">
        <div class="flex items-center justify-between">
          <span class="text-xs font-semibold uppercase tracking-wider text-text-muted">Net Balance</span>
          <div class="w-8 h-8 rounded-lg bg-celtic-gold/10 text-celtic-gold flex items-center justify-center">
            <svg xmlns="http://www.w3.org/2000/svg" class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z" />
            </svg>
          </div>
        </div>
        <div class="mt-4">
          <p :class="['text-2xl font-bold', (summary?.netBalance ?? 0) >= 0 ? 'text-celtic-green' : 'text-danger']">
            £{{ (summary?.netBalance ?? 0).toFixed(2) }}
          </p>
          <p class="text-xs text-text-secondary mt-1">
            Income minus Outgoing
          </p>
        </div>
      </div>

      <!-- Season Setup Sub Info Card -->
      <div class="card p-5 border-l-4 border-l-blue-500 flex flex-col justify-between">
        <div class="flex items-center justify-between">
          <span class="text-xs font-semibold uppercase tracking-wider text-text-muted">Sub Rate</span>
          <NuxtLink to="/admin/seasons" class="text-xs text-celtic-green hover:underline">Season Setup &rarr;</NuxtLink>
        </div>
        <div class="mt-4">
          <p class="text-2xl font-bold text-text-primary">
            £{{ (summary?.subAmount ?? 0).toFixed(2) }}
            <span class="text-xs font-normal text-text-secondary">/ {{ summary?.subFrequency?.toLowerCase() }}</span>
          </p>
          <p class="text-xs text-text-secondary mt-1">
            {{ summary?.activePlayersCount ?? 0 }} Active Squad Players
          </p>
        </div>
      </div>
    </div>

    <!-- Main Tabs -->
    <div class="card overflow-hidden border border-border">
      <div class="flex border-b border-border bg-surface-light px-4 pt-3 gap-2">
        <button
          @click="activeTab = 'subs'"
          :class="['px-5 py-2.5 text-sm font-semibold rounded-t-xl transition-colors border-b-2', activeTab === 'subs' ? 'bg-surface text-celtic-green border-celtic-green shadow-sm' : 'text-text-secondary hover:text-text-primary border-transparent']"
        >
          Monthly Subs (Income)
        </button>
        <button
          @click="activeTab = 'expenses'"
          :class="['px-5 py-2.5 text-sm font-semibold rounded-t-xl transition-colors border-b-2', activeTab === 'expenses' ? 'bg-surface text-celtic-green border-celtic-green shadow-sm' : 'text-text-secondary hover:text-text-primary border-transparent']"
        >
          Outgoing Expenses (Expenditure)
        </button>
      </div>

      <!-- Tab Content: Monthly Subs -->
      <div v-if="activeTab === 'subs'" class="p-6 space-y-6">
        <div class="flex flex-col sm:flex-row items-center justify-between gap-4">
          <div class="flex items-center gap-3 w-full sm:w-auto flex-wrap">
            <span class="text-sm font-medium text-text-secondary">Period / Month:</span>
            <select v-model="selectedMonth" @change="loadSubStatuses" class="input py-1.5 text-sm bg-surface">
              <option v-for="m in availableMonths" :key="m.value" :value="m.value">
                {{ m.label }}
              </option>
            </select>

            <span class="text-sm font-medium text-text-secondary ml-2">Team:</span>
            <select v-model="selectedTeamFilter" class="input py-1.5 text-sm bg-surface max-w-[150px]">
              <option value="All">All Teams</option>
              <option value="Unassigned">Unassigned</option>
              <option v-for="team in teams" :key="team.id" :value="team.id">
                {{ team.name }}
              </option>
            </select>
          </div>
          <div class="w-full sm:w-64">
            <input v-model="searchQuery" type="text" placeholder="Search player name..." class="input py-1.5 text-sm" />
          </div>
        </div>

        <div v-if="loading" class="flex justify-center py-12">
          <div class="animate-spin w-8 h-8 rounded-full border-4 border-celtic-green border-t-transparent"></div>
        </div>

        <!-- Squad Subs Table -->
        <div v-else class="overflow-x-auto">
          <table class="w-full text-left border-collapse">
            <thead>
              <tr class="border-b border-border bg-surface-hover text-xs font-semibold uppercase text-text-muted">
                <th class="py-3 px-4">Player</th>
                <th class="py-3 px-4">Status</th>
                <th class="py-3 px-4">Expected Sub</th>
                <th class="py-3 px-4">Paid Date</th>
                <th class="py-3 px-4">Method</th>
                <th class="py-3 px-4 text-right">Action</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-border text-sm">
              <tr v-for="player in filteredPlayers" :key="player.playerId" class="hover:bg-surface-hover/50 transition-colors">
                <td class="py-3.5 px-4 font-semibold text-text-primary">
                  <div class="flex items-center gap-2">
                    <span>{{ player.playerName }}</span>
                    <span v-if="player.teamName" class="badge bg-celtic-gold/10 text-celtic-gold border border-celtic-gold/30 text-[10px] px-1.5 py-0.5 font-medium">
                      {{ player.teamName }}
                    </span>
                  </div>
                </td>
                <td class="py-3.5 px-4">
                  <span v-if="getPeriodStatus(player)?.isPaid" class="badge badge-success flex items-center gap-1 w-max">
                    <svg xmlns="http://www.w3.org/2000/svg" class="w-3.5 h-3.5" viewBox="0 0 20 20" fill="currentColor">
                      <path fill-rule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clip-rule="evenodd" />
                    </svg>
                    Paid
                  </span>
                  <span v-else class="badge bg-amber-500/10 text-amber-500 border border-amber-500/20 w-max">
                    Unpaid
                  </span>
                </td>
                <td class="py-3.5 px-4 font-medium text-text-secondary">
                  £{{ (getPeriodStatus(player)?.expectedAmount ?? summary?.subAmount ?? 0).toFixed(2) }}
                </td>
                <td class="py-3.5 px-4 text-text-muted">
                  <template v-if="getPeriodStatus(player)?.paidDate">
                    {{ new Date(getPeriodStatus(player)!.paidDate!).toLocaleDateString() }}
                  </template>
                  <template v-else>-</template>
                </td>
                <td class="py-3.5 px-4 text-text-muted">
                  {{ getPeriodStatus(player)?.method || '-' }}
                </td>
                <td class="py-3.5 px-4 text-right">
                  <button
                    v-if="!getPeriodStatus(player)?.isPaid"
                    @click="openSubModal(player)"
                    class="btn-primary text-xs py-1 px-3"
                  >
                    Mark Paid
                  </button>
                  <div v-else class="flex items-center justify-end gap-2">
                    <span class="text-xs text-celtic-green font-medium">£{{ getPeriodStatus(player)?.paidAmount?.toFixed(2) }}</span>
                    <button
                      @click="revokeSubPayment(getPeriodStatus(player)!.paymentId!)"
                      class="text-xs text-danger hover:underline ml-2"
                    >
                      Undo
                    </button>
                  </div>
                </td>
              </tr>

              <tr v-if="filteredPlayers.length === 0">
                <td colspan="6" class="py-8 text-center text-text-muted">
                  No squad players found matching query.
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <!-- Tab Content: Expenses -->
      <div v-if="activeTab === 'expenses'" class="p-6 space-y-6">
        <div class="flex flex-col sm:flex-row items-center justify-between gap-4">
          <div class="flex items-center gap-3 w-full sm:w-auto">
            <span class="text-sm font-medium text-text-secondary">Category:</span>
            <select v-model="selectedCategoryFilter" class="input py-1.5 text-sm bg-surface">
              <option value="All">All Categories</option>
              <option value="PitchHire">Pitch Hire</option>
              <option value="Kit">Kit</option>
              <option value="Equipment">Equipment</option>
              <option value="Referee">Referee</option>
              <option value="Tournament">Tournament</option>
              <option value="Other">Other</option>
            </select>
          </div>
          <p class="text-sm text-text-secondary">
            Showing {{ filteredExpenses.length }} expense records
          </p>
        </div>

        <div v-if="loading" class="flex justify-center py-12">
          <div class="animate-spin w-8 h-8 rounded-full border-4 border-celtic-green border-t-transparent"></div>
        </div>

        <!-- Expenses Table -->
        <div v-else class="overflow-x-auto">
          <table class="w-full text-left border-collapse">
            <thead>
              <tr class="border-b border-border bg-surface-hover text-xs font-semibold uppercase text-text-muted">
                <th class="py-3 px-4">Date</th>
                <th class="py-3 px-4">Category</th>
                <th class="py-3 px-4">Description</th>
                <th class="py-3 px-4">Amount</th>
                <th class="py-3 px-4">Paid By</th>
                <th class="py-3 px-4 text-right">Actions</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-border text-sm">
              <tr v-for="expense in filteredExpenses" :key="expense.id" class="hover:bg-surface-hover/50 transition-colors">
                <td class="py-3.5 px-4 text-text-secondary">
                  {{ new Date(expense.date).toLocaleDateString() }}
                </td>
                <td class="py-3.5 px-4">
                  <span class="badge badge-success bg-surface-light text-text-primary border-border">
                    {{ expense.category }}
                  </span>
                </td>
                <td class="py-3.5 px-4 font-semibold text-text-primary">
                  {{ expense.description }}
                  <p v-if="expense.notes" class="text-xs font-normal text-text-muted mt-0.5">{{ expense.notes }}</p>
                </td>
                <td class="py-3.5 px-4 font-bold text-danger">
                  £{{ expense.amount.toFixed(2) }}
                </td>
                <td class="py-3.5 px-4 text-text-muted">
                  {{ expense.paidBy || '-' }}
                </td>
                <td class="py-3.5 px-4 text-right space-x-2">
                  <button @click="openExpenseModal(expense)" class="text-xs text-text-secondary hover:text-celtic-green font-medium">Edit</button>
                  <button @click="confirmDeleteExpense(expense.id)" class="text-xs text-danger hover:underline">Delete</button>
                </td>
              </tr>

              <tr v-if="filteredExpenses.length === 0">
                <td colspan="6" class="py-12 text-center text-text-muted">
                  No outgoing expense records found for this season.
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>

    <!-- Record Sub Payment Modal -->
    <div v-if="isSubModalOpen" class="fixed inset-0 z-[100] flex items-center justify-center bg-black/60 backdrop-blur-sm p-4">
      <div class="card w-full max-w-md p-6 animate-fade-in shadow-2xl border-celtic-green/30">
        <h2 class="text-xl font-bold text-text-primary mb-1">Mark Sub Paid</h2>
        <p class="text-xs text-text-secondary mb-6">Record monthly sub received for {{ subForm.playerName }}</p>

        <form @submit.prevent="submitSubForm" class="space-y-4">
          <div>
            <label class="block text-sm font-medium text-text-secondary mb-1">Amount (£) *</label>
            <input v-model.number="subForm.amount" type="number" step="0.01" min="0" class="input" required />
          </div>

          <div>
            <label class="block text-sm font-medium text-text-secondary mb-1">Paid Date *</label>
            <input v-model="subForm.paidDate" type="date" class="input" required />
          </div>

          <div>
            <label class="block text-sm font-medium text-text-secondary mb-1">Payment Method *</label>
            <select v-model="subForm.method" class="input bg-surface-light text-text-primary" required>
              <option value="BankTransfer">Bank Transfer</option>
              <option value="Cash">Cash</option>
              <option value="StandingOrder">Standing Order</option>
              <option value="Other">Other</option>
            </select>
          </div>

          <div>
            <label class="block text-sm font-medium text-text-secondary mb-1">Notes (Optional)</label>
            <input v-model="subForm.notes" type="text" class="input" placeholder="e.g. Reference string or receipt notes" />
          </div>

          <div v-if="subFormError" class="text-danger text-sm mt-2">
            {{ subFormError }}
          </div>

          <div class="flex justify-end gap-3 mt-6 pt-4 border-t border-border">
            <button type="button" @click="isSubModalOpen = false" class="btn-secondary">Cancel</button>
            <button type="submit" class="btn-primary" :disabled="subSaving">
              {{ subSaving ? 'Saving...' : 'Save Payment' }}
            </button>
          </div>
        </form>
      </div>
    </div>

    <!-- Expense Modal -->
    <div v-if="isExpenseModalOpen" class="fixed inset-0 z-[100] flex items-center justify-center bg-black/60 backdrop-blur-sm p-4">
      <div class="card w-full max-w-md p-6 animate-fade-in shadow-2xl border-celtic-green/30">
        <h2 class="text-xl font-bold text-text-primary mb-1">
          {{ editingExpense ? 'Edit Expense' : 'Add Outgoing Expense' }}
        </h2>
        <p class="text-xs text-text-secondary mb-6">Enter details for one-off team expenditure</p>

        <form @submit.prevent="submitExpenseForm" class="space-y-4">
          <div>
            <label class="block text-sm font-medium text-text-secondary mb-1">Category *</label>
            <select v-model="expenseForm.category" class="input bg-surface-light text-text-primary" required>
              <option value="PitchHire">Pitch Hire</option>
              <option value="Kit">Kit</option>
              <option value="Equipment">Equipment</option>
              <option value="Referee">Referee</option>
              <option value="Tournament">Tournament</option>
              <option value="Other">Other</option>
            </select>
          </div>

          <div>
            <label class="block text-sm font-medium text-text-secondary mb-1">Description *</label>
            <input v-model="expenseForm.description" type="text" placeholder="e.g. Astro pitch booking for Aug 12" class="input" required />
          </div>

          <div class="grid grid-cols-2 gap-4">
            <div>
              <label class="block text-sm font-medium text-text-secondary mb-1">Amount (£) *</label>
              <input v-model.number="expenseForm.amount" type="number" step="0.01" min="0" class="input" required />
            </div>
            <div>
              <label class="block text-sm font-medium text-text-secondary mb-1">Date *</label>
              <input v-model="expenseForm.date" type="date" class="input" required />
            </div>
          </div>

          <div>
            <label class="block text-sm font-medium text-text-secondary mb-1">Paid By (Optional)</label>
            <input v-model="expenseForm.paidBy" type="text" placeholder="e.g. Coach / Treasurer name" class="input" />
          </div>

          <div>
            <label class="block text-sm font-medium text-text-secondary mb-1">Notes (Optional)</label>
            <input v-model="expenseForm.notes" type="text" placeholder="e.g. Invoice #1234" class="input" />
          </div>

          <div v-if="expenseFormError" class="text-danger text-sm mt-2">
            {{ expenseFormError }}
          </div>

          <div class="flex justify-end gap-3 mt-6 pt-4 border-t border-border">
            <button type="button" @click="isExpenseModalOpen = false" class="btn-secondary">Cancel</button>
            <button type="submit" class="btn-primary" :disabled="expenseSaving">
              {{ expenseSaving ? 'Saving...' : 'Save Expense' }}
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useSeasons, type Season } from '~/composables/useSeasons'
import { usePayments, type PlayerSubStatus, type Expense } from '~/composables/usePayments'
import { useTeams } from '~/composables/useTeams'

definePageMeta({
  layout: 'app',
})

useHead({
  title: 'Payments & Finances - Stalybridge Celtic U7',
})

const { seasons, fetchSeasons } = useSeasons()
const { teams, fetchTeams } = useTeams()
const {
  summary,
  playerSubStatuses,
  expenses,
  loading,
  fetchSummary,
  fetchPlayerSubStatuses,
  recordSubPayment,
  deleteSubPayment,
  fetchExpenses,
  createExpense,
  updateExpense,
  deleteExpense,
} = usePayments()

const selectedSeasonId = ref<string>('')
const activeTab = ref<'subs' | 'expenses'>('subs')
const searchQuery = ref('')
const selectedCategoryFilter = ref('All')
const selectedTeamFilter = ref('All')

const selectedSeason = computed(() => seasons.value.find(s => s.id === selectedSeasonId.value))

// Month options based on selected season
const now = new Date()
const availableMonths = computed(() => {
  if (!selectedSeason.value) return []

  const months = []
  const start = new Date(selectedSeason.value.startDate)
  const end = new Date(selectedSeason.value.endDate)

  let cur = new Date(Date.UTC(start.getFullYear(), start.getMonth(), 1))
  const last = new Date(Date.UTC(end.getFullYear(), end.getMonth(), 1))

  while (cur <= last) {
    const year = cur.getUTCFullYear()
    const month = cur.getUTCMonth() + 1
    const val = `${year}-${month.toString().padStart(2, '0')}`
    const label = cur.toLocaleDateString('en-US', { month: 'long', year: 'numeric', timeZone: 'UTC' })
    months.push({ value: val, label })

    cur.setUTCMonth(cur.getUTCMonth() + 1)
  }

  return months
})

const selectedMonth = ref(`${now.getFullYear()}-${(now.getMonth() + 1).toString().padStart(2, '0')}`)

function updateSelectedMonthForSeason() {
  const months = availableMonths.value
  if (months.length === 0) return
  const nowStr = `${now.getFullYear()}-${(now.getMonth() + 1).toString().padStart(2, '0')}`
  const hasNow = months.some(m => m.value === nowStr)
  if (hasNow) {
    selectedMonth.value = nowStr
  } else {
    selectedMonth.value = months[0].value
  }
}

onMounted(async () => {
  fetchTeams()
  await fetchSeasons()
  if (seasons.value.length > 0) {
    const current = seasons.value.find(s => s.isCurrent) || seasons.value[0]
    selectedSeasonId.value = current.id
    updateSelectedMonthForSeason()
    loadData()
  }
})

function onSeasonChange() {
  updateSelectedMonthForSeason()
  loadData()
}

async function loadData() {
  if (!selectedSeasonId.value) return
  await Promise.all([
    fetchSummary(selectedSeasonId.value),
    loadSubStatuses(),
    fetchExpenses(selectedSeasonId.value),
  ])
}

async function loadSubStatuses() {
  if (!selectedSeasonId.value) return
  const [yearStr, monthStr] = selectedMonth.value.split('-')
  await fetchPlayerSubStatuses(selectedSeasonId.value, parseInt(yearStr), parseInt(monthStr))
}

const filteredPlayers = computed(() => {
  let list = playerSubStatuses.value
  if (selectedTeamFilter.value !== 'All') {
    if (selectedTeamFilter.value === 'Unassigned') {
      list = list.filter(p => !p.teamId)
    } else {
      list = list.filter(p => p.teamId === selectedTeamFilter.value)
    }
  }
  if (searchQuery.value.trim()) {
    const q = searchQuery.value.toLowerCase()
    list = list.filter(p => p.playerName.toLowerCase().includes(q))
  }
  return list
})

function getPeriodStatus(player: PlayerSubStatus) {
  return player.periods[0] || null
}

const filteredExpenses = computed(() => {
  if (selectedCategoryFilter.value === 'All') return expenses.value
  return expenses.value.filter(e => e.category === selectedCategoryFilter.value)
})

// --- Sub Payment Modal ---
const isSubModalOpen = ref(false)
const subSaving = ref(false)
const subFormError = ref<string | null>(null)
const subForm = ref({
  playerId: '',
  playerName: '',
  amount: 0,
  paidDate: '',
  periodStart: '',
  periodEnd: '',
  method: 'BankTransfer',
  notes: '',
})

function openSubModal(player: PlayerSubStatus) {
  const period = getPeriodStatus(player)
  const defaultAmount = period?.expectedAmount ?? summary.value?.subAmount ?? 25

  const [y, m] = selectedMonth.value.split('-').map(Number)
  const start = new Date(Date.UTC(y, m - 1, 1))
  const end = new Date(Date.UTC(y, m, 0, 23, 59, 59))

  subForm.value = {
    playerId: player.playerId,
    playerName: player.playerName,
    amount: defaultAmount,
    paidDate: new Date().toISOString().split('T')[0],
    periodStart: start.toISOString(),
    periodEnd: end.toISOString(),
    method: 'BankTransfer',
    notes: '',
  }
  subFormError.value = null
  isSubModalOpen.value = true
}

async function submitSubForm() {
  subSaving.value = true
  subFormError.value = null

  const res = await recordSubPayment({
    playerId: subForm.value.playerId,
    seasonId: selectedSeasonId.value,
    amount: subForm.value.amount,
    paidDate: new Date(subForm.value.paidDate).toISOString(),
    periodStart: subForm.value.periodStart,
    periodEnd: subForm.value.periodEnd,
    method: subForm.value.method,
    notes: subForm.value.notes,
  })

  if (res.success) {
    isSubModalOpen.value = false
    await loadData()
  } else {
    subFormError.value = res.error || 'Failed to record payment'
  }

  subSaving.value = false
}

async function revokeSubPayment(paymentId: string) {
  if (!confirm('Are you sure you want to revoke this payment status?')) return
  const res = await deleteSubPayment(paymentId)
  if (res.success) {
    await loadData()
  }
}

// --- Expense Modal ---
const isExpenseModalOpen = ref(false)
const editingExpense = ref<Expense | null>(null)
const expenseSaving = ref(false)
const expenseFormError = ref<string | null>(null)
const expenseForm = ref({
  category: 'PitchHire',
  description: '',
  amount: 0,
  date: '',
  paidBy: '',
  notes: '',
})

function openExpenseModal(expense?: Expense) {
  if (expense) {
    editingExpense.value = expense
    expenseForm.value = {
      category: expense.category,
      description: expense.description,
      amount: expense.amount,
      date: expense.date.split('T')[0],
      paidBy: expense.paidBy || '',
      notes: expense.notes || '',
    }
  } else {
    editingExpense.value = null
    expenseForm.value = {
      category: 'PitchHire',
      description: '',
      amount: 0,
      date: new Date().toISOString().split('T')[0],
      paidBy: '',
      notes: '',
    }
  }
  expenseFormError.value = null
  isExpenseModalOpen.value = true
}

async function submitExpenseForm() {
  expenseSaving.value = true
  expenseFormError.value = null

  const payload = {
    ...expenseForm.value,
    date: new Date(expenseForm.value.date).toISOString(),
  }

  const res = editingExpense.value
    ? await updateExpense(editingExpense.value.id, payload)
    : await createExpense({ seasonId: selectedSeasonId.value, ...payload })

  if (res.success) {
    isExpenseModalOpen.value = false
    await fetchSummary(selectedSeasonId.value)
  } else {
    expenseFormError.value = res.error || 'Failed to save expense'
  }

  expenseSaving.value = false
}

async function confirmDeleteExpense(expenseId: string) {
  if (!confirm('Are you sure you want to delete this expense record?')) return
  const res = await deleteExpense(expenseId)
  if (res.success) {
    await fetchSummary(selectedSeasonId.value)
  }
}
</script>

<style scoped>
.animate-fade-in {
  animation: fadeIn 0.2s ease-out forwards;
}
@keyframes fadeIn {
  from { opacity: 0; transform: scale(0.95); }
  to { opacity: 1; transform: scale(1); }
}
</style>
