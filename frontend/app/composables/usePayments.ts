import { ref } from 'vue'
import { useAuth } from './useAuth'

export interface FinancialSummary {
  seasonId: string
  seasonName: string
  subAmount: number
  subFrequency: string
  totalIncome: number
  totalExpenses: number
  netBalance: number
  activePlayersCount: number
  upToDatePlayersCount: number
  currentMonthPaidCount: number
  currentMonthTotalPlayers: number
}

export interface SubPeriodStatus {
  periodName: string
  periodStart: string
  periodEnd: string
  isPaid: boolean
  paymentId?: string
  expectedAmount: number
  paidAmount?: number
  paidDate?: string
  method?: string
}

export interface PlayerSubStatus {
  playerId: string
  playerName: string
  isActive: boolean
  periods: SubPeriodStatus[]
  totalPaidThisSeason: number
  totalDueThisSeason: number
  isUpToDate: boolean
}

export interface Expense {
  id: string
  seasonId: string
  category: string
  description: string
  amount: number
  date: string
  paidBy?: string
  notes?: string
}

export function usePayments() {
  const { getAuthHeaders } = useAuth()
  
  const summary = ref<FinancialSummary | null>(null)
  const playerSubStatuses = ref<PlayerSubStatus[]>([])
  const expenses = ref<Expense[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function fetchSummary(seasonId: string) {
    if (!seasonId) return
    loading.value = true
    error.value = null
    try {
      summary.value = await $fetch<FinancialSummary>(`/api/payments/summary?seasonId=${seasonId}`, {
        headers: getAuthHeaders(),
      })
    } catch (err: any) {
      error.value = err?.data?.message || 'Failed to fetch financial summary'
    } finally {
      loading.value = false
    }
  }

  async function fetchPlayerSubStatuses(seasonId: string, year?: number, month?: number) {
    if (!seasonId) return
    loading.value = true
    error.value = null
    try {
      let url = `/api/payments/subs?seasonId=${seasonId}`
      if (year && month) {
        url += `&year=${year}&month=${month}`
      }
      playerSubStatuses.value = await $fetch<PlayerSubStatus[]>(url, {
        headers: getAuthHeaders(),
      })
    } catch (err: any) {
      error.value = err?.data?.message || 'Failed to fetch player sub statuses'
    } finally {
      loading.value = false
    }
  }

  async function recordSubPayment(payload: {
    playerId: string
    seasonId: string
    amount: number
    paidDate: string
    periodStart: string
    periodEnd: string
    method?: string
    notes?: string
  }) {
    try {
      const payment = await $fetch('/api/payments/subs', {
        method: 'POST',
        headers: getAuthHeaders(),
        body: payload,
      })
      return { success: true, payment }
    } catch (err: any) {
      return { success: false, error: err?.data?.message || 'Failed to record payment' }
    }
  }

  async function deleteSubPayment(id: string) {
    try {
      await $fetch(`/api/payments/subs/${id}`, {
        method: 'DELETE',
        headers: getAuthHeaders(),
      })
      return { success: true }
    } catch (err: any) {
      return { success: false, error: err?.data?.message || 'Failed to delete payment' }
    }
  }

  async function fetchExpenses(seasonId: string) {
    if (!seasonId) return
    loading.value = true
    error.value = null
    try {
      expenses.value = await $fetch<Expense[]>(`/api/payments/expenses?seasonId=${seasonId}`, {
        headers: getAuthHeaders(),
      })
    } catch (err: any) {
      error.value = err?.data?.message || 'Failed to fetch expenses'
    } finally {
      loading.value = false
    }
  }

  async function createExpense(payload: {
    seasonId: string
    category: string
    description: string
    amount: number
    date: string
    paidBy?: string
    notes?: string
  }) {
    try {
      const expense = await $fetch<Expense>('/api/payments/expenses', {
        method: 'POST',
        headers: getAuthHeaders(),
        body: payload,
      })
      expenses.value.unshift(expense)
      return { success: true, expense }
    } catch (err: any) {
      return { success: false, error: err?.data?.message || 'Failed to create expense' }
    }
  }

  async function updateExpense(id: string, payload: {
    category: string
    description: string
    amount: number
    date: string
    paidBy?: string
    notes?: string
  }) {
    try {
      const updated = await $fetch<Expense>(`/api/payments/expenses/${id}`, {
        method: 'PUT',
        headers: getAuthHeaders(),
        body: payload,
      })
      const idx = expenses.value.findIndex(e => e.id === id)
      if (idx !== -1) {
        expenses.value[idx] = updated
      }
      return { success: true, expense: updated }
    } catch (err: any) {
      return { success: false, error: err?.data?.message || 'Failed to update expense' }
    }
  }

  async function deleteExpense(id: string) {
    try {
      await $fetch(`/api/payments/expenses/${id}`, {
        method: 'DELETE',
        headers: getAuthHeaders(),
      })
      expenses.value = expenses.value.filter(e => e.id !== id)
      return { success: true }
    } catch (err: any) {
      return { success: false, error: err?.data?.message || 'Failed to delete expense' }
    }
  }

  return {
    summary,
    playerSubStatuses,
    expenses,
    loading,
    error,
    fetchSummary,
    fetchPlayerSubStatuses,
    recordSubPayment,
    deleteSubPayment,
    fetchExpenses,
    createExpense,
    updateExpense,
    deleteExpense,
  }
}
