import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount } from '@vue/test-utils'
import SettingsPage from '@/pages/admin/settings.vue'
import { ref } from 'vue'

vi.stubGlobal('useHead', vi.fn())
vi.stubGlobal('definePageMeta', vi.fn())
vi.stubGlobal('useToast', vi.fn(() => ({ add: vi.fn() })))

const mockSettingsData = ref({
  nextSubPaymentDate: '2026-10-15T00:00:00Z',
  trainingDay: 3, // Wednesday
  trainingStartTime: '17:30:00',
  trainingEndTime: '19:00:00',
  trainingLocation: 'Riverside Sports Complex',
  coachWhatsAppNumber: '1234567890'
})

vi.stubGlobal('useFetch', vi.fn(() => ({
  data: mockSettingsData,
  pending: ref(false),
  error: ref(null)
})))

describe('Admin Settings Page', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('renders settings form correctly', () => {
    const wrapper = mount(SettingsPage, {
      global: {
        stubs: {
          UForm: { template: '<form><slot /></form>' },
          UFormGroup: { template: '<div><slot /></div>' },
          UInput: { template: '<input />' },
          USelect: { template: '<select></select>' },
          UButton: { template: '<button><slot /></button>' },
          UCard: { template: '<div><slot /></div>' },
          UIcon: true
        }
      }
    })

    expect(wrapper.text()).toContain('Club Settings')
    expect(wrapper.text()).toContain('Save Settings')
  })
})
