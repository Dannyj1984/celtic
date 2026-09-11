import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount } from '@vue/test-utils'
import { ref } from 'vue'
import LiveMatchModal from '~/components/LiveMatchModal.vue'
import type { MatchSquad } from '~/composables/useMatchSquad'

const mockSquad: MatchSquad = {
  id: 'squad-1',
  format: '5v5',
  firstHalfGoalkeeperPlayerId: 'p1',
  firstHalfGoalkeeperName: 'GK Player 1',
  secondHalfGoalkeeperPlayerId: 'p2',
  secondHalfGoalkeeperName: 'GK Player 2',
  halfDurationMinutes: 25,
  totalPeriods: 8,
  periodDurationMinutes: 6,
  registeredPlayers: [
    { id: 'p1', name: 'Alfie' },
    { id: 'p2', name: 'Bobby' },
    { id: 'p3', name: 'Charlie' },
    { id: 'p4', name: 'David' },
    { id: 'p5', name: 'Ethan' },
    { id: 'p6', name: 'Frankie' },
  ],
  periods: [
    {
      periodNumber: 1,
      half: 1,
      startMinute: 0,
      endMinute: 6,
      goalkeeper: { id: 'p1', name: 'Alfie' },
      outfieldPlayers: [
        { id: 'p2', name: 'Bobby' },
        { id: 'p3', name: 'Charlie' },
        { id: 'p4', name: 'David' },
        { id: 'p5', name: 'Ethan' },
      ],
      benchPlayers: [{ id: 'p6', name: 'Frankie' }],
      substitutions: [],
    },
    {
      periodNumber: 2,
      half: 1,
      startMinute: 6,
      endMinute: 12,
      goalkeeper: { id: 'p1', name: 'Alfie' },
      outfieldPlayers: [
        { id: 'p6', name: 'Frankie' },
        { id: 'p3', name: 'Charlie' },
        { id: 'p4', name: 'David' },
        { id: 'p5', name: 'Ethan' },
      ],
      benchPlayers: [{ id: 'p2', name: 'Bobby' }],
      substitutions: [
        { playerInId: 'p6', playerInName: 'Frankie', playerOutId: 'p2', playerOutName: 'Bobby' }
      ],
    }
  ],
  playerMinutes: [
    { playerId: 'p1', playerName: 'Alfie', totalMinutes: 12, goalkeeperMinutes: 12, outfieldMinutes: 0, benchMinutes: 0 },
    { playerId: 'p2', playerName: 'Bobby', totalMinutes: 6, goalkeeperMinutes: 0, outfieldMinutes: 6, benchMinutes: 6 },
    { playerId: 'p3', playerName: 'Charlie', totalMinutes: 12, goalkeeperMinutes: 0, outfieldMinutes: 12, benchMinutes: 0 },
    { playerId: 'p4', playerName: 'David', totalMinutes: 12, goalkeeperMinutes: 0, outfieldMinutes: 12, benchMinutes: 0 },
    { playerId: 'p5', playerName: 'Ethan', totalMinutes: 12, goalkeeperMinutes: 0, outfieldMinutes: 12, benchMinutes: 0 },
    { playerId: 'p6', playerName: 'Frankie', totalMinutes: 6, goalkeeperMinutes: 0, outfieldMinutes: 6, benchMinutes: 6 },
  ],
  updatedAt: '2026-09-04T10:00:00Z',
}

const mockSquadRef = ref<MatchSquad | null>(mockSquad)
const mockSwapPlayers = vi.fn()
const mockFetchSquad = vi.fn(() => Promise.resolve({ success: true, squad: mockSquad }))
const mockGenerateSquad = vi.fn(() => Promise.resolve({ success: true, squad: mockSquad }))

vi.mock('~/composables/useMatchSquad', () => ({
  useMatchSquad: () => ({
    squad: mockSquadRef,
    swapPlayers: mockSwapPlayers,
    fetchSquad: mockFetchSquad,
    generateSquad: mockGenerateSquad,
  })
}))

