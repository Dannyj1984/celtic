import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount } from '@vue/test-utils'
import { ref } from 'vue'

vi.stubGlobal('useHead', vi.fn())
vi.stubGlobal('definePageMeta', vi.fn())
vi.stubGlobal('useCookie', vi.fn(() => ref(null)))
vi.stubGlobal('$fetch', vi.fn())
vi.stubGlobal('useToast', vi.fn(() => ({ add: vi.fn() })))

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

vi.mock('~/composables/usePlayers', () => ({
  usePlayers: () => ({ players: ref([]), fetchPlayers: vi.fn() })
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

  it('opens custom delete modal and deletes session on confirm', async () => {
    const wrapper = mount(SchedulePage, { global: { stubs } })
    const deleteBtn = wrapper.find('button[title="Delete Event"]')
    expect(deleteBtn.exists()).toBe(true)
    await deleteBtn.trigger('click')
    expect(wrapper.text()).toContain('Delete Training Session?')
    expect(wrapper.text()).toContain('This action cannot be undone')
    
    const confirmDeleteBtn = wrapper.findAll('button').find(b => b.text().includes('Delete Session'))
    expect(confirmDeleteBtn?.exists()).toBe(true)
    await confirmDeleteBtn!.trigger('click')
    expect(mockDeleteEvent).toHaveBeenCalledWith('e1')
  })

  it('opens delete modal from edit event modal', async () => {
    const wrapper = mount(SchedulePage, { global: { stubs } })
    const editBtn = wrapper.find('button[title="Edit Event"]')
    await editBtn.trigger('click')
    expect(wrapper.text()).toContain('Edit Event')
    const modalDeleteBtn = wrapper.find('button.bg-danger\\/10')
    expect(modalDeleteBtn.exists()).toBe(true)
    await modalDeleteBtn.trigger('click')
    expect(wrapper.text()).toContain('Delete Training Session?')
    const confirmDeleteBtn = wrapper.findAll('button').find(b => b.text().includes('Delete Session'))
    await confirmDeleteBtn!.trigger('click')
    expect(mockDeleteEvent).toHaveBeenCalledWith('e1')
  })
})
