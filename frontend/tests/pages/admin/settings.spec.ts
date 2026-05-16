import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount } from '@vue/test-utils'
import SettingsPage from '@/pages/admin/settings.vue'
import { ref } from 'vue'

vi.stubGlobal('useHead', vi.fn())
vi.stubGlobal('definePageMeta', vi.fn())
vi.stubGlobal('useToast', vi.fn(() => ({ add: vi.fn() })))
vi.stubGlobal('useAuth', vi.fn(() => ({ getAuthHeaders: vi.fn(() => ({})) })))
vi.stubGlobal('$fetch', vi.fn())

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

const stubs = {
  UForm: { template: '<form @submit.prevent="$emit(\'submit\')"><slot /></form>' },
  UFormGroup: { props: ['label'], template: '<div>{{ label }}<slot /></div>' },
  UInput: { template: '<input />' },
  USelect: { template: '<select></select>' },
  UButton: { template: '<button type="submit"><slot /></button>' },
  UCard: { template: '<div><slot /></div>' },
  UTextarea: { props: ['modelValue'], template: '<textarea v-bind="$attrs" :value="modelValue" @input="$emit(\'update:modelValue\', $event.target.value)"></textarea>', emits: ['update:modelValue'] },
  UIcon: true
}

describe('Admin Settings Page', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('renders settings form with title', () => {
    const wrapper = mount(SettingsPage, { global: { stubs } })
    expect(wrapper.text()).toContain('Club Settings')
    expect(wrapper.text()).toContain('Save Settings')
  })

  it('renders subscription settings section', () => {
    const wrapper = mount(SettingsPage, { global: { stubs } })
    expect(wrapper.text()).toContain('Subscription Settings')
    expect(wrapper.text()).toContain('Next Payment Due')
  })

  it('renders training schedule section', () => {
    const wrapper = mount(SettingsPage, { global: { stubs } })
    expect(wrapper.text()).toContain('Weekly Training')
    expect(wrapper.text()).toContain('Day of Week')
    expect(wrapper.text()).toContain('Start Time')
    expect(wrapper.text()).toContain('End Time')
  })

  it('renders contact info section', () => {
    const wrapper = mount(SettingsPage, { global: { stubs } })
    expect(wrapper.text()).toContain('Contact Info')
    expect(wrapper.text()).toContain('Coach WhatsApp Number')
  })

  it('renders training focus field', () => {
    const wrapper = mount(SettingsPage, { global: { stubs } })
    expect(wrapper.text()).toContain('Next Training Focus')
  })
  it('renders a field for good to know details', () => {
    const wrapper = mount(SettingsPage, { global: { stubs } })
    expect(wrapper.text()).toContain('Good to know details')
  })
  it('shows submitted good to know info', async () => {
    const wrapper = mount(SettingsPage, { global: { stubs } })
    const goodToKnowInput = wrapper.find('[data-testid="goodToKnow-input"]')
    const testInfo = 'Some important info'
    await goodToKnowInput.setValue(testInfo)
    expect(wrapper.vm.state.goodToKnow).toBe(testInfo)
  })
})

