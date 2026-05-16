import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount } from '@vue/test-utils'
import { ref } from 'vue'
import SquadManagement from '~/pages/admin/players.vue'

// Mocking Nuxt and Auth composables
vi.stubGlobal('useHead', vi.fn())
vi.stubGlobal('definePageMeta', vi.fn())
vi.stubGlobal('$fetch', vi.fn())

vi.mock('~/composables/useAuth', () => ({
  useAuth: () => ({
    getAuthHeaders: vi.fn(() => ({}))
  })
}))

const mockPlayers = ref([
  {
    id: 1,
    firstName: 'John',
    lastName: 'Terry',
    isActive: true,
    dateOfBirth: '2015-05-10',
    attendance: {
      trainingAttended: 8,
      trainingTotal: 10,
      matchAttended: 4,
      matchTotal: 6
    }
  }
])

const mockFetchPlayers = vi.fn()
const mockCreatePlayer = vi.fn(() => Promise.resolve({ success: true }))
const mockUpdatePlayer = vi.fn(() => Promise.resolve({ success: true }))

vi.mock('~/composables/usePlayers', () => ({
  usePlayers: () => ({
    players: mockPlayers,
    loading: ref(false),
    error: ref(null),
    fetchPlayers: mockFetchPlayers,
    createPlayer: mockCreatePlayer,
    updatePlayer: mockUpdatePlayer
  })
}))

describe('SquadManagement', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('renders correctly and shows players', () => {
    const wrapper = mount(SquadManagement)
    expect(wrapper.text()).toContain('Squad Management')
    expect(wrapper.text()).toContain('John Terry')
  })

  it('opens create modal when clicking add button', async () => {
    const wrapper = mount(SquadManagement)
    const addButton = wrapper.find('button.btn-primary')
    await addButton.trigger('click')

    expect(wrapper.text()).toContain('Add New Player')
    expect(wrapper.find('form').exists()).toBe(true)
  })

  it('submits the form to create a player', async () => {
    const wrapper = mount(SquadManagement)

    // Open modal
    await wrapper.find('button.btn-primary').trigger('click')

    // Fill form
    const inputs = wrapper.findAll('input')
    await inputs[0].setValue('Frank') // First Name
    await inputs[1].setValue('Lampard') // Last Name

    // Submit
    await wrapper.find('form').trigger('submit.prevent')

    expect(mockCreatePlayer).toHaveBeenCalledWith(expect.objectContaining({
      firstName: 'Frank',
      lastName: 'Lampard'
    }))
  })

  it('should show the number of training sessions attended for each player', () => {
    const wrapper = mount(SquadManagement)
    const attendanceSection = wrapper.find('[data-testid="attendance-training"]')
    expect(attendanceSection.text()).toContain('8 / 10')
  })

  it('should show the number of match sessions attended for each player', () => {
    const wrapper = mount(SquadManagement)
    const attendanceSection = wrapper.find('[data-testid="attendance-match"]')
    expect(attendanceSection.text()).toContain('4 / 6')
  })
})
