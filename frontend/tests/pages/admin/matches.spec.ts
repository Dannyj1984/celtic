import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount } from '@vue/test-utils'
import { ref } from 'vue'

vi.stubGlobal('useHead', vi.fn())
vi.stubGlobal('definePageMeta', vi.fn())
vi.stubGlobal('useCookie', vi.fn(() => ref(null)))
vi.stubGlobal('useState', vi.fn((_key, init) => ref(init ? init() : null)))
vi.stubGlobal('$fetch', vi.fn())

const mockMatches = ref([
  { id: 'm1', date: '2026-05-01T15:00:00Z', opposition: 'Rangers FC', location: 'Home', goalsFor: 3, goalsAgainst: 1, result: 'Win', isPublished: true, seasonId: 's1', seasonName: '2025-26', matchReport: null, playerOfTheMatchId: null, playerOfTheMatchName: null }
])
const mockFetchMatches = vi.fn()
const mockCreateMatch = vi.fn(() => Promise.resolve({ success: true }))
const mockUpdateMatch = vi.fn(() => Promise.resolve({ success: true }))
const mockDeleteMatch = vi.fn(() => Promise.resolve({ success: true }))

vi.mock('~/composables/useMatches', () => ({
  useMatches: () => ({ matches: mockMatches, loading: ref(false), error: ref(null), fetchMatches: mockFetchMatches, createMatch: mockCreateMatch, updateMatch: mockUpdateMatch, deleteMatch: mockDeleteMatch })
}))

vi.mock('~/composables/useSeasons', () => ({
  useSeasons: () => ({ seasons: ref([{ id: 's1', name: '2025-26' }]), fetchSeasons: vi.fn() })
}))

vi.mock('~/composables/usePlayers', () => ({
  usePlayers: () => ({ players: ref([{ id: 'p1', firstName: 'John', lastName: 'Terry' }]), fetchPlayers: vi.fn() })
}))

import MatchesPage from '~/pages/admin/matches.vue'

describe('Admin Matches Page', () => {
  beforeEach(() => { vi.clearAllMocks() })

  it('renders page title and matches', () => {
    const wrapper = mount(MatchesPage)
    expect(wrapper.text()).toContain('Matches')
    expect(wrapper.text()).toContain('Rangers FC')
  })

  it('shows score and result for past matches', () => {
    const wrapper = mount(MatchesPage)
    expect(wrapper.text()).toContain('3 - 1')
    expect(wrapper.text()).toContain('Win')
  })

  it('renders Add Match button', () => {
    const wrapper = mount(MatchesPage)
    expect(wrapper.text()).toContain('+ Add Match')
  })

  it('opens create modal on button click', async () => {
    const wrapper = mount(MatchesPage)
    await wrapper.find('button.btn-primary').trigger('click')
    expect(wrapper.text()).toContain('Add New Match')
    expect(wrapper.find('form').exists()).toBe(true)
  })

  it('shows empty state when no matches', () => {
    mockMatches.value = []
    const wrapper = mount(MatchesPage)
    expect(wrapper.text()).toContain('No matches scheduled')
  })
})
