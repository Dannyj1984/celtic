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

const mockTeams = ref([
  { id: 't1', name: 'Stripes', colorHex: '#006837', isActive: true, playersCount: 1 },
  { id: 't2', name: 'Hoops', colorHex: '#F59E0B', isActive: true, playersCount: 1 }
])

vi.mock('~/composables/useTeams', () => ({
  useTeams: () => ({
    teams: mockTeams,
    loading: ref(false),
    error: ref(null),
    fetchTeams: vi.fn(),
    createTeam: vi.fn(),
    updateTeam: vi.fn(),
    deleteTeam: vi.fn()
  })
}))

const mockPlayers = ref([
  {
    id: 1,
    firstName: 'John',
    lastName: 'Terry',
    isActive: true,
    dateOfBirth: '2015-05-10',
    teams: [
      { id: 't1', name: 'Stripes', colorHex: '#006837' },
      { id: 't2', name: 'Hoops', colorHex: '#F59E0B' }
    ],
    teamIds: ['t1', 't2'],
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

  it('should display multiple team badges for players assigned to multiple teams', () => {
    const wrapper = mount(SquadManagement)
    expect(wrapper.text()).toContain('Stripes')
    expect(wrapper.text()).toContain('Hoops')
  })

  it('supports selecting multiple teams when creating a player', async () => {
    const wrapper = mount(SquadManagement)

    // Open modal
    await wrapper.find('button.btn-primary').trigger('click')

    // Click both team buttons to select them
    const teamButtons = wrapper.findAll('form button[type="button"]')
    // First 2 buttons correspond to Stripes and Hoops
    await teamButtons[0].trigger('click')
    await teamButtons[1].trigger('click')

    const inputs = wrapper.findAll('input')
    await inputs[0].setValue('Cole')
    await inputs[1].setValue('Palmer')

    // Submit
    await wrapper.find('form').trigger('submit.prevent')

    expect(mockCreatePlayer).toHaveBeenCalledWith(expect.objectContaining({
      firstName: 'Cole',
      lastName: 'Palmer',
      teamIds: ['t1', 't2'],
      teamId: 't1'
    }))
  })
})
