import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount } from '@vue/test-utils'
import SeasonPage from '../../app/pages/season.vue'
import { ref } from 'vue'

// Mock Nuxt composables
vi.stubGlobal('useHead', vi.fn())
vi.stubGlobal('definePageMeta', vi.fn())

vi.stubGlobal('useAuth', vi.fn(() => ({
  getAuthHeaders: vi.fn(() => ({}))
})))

vi.stubGlobal('useCalendar', vi.fn(() => ({
  downloadIcs: vi.fn(),
  openGoogleCalendar: vi.fn()
})))

const mockUpcoming = [
  {
    id: 'e1',
    type: 'Match',
    dateTime: '2026-10-15T09:00:00Z',
    location: 'Pitch 1',
    opposition: 'Hyde United',
    status: 'Attending',
    played: true,
    teamId: 't1',
    teamName: 'Blues'
  },
  {
    id: 'e2',
    type: 'Match',
    dateTime: '2026-10-22T10:00:00Z',
    location: 'Pitch 2',
    opposition: 'Glossop North End',
    status: 'No Response',
    played: false,
    teamId: null,
    teamName: null
  }
]

const mockPast = [
  {
    id: 'e3',
    type: 'Match',
    dateTime: '2026-09-01T09:00:00Z',
    location: 'Pitch 1',
    opposition: 'Mossley AFC',
    status: 'Attending',
    played: true,
    result: 'Win',
    score: '4 - 2',
    teamId: 't2',
    teamName: 'Whites',
    playerOfTheMatchName: 'Leo Messi',
    matchReport: 'Great team performance!'
  }
]

describe('Season Fixtures Page', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    vi.stubGlobal('$fetch', vi.fn((url: string) => {
      if (url.includes('/upcoming/Match')) return Promise.resolve(mockUpcoming)
      if (url.includes('/past/Match')) return Promise.resolve(mockPast)
      return Promise.resolve([])
    }))
  })

  it('renders season fixtures and displays team badges', async () => {
    const wrapper = mount(SeasonPage, {
      global: {
        stubs: {
          NuxtLink: { template: '<a><slot /></a>' },
          UIcon: { template: '<span></span>' },
          UBadge: { template: '<span><slot /></span>' },
          UDropdown: { template: '<div><slot /></div>' },
          CalendarDaysIcon: { template: '<span></span>' },
          MapPinIcon: { template: '<span></span>' },
          CheckCircleIcon: { template: '<span></span>' }
        }
      }
    })

    // Wait for fixtures to load
    await wrapper.vm.$nextTick()
    await new Promise(resolve => setTimeout(resolve, 50))
    await wrapper.vm.$nextTick()

    // Title and subtitle
    expect(wrapper.text()).toContain('Season Fixtures')
    expect(wrapper.text()).toContain('Hyde United')
    expect(wrapper.text()).toContain('Glossop North End')

    // Team name / badge for Blues
    expect(wrapper.text()).toContain('Blues')
  })
})
