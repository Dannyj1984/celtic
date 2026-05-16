import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount } from '@vue/test-utils'
import Dashboard from '../../app/pages/dashboard.vue'
import { ref } from 'vue'

// Mocking Nuxt and Auth composables
vi.stubGlobal('useHead', vi.fn())
vi.stubGlobal('definePageMeta', vi.fn())
vi.stubGlobal('useRuntimeConfig', vi.fn(() => ({ public: { vapidPublicKey: 'test-key' } })))

vi.mock('~/composables/useNotifications', () => ({
  useNotifications: () => ({
    isSupported: ref(true),
    isSubscribed: ref(false),
    loading: ref(false),
    checkSubscription: vi.fn(),
    subscribe: vi.fn(),
    unsubscribe: vi.fn()
  })
}))

const isAdminMock = ref(false)

vi.stubGlobal('useAuth', vi.fn(() => ({
  user: ref({ fullName: 'Alex', email: 'parent@test.com' }),
  isAdmin: isAdminMock,
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
    location: 'Riverside Sports Complex',
    goodToKnow: 'Some important info'
  },
  performance: {
    training: {
      totalSessions: 10,
      attendedSessions: 9,
      percentage: 90
    },
    matches: {
      totalSessions: 10,
      attendedSessions: 9,
      percentage: 90
    }
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
    isAdminMock.value = false
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
          UMeter: { template: '<div><slot /></div>' },
          UModal: { template: '<div><slot /></div>' },
          UFormGroup: { template: '<div><slot /></div>' },
          UInput: { template: '<input />' }
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

    // Check Good to know
    expect(wrapper.text()).toContain('Good to Know')
    expect(wrapper.text()).toContain('Some important info')
  })
  it('renders Change Password button for non admin users', () => {
    const wrapper = mount(Dashboard, {
      global: {
        stubs: {
          NuxtLink: { template: '<a><slot /></a>' },
          UIcon: { template: '<span></span>' },
          UButton: { template: '<button><slot /></button>' },
          UBadge: { template: '<span><slot /></span>' },
          UCard: { template: '<div><slot /></div>' },
          UProgress: { template: '<div><slot /></div>' },
          UMeter: { template: '<div><slot /></div>' },
          UModal: { template: '<div><slot /></div>' },
          UFormGroup: { template: '<div><slot /></div>' },
          UInput: { template: '<input />' }
        }
      }
    })
    // Check Change Password button
    expect(wrapper.text()).toContain('Change Password')
  })
  it('does not render the Change Password button for admin users', () => {
    isAdminMock.value = true
    const wrapper = mount(Dashboard, {
      global: {
        stubs: {
          NuxtLink: { template: '<a><slot /></a>' },
          UIcon: { template: '<span></span>' },
          UButton: { template: '<button><slot /></button>' },
          UBadge: { template: '<span><slot /></span>' },
          UCard: { template: '<div><slot /></div>' },
          UProgress: { template: '<div><slot /></div>' },
          UMeter: { template: '<div><slot /></div>' },
          UModal: { template: '<div><slot /></div>' },
          UFormGroup: { template: '<div><slot /></div>' },
          UInput: { template: '<input />' }
        }
      }
    })
    // Check Change Password button
    expect(wrapper.text()).not.toContain('Change Password')
  })
  it('opens the change password modal when the Change Password button is clicked', async () => {
    const wrapper = mount(Dashboard, {
      global: {
        stubs: {
          NuxtLink: { template: '<a><slot /></a>' },
          UIcon: { template: '<span></span>' },
          UButton: { template: '<button><slot /></button>' },
          UBadge: { template: '<span><slot /></span>' },
          UCard: { template: '<div><slot /></div>' },
          UProgress: { template: '<div><slot /></div>' },
          UMeter: { template: '<div><slot /></div>' },
          UModal: { template: '<div><slot /></div>' },
          UFormGroup: { template: '<div><slot /></div>' },
          UInput: { template: '<input />' }
        }
      }
    })
    // Check Change Password button
    await wrapper.find('button').trigger('click')
    const changePasswordForm = wrapper.find('[data-testid="change-password-form"]')
    expect(changePasswordForm).toBeTruthy()
  })
})
