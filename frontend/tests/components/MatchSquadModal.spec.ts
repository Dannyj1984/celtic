import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount } from '@vue/test-utils'
import { ref } from 'vue'
import MatchSquadModal from '~/components/MatchSquadModal.vue'
import type { MatchSquad } from '~/composables/useMatchSquad'

const mockSquad: MatchSquad = {
  id: 'squad-1',
  format: '5v5',
  firstHalfGoalkeeperPlayerId: 'p1',
  firstHalfGoalkeeperName: 'Alfie',
  secondHalfGoalkeeperPlayerId: 'p2',
  secondHalfGoalkeeperName: 'Bobby',
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
const mockFetchSquad = vi.fn(() => Promise.resolve({ success: true, squad: mockSquad }))
const mockGenerateSquad = vi.fn(() => Promise.resolve({ success: true, squad: mockSquad }))
const mockSaveSquad = vi.fn(() => Promise.resolve({ success: true, squad: mockSquad }))
const mockSwapPlayers = vi.fn()

vi.mock('~/composables/useMatchSquad', () => ({
  useMatchSquad: () => ({
    squad: mockSquadRef,
    loading: ref(false),
    saving: ref(false),
    error: ref(null),
    fetchSquad: mockFetchSquad,
    generateSquad: mockGenerateSquad,
    saveSquad: mockSaveSquad,
    swapPlayers: mockSwapPlayers,
  })
}))

describe('MatchSquadModal', () => {
  const writeTextMock = vi.fn().mockResolvedValue(undefined)

  beforeEach(() => {
    vi.clearAllMocks()
    mockSquadRef.value = JSON.parse(JSON.stringify(mockSquad))
    Object.defineProperty(navigator, 'clipboard', {
      value: {
        writeText: writeTextMock,
      },
      writable: true,
      configurable: true,
    })
  })

  it('renders nothing when isOpen is false', () => {
    const wrapper = mount(MatchSquadModal, {
      props: {
        isOpen: false,
      }
    })
    expect(wrapper.find('.card').exists()).toBe(false)
  })

  it('renders squad modal with period details and tabs when isOpen is true', () => {
    const wrapper = mount(MatchSquadModal, {
      props: {
        isOpen: true,
        event: {
          id: 'e1',
          matchId: 'm1',
          opposition: 'Glossop North End U7',
          dateTime: '2026-10-10T10:00:00Z',
          format: '5v5'
        }
      }
    })

    expect(wrapper.text()).toContain('Matchday Squad Plan')
    expect(wrapper.text()).toContain('Glossop North End U7')
    expect(wrapper.text()).toContain('1st Half GK')
    expect(wrapper.text()).toContain('2nd Half GK')
    expect(wrapper.text()).toContain('Intervals View')
    expect(wrapper.text()).toContain('Match Matrix')
    expect(wrapper.text()).toContain('Playing Time Summary')
  })

  it('switches between view tabs correctly', async () => {
    const wrapper = mount(MatchSquadModal, {
      props: {
        isOpen: true,
        event: { id: 'e1', opposition: 'Test' }
      }
    })

    const buttons = wrapper.findAll('button')
    
    // Switch to Match Matrix
    const matrixBtn = buttons.find(b => b.text().includes('Match Matrix'))
    expect(matrixBtn).toBeDefined()
    if (matrixBtn) {
      await matrixBtn.trigger('click')
      expect(wrapper.text()).toContain('Total Mins')
      expect(wrapper.find('table').exists()).toBe(true)
    }

    // Switch to Playing Time Summary
    const minutesBtn = buttons.find(b => b.text().includes('Playing Time Summary'))
    expect(minutesBtn).toBeDefined()
    if (minutesBtn) {
      await minutesBtn.trigger('click')
      expect(wrapper.text()).toContain('Equal Playing Time Distribution')
    }
  })

  it('allows format changing to 3v3 and regenerates squad', async () => {
    const wrapper = mount(MatchSquadModal, {
      props: {
        isOpen: true,
        event: { id: 'e1', opposition: 'Test', format: '3v3' }
      }
    })

    const select = wrapper.find('select')
    expect(select.exists()).toBe(true)
    await select.setValue('3v3')
    await select.trigger('change')

    expect(mockGenerateSquad).toHaveBeenCalledWith(expect.objectContaining({
      format: '3v3'
    }))
  })

  it('saves squad plan when clicking save button', async () => {
    const wrapper = mount(MatchSquadModal, {
      props: {
        isOpen: true,
        event: { id: 'e1', opposition: 'Test' }
      }
    })

    const saveBtn = wrapper.findAll('button').find(b => b.text().includes('Save Rotation Plan'))
    expect(saveBtn).toBeDefined()
    if (saveBtn) {
      await saveBtn.trigger('click')
      expect(mockSaveSquad).toHaveBeenCalled()
      expect(wrapper.emitted('saved')).toBeTruthy()
    }
  })

  it('copies schedule text to clipboard on copy button click', async () => {
    const wrapper = mount(MatchSquadModal, {
      props: {
        isOpen: true,
        event: { id: 'e1', opposition: 'Test' }
      }
    })

    const copyBtn = wrapper.findAll('button').find(b => b.text().includes('Copy Schedule'))
    expect(copyBtn).toBeDefined()
    if (copyBtn) {
      await copyBtn.trigger('click')
      expect(writeTextMock).toHaveBeenCalled()
    }
  })

  it('emits close when clicking close button', async () => {
    const wrapper = mount(MatchSquadModal, {
      props: {
        isOpen: true,
        event: { id: 'e1', opposition: 'Test' }
      }
    })

    const closeBtn = wrapper.findAll('button').find(b => b.text().trim() === 'Close')
    expect(closeBtn).toBeDefined()
    if (closeBtn) {
      await closeBtn.trigger('click')
      expect(wrapper.emitted('close')).toBeTruthy()
    }
  })
})
