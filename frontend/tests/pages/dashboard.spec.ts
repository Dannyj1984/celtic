import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount } from '@vue/test-utils'
import Dashboard from '../../app/pages/dashboard.vue'
import { ref } from 'vue'

// Mocking Nuxt and Auth composables
vi.stubGlobal('useHead', vi.fn())
vi.stubGlobal('definePageMeta', vi.fn())

vi.stubGlobal('useAuth', vi.fn(() => ({
  user: ref({ fullName: 'Alex', email: 'parent@test.com' }),
  isAdmin: ref(false),
  logout: vi.fn(),
  getAuthHeaders: vi.fn(() => ({}))
})))

vi.stubGlobal('useToast', vi.fn(() => ({
  add: vi.fn()
})))

const mockDashboardData = ref({
  parentName: 'Alex',
  playerName: 'Leo Messi',
  subscriptionStatus: 'Active',
  nextSubPaymentDate: '2026-10-15T00:00:00Z',
  nextMatch: {
    id: '123',
    date: '2026-05-01T15:00:00Z',
    opposition: 'JNR Tigers',
    location: 'Riverside Pitch 4'
  },
  trainingSchedule: {
    day: 'Wednesday',
    startTime: '17:30',
    endTime: '19:00',
    location: 'Riverside Sports Complex'
  },
  performance: {
    totalRecentSessions: 10,
    attendedSessions: 9
  },
  coachWhatsAppNumber: '1234567890'
})

vi.stubGlobal('useFetch', vi.fn(() => ({
  data: mockDashboardData,
  pending: ref(false),
  error: ref(null)
})))

describe('Dashboard Page', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('renders dashboard content correctly', () => {
    const wrapper = mount(Dashboard, {
      global: {
        stubs: {
          NuxtLink: { template: '<a><slot /></a>' },
          UIcon: { template: '<span></span>' },
          UButton: { template: '<button><slot /></button>' },
          UBadge: { template: '<span><slot /></span>' },
          UCard: { template: '<div><slot /></div>' },
          UProgress: { template: '<div><slot /></div>' },
          UMeter: { template: '<div><slot /></div>' }
        }
      }
    })

    // Check Welcome section
    expect(wrapper.text()).toContain('Welcome, Alex')
    expect(wrapper.text()).toContain('Leo Messi')

    // Check Subscription Status
    expect(wrapper.text()).toContain('Subscription Status')
    expect(wrapper.text()).toContain('Active')

    // Check Upcoming Activities
    expect(wrapper.text()).toContain('Upcoming Activities')
    expect(wrapper.text()).toContain('JNR Tigers')

    // Check Training
    expect(wrapper.text()).toContain('Weekly Training')
    expect(wrapper.text()).toContain('Wednesday')
    expect(wrapper.text()).toContain('17:30')

    // Check Performance
    expect(wrapper.text()).toContain('Season Performance')
    expect(wrapper.text()).toContain('90%')
  })
})
