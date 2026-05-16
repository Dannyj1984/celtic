import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount } from '@vue/test-utils'
import { ref } from 'vue'

vi.stubGlobal('useHead', vi.fn())
vi.stubGlobal('definePageMeta', vi.fn())
vi.stubGlobal('$fetch', vi.fn())

const mockSeasons = ref([
  { id: 's1', name: '2025-26', startDate: '2025-09-01T00:00:00Z', endDate: '2026-06-30T00:00:00Z', subAmount: 25, subFrequency: 'Monthly', isCurrent: true },
  { id: 's2', name: '2024-25', startDate: '2024-09-01T00:00:00Z', endDate: '2025-06-30T00:00:00Z', subAmount: 20, subFrequency: 'Weekly', isCurrent: false }
])
const mockFetchSeasons = vi.fn()
const mockCreateSeason = vi.fn(() => Promise.resolve({ success: true }))
const mockUpdateSeason = vi.fn(() => Promise.resolve({ success: true }))

vi.mock('~/composables/useSeasons', () => ({
  useSeasons: () => ({ seasons: mockSeasons, loading: ref(false), error: ref(null), fetchSeasons: mockFetchSeasons, createSeason: mockCreateSeason, updateSeason: mockUpdateSeason })
}))

import SeasonsPage from '~/pages/admin/seasons.vue'

describe('Admin Seasons Page', () => {
  beforeEach(() => { vi.clearAllMocks() })

  it('renders page title and seasons', () => {
    const wrapper = mount(SeasonsPage)
    expect(wrapper.text()).toContain('Season Settings')
    expect(wrapper.text()).toContain('2025-26')
    expect(wrapper.text()).toContain('2024-25')
  })

  it('shows Current Season badge for current season', () => {
    const wrapper = mount(SeasonsPage)
    expect(wrapper.text()).toContain('Current Season')
  })

  it('shows subscription amount and frequency', () => {
    const wrapper = mount(SeasonsPage)
    expect(wrapper.text()).toContain('£25.00')
    expect(wrapper.text()).toContain('monthly')
  })

  it('opens create modal on button click', async () => {
    const wrapper = mount(SeasonsPage)
    await wrapper.find('button.btn-primary').trigger('click')
    expect(wrapper.text()).toContain('Create Season')
    expect(wrapper.find('form').exists()).toBe(true)
  })

  it('shows empty state when no seasons', () => {
    mockSeasons.value = []
    const wrapper = mount(SeasonsPage)
    expect(wrapper.text()).toContain('No seasons configured')
  })
})
