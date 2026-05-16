import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount } from '@vue/test-utils'
import { ref } from 'vue'

vi.stubGlobal('useHead', vi.fn())
vi.stubGlobal('definePageMeta', vi.fn())
vi.stubGlobal('$fetch', vi.fn())
vi.stubGlobal('useAuth', vi.fn(() => ({ getAuthHeaders: vi.fn(() => ({})) })))

const mockParents = ref([
  { userId: 'u1', fullName: 'Jane Doe', email: 'jane@test.com', phone: '07123456789', role: 'Parent', children: [{ playerId: 'p1', firstName: 'Kid', lastName: 'One', relationship: 'Mother', subscriptionStatus: 'Active' }] },
  { userId: 'u2', fullName: 'Bob Smith', email: 'bob@test.com', phone: null, role: 'Parent', children: [] }
])

vi.mock('~/composables/useParents', () => ({
  useParents: () => ({ parents: mockParents, loading: ref(false), error: ref(null), fetchParents: vi.fn(), linkPlayer: vi.fn(() => Promise.resolve({ success: true })) })
}))

vi.mock('~/composables/usePlayers', () => ({
  usePlayers: () => ({ players: ref([{ id: 'p2', firstName: 'Player', lastName: 'Two' }]), fetchPlayers: vi.fn() })
}))

vi.mock('~/composables/useAuth', () => ({
  useAuth: () => ({ getAuthHeaders: () => ({}) })
}))

import ParentsPage from '~/pages/admin/parents.vue'

describe('Admin Parents Page', () => {
  beforeEach(() => { vi.clearAllMocks() })

  it('renders page title and parent names', () => {
    const wrapper = mount(ParentsPage)
    expect(wrapper.text()).toContain('Parent Accounts')
    expect(wrapper.text()).toContain('Jane Doe')
    expect(wrapper.text()).toContain('Bob Smith')
  })

  it('shows parent email', () => {
    const wrapper = mount(ParentsPage)
    expect(wrapper.text()).toContain('jane@test.com')
  })

  it('shows linked children', () => {
    const wrapper = mount(ParentsPage)
    expect(wrapper.text()).toContain('Kid One')
    expect(wrapper.text()).toContain('Mother')
  })

  it('shows subscription status for children', () => {
    const wrapper = mount(ParentsPage)
    expect(wrapper.text()).toContain('Active')
  })

  it('shows empty state message for parents with no linked players', () => {
    const wrapper = mount(ParentsPage)
    expect(wrapper.text()).toContain('No players linked yet')
  })

  it('opens create account modal', async () => {
    const wrapper = mount(ParentsPage)
    await wrapper.find('button.btn-primary').trigger('click')
    expect(wrapper.text()).toContain('Create Parent Account')
  })
})