describe('LiveMatchModal', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    mockSquadRef.value = JSON.parse(JSON.stringify(mockSquad))
  })

  it('renders nothing when isOpen is false', () => {
    const wrapper = mount(LiveMatchModal, {
      props: {
        isOpen: false,
        initialSquad: mockSquad,
      }
    })
    expect(wrapper.find('.card').exists()).toBe(false)
  })

  it('renders modal with match details and pitch when isOpen is true', () => {
    const wrapper = mount(LiveMatchModal, {
      props: {
        isOpen: true,
        initialSquad: mockSquad,
        event: {
          opposition: 'Curzon Ashton U7',
          format: '5v5'
        }
      }
    })

    expect(wrapper.text()).toContain('LIVE MATCHDAY')
    expect(wrapper.text()).toContain('Curzon Ashton U7')
    expect(wrapper.text()).toContain('5v5 (with GK)')
    expect(wrapper.text()).toContain('Alfie')
    expect(wrapper.text()).toContain('Bobby')
    expect(wrapper.text()).toContain('Frankie')
  })

  it('renders 3v3 layout correctly without goalkeeper slot', () => {
    const squad3v3 = {
      ...mockSquad,
      format: '3v3',
      periods: [
        {
          periodNumber: 1,
          half: 1,
          startMinute: 0,
          endMinute: 6,
          goalkeeper: null,
          outfieldPlayers: [
            { id: 'p1', name: 'Alfie' },
            { id: 'p2', name: 'Bobby' },
            { id: 'p3', name: 'Charlie' },
          ],
          benchPlayers: [{ id: 'p4', name: 'David' }],
          substitutions: [],
        }
      ]
    }
    mockSquadRef.value = JSON.parse(JSON.stringify(squad3v3))

    const wrapper = mount(LiveMatchModal, {
      props: {
        isOpen: true,
        initialSquad: squad3v3,
        event: {
          opposition: 'Moston Tigers U7',
          format: '3v3'
        }
      }
    })

    expect(wrapper.text()).toContain('3v3 (no GK)')
    expect(wrapper.text()).not.toContain('GOALKEEPER')
  })

  it('emits close event when clicking close button', async () => {
    const wrapper = mount(LiveMatchModal, {
      props: {
        isOpen: true,
        initialSquad: mockSquad,
      }
    })

    const closeBtn = wrapper.find('button.text-text-muted')
    await closeBtn.trigger('click')
    expect(wrapper.emitted('close')).toBeTruthy()
  })

  it('handles player selection and swapping on pitch/bench', async () => {
    const wrapper = mount(LiveMatchModal, {
      props: {
        isOpen: true,
        initialSquad: mockSquad,
      }
    })

    // Find clickable player elements
    const divs = wrapper.findAll('div')
    const bobbyElement = divs.find(el => el.text().includes('Bobby') && el.classes().includes('cursor-pointer'))
    const frankieElement = divs.find(el => el.text().includes('Frankie') && el.classes().includes('cursor-pointer'))

    expect(bobbyElement).toBeDefined()
    expect(frankieElement).toBeDefined()

    if (bobbyElement && frankieElement) {
      await bobbyElement.trigger('click')
      await frankieElement.trigger('click')
      expect(mockSwapPlayers).toHaveBeenCalledWith(0, 'p2', 'p6')
    }
  })

  it('triggers interval alerts and next interval button', async () => {
    const wrapper = mount(LiveMatchModal, {
      props: {
        isOpen: true,
        initialSquad: mockSquad,
      }
    })

    const nextSubBtn = wrapper.findAll('button').find(b => b.text().includes('Next Sub'))
    expect(nextSubBtn).toBeDefined()
    if (nextSubBtn) {
      await nextSubBtn.trigger('click')
      expect(wrapper.text().toLowerCase()).toContain('time for substitutions')
    }
  })
})
