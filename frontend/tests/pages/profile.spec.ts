import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount, flushPromises } from '@vue/test-utils'

vi.stubGlobal('useHead', vi.fn())
vi.stubGlobal('definePageMeta', vi.fn())
vi.stubGlobal('useToast', vi.fn(() => ({ add: vi.fn() })))

const mockGetAuthHeaders = vi.fn(() => ({ Authorization: 'Bearer test-token' }))
vi.stubGlobal('useAuth', () => ({ getAuthHeaders: mockGetAuthHeaders }))

const mockProfileData = {
    playerId: 'p1',
    fullName: 'Leo Messi',
    preferredFoot: 'Left',
    createdYear: 2026,
    joinedYear: 2026,
    matchAttendance: {
        totalSessions: 10,
        attendedSessions: 9
    },
    playerOfTheMatchCount: 3,
    badges: [
        { type: 'PotM', tier: 'Bronze', name: 'Star Player' }
    ],
    recentMatches: [
        {
            id: 'm1',
            date: '2026-05-01T15:00:00Z',
            opposition: 'JNR Tigers',
            result: 'Win',
            score: '4 - 2',
            wasPlayerOfTheMatch: true
        }
    ]
}

const mockFetch = vi.fn()
vi.stubGlobal('$fetch', mockFetch)

import ProfilePage from '~/pages/profile.vue'

describe('Profile Page', () => {
    const stubs = {
        UIcon: true,
        UCard: { template: '<div><slot /></div>' },
        UBadge: { template: '<span><slot /></span>' },
        NuxtLink: { template: '<a><slot /></a>' }
    }

    beforeEach(() => {
        vi.clearAllMocks()
        mockFetch.mockResolvedValue(mockProfileData)
    })

    it('fetches profile data on mount and renders player header', async () => {
        const wrapper = mount(ProfilePage, { global: { stubs } })
        await flushPromises()

        expect(mockFetch).toHaveBeenCalledWith('/api/parent/profile', {
            headers: { Authorization: 'Bearer test-token' }
        })

        expect(wrapper.text()).toContain('Leo Messi')
        expect(wrapper.text()).toContain('L')
        expect(wrapper.text()).toContain('Left Foot')
        expect(wrapper.text()).toContain('Class of 2026')
    })

    it('calculates and displays season attendance percentage', async () => {
        const wrapper = mount(ProfilePage, { global: { stubs } })
        await flushPromises()

        expect(wrapper.text()).toContain('90%')
        expect(wrapper.text()).toContain('9 / 10 Matches')
    })

    it('renders badges and player of the match count', async () => {
        const wrapper = mount(ProfilePage, { global: { stubs } })
        await flushPromises()

        expect(wrapper.text()).toContain('Star Player')
        expect(wrapper.text()).toContain('Bronze')
        expect(wrapper.text()).toContain('3')
    })

    it('renders empty badges message when player has no badges', async () => {
        mockFetch.mockResolvedValue({
            ...mockProfileData,
            badges: []
        })

        const wrapper = mount(ProfilePage, { global: { stubs } })
        await flushPromises()

        expect(wrapper.text()).toContain('Keep playing to earn badges!')
    })

    it('renders recent matches with POTM badge', async () => {
        const wrapper = mount(ProfilePage, { global: { stubs } })
        await flushPromises()

        expect(wrapper.text()).toContain('Stalybridge Celtic U7')
        expect(wrapper.text()).toContain('vs JNR Tigers')
        expect(wrapper.text()).toContain('4 - 2')
        expect(wrapper.text()).toContain('POTM')
    })

    it('renders empty matches state when recentMatches is empty', async () => {
        mockFetch.mockResolvedValue({
            ...mockProfileData,
            recentMatches: []
        })

        const wrapper = mount(ProfilePage, { global: { stubs } })
        await flushPromises()

        expect(wrapper.text()).toContain('No match history available for this season yet.')
    })

    it('displays error message when profile fetch fails', async () => {
        mockFetch.mockRejectedValue(new Error('Network Error'))

        const wrapper = mount(ProfilePage, { global: { stubs } })
        await flushPromises()

        expect(wrapper.text()).toContain('Failed to load player profile.')
    })
    it('allows a user to double click on the players foot which opens a modal to change this.', async () => {
        const wrapper = mount(ProfilePage, { global: { stubs } })
        await flushPromises()
        const footElement = wrapper.find('.foot-toggle')
        await footElement.trigger('dblclick')
        await flushPromises()
        expect(wrapper.text()).toContain("Select foot")
    })

    it('renders kit sizing section and allows opening kit sizing modal', async () => {
        mockFetch.mockResolvedValue({
            ...mockProfileData,
            shirtSize: '5-6 yrs',
            shortSize: '5-6 yrs',
            sockSize: 12
        })

        const wrapper = mount(ProfilePage, { global: { stubs } })
        await flushPromises()

        expect(wrapper.text()).toContain('Kit Sizing')
        expect(wrapper.text()).toContain('5-6 yrs')
        expect(wrapper.text()).toContain('12')

        const kitToggle = wrapper.find('.kit-toggle')
        await kitToggle.trigger('click')
        await flushPromises()

        expect(wrapper.text()).toContain('Update your child\'s official team kit sizes')
    })
})
