import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount } from '@vue/test-utils'
import { ref } from 'vue'

vi.stubGlobal('useHead', vi.fn())
vi.stubGlobal('definePageMeta', vi.fn())
vi.stubGlobal('$fetch', vi.fn())

const futureDate = new Date(Date.now() + 86400000 * 7).toISOString()
const pastDate = new Date(Date.now() - 86400000 * 7).toISOString()

const mockEvents = ref([
  { id: 'e1', type: 'Training', dateTime: futureDate, location: 'Pitch 1', isCancelled: false, notes: null, attendingPlayers: [{ playerId: 'p1', fullName: 'John Terry' }] },
  { id: 'e2', type: 'Match', dateTime: pastDate, location: 'Away Ground', isCancelled: true, notes: 'Cancelled due to weather', attendingPlayers: [], seasonName: '2025-26' }
])
const mockFetchEvents = vi.fn()
const mockCreateEvent = vi.fn(() => Promise.resolve({ success: true }))
const mockUpdateEvent = vi.fn(() => Promise.resolve({ success: true }))
const mockDeleteEvent = vi.fn(() => Promise.resolve({ success: true }))

vi.mock('~/composables/useEvents', () => ({
  useEvents: () => ({ events: mockEvents, loading: ref(false), error: ref(null), fetchEvents: mockFetchEvents, createEvent: mockCreateEvent, updateEvent: mockUpdateEvent, deleteEvent: mockDeleteEvent })
}))

import SchedulePage from '~/pages/admin/schedule.vue'

describe('Admin Schedule Page', () => {
  beforeEach(() => { vi.clearAllMocks() })

  const stubs = { UIcon: true, NuxtLink: { template: '<a><slot /></a>' } }

  it('renders page title', () => {
    const wrapper = mount(SchedulePage, { global: { stubs } })
    expect(wrapper.text()).toContain('Schedule')
  })

  it('shows attendance count', () => {
    const wrapper = mount(SchedulePage, { global: { stubs } })
    expect(wrapper.text()).toContain('1 Attending')
  })

  it('opens create modal on button click', async () => {
    const wrapper = mount(SchedulePage, { global: { stubs } })
    await wrapper.find('button.btn-secondary').trigger('click')
    expect(wrapper.text()).toContain('Add One-off Event')
    expect(wrapper.find('form').exists()).toBe(true)
  })
})
