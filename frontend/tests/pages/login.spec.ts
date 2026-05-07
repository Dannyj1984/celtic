import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount } from '@vue/test-utils'
import { ref } from 'vue'
import LoginForm from '../../app/pages/login.vue'

// Mock Nuxt auto-imports and composables used in the component
vi.stubGlobal('definePageMeta', vi.fn())
vi.stubGlobal('useHead', vi.fn())
vi.stubGlobal('navigateTo', vi.fn())
vi.stubGlobal('ref', ref)

const mockLogin = vi.fn()
const mockLoading = ref(false)
const mockError = ref<string | null>(null)

vi.stubGlobal('useAuth', () => ({
  login: mockLogin,
  loading: mockLoading,
  error: mockError,
}))

describe('LoginForm', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    mockLoading.value = false
    mockError.value = null
  })

  it('renders correctly with titles', () => {
    const wrapper = mount(LoginForm)
    // The component title text
    expect(wrapper.text()).toContain('Junior Football')
    expect(wrapper.text()).toContain('Welcome back!')
    expect(wrapper.text()).toContain('Parent Email')
    expect(wrapper.text()).toContain('Password')
  })

  it('renders interactive elements', () => {
    const wrapper = mount(LoginForm)
    expect(wrapper.find('input[type="email"]').exists()).toBe(true)
    expect(wrapper.find('input[type="password"]').exists()).toBe(true)
    expect(wrapper.find('input[type="checkbox"]').exists()).toBe(true)
    expect(wrapper.find('button[type="submit"]').exists()).toBe(true)
    expect(wrapper.text()).toContain('Forgot')
  })

  it('binds input fields correctly', async () => {
    const wrapper = mount(LoginForm)
    const emailInput = wrapper.find('input[type="email"]')
    await emailInput.setValue('test@example.com')
    expect((emailInput.element as HTMLInputElement).value).toBe('test@example.com')
  })

  it('displays error message from useAuth', async () => {
    const wrapper = mount(LoginForm)
    mockError.value = 'Invalid credentials'
    await wrapper.vm.$nextTick()

    // The error element renders when `error` is present
    expect(wrapper.text()).toContain('Invalid credentials')
  })

  it('calls login function on submit', async () => {
    const wrapper = mount(LoginForm)
    mockLogin.mockResolvedValue(true)

    await wrapper.find('input[type="email"]').setValue('test@celtic.app')
    await wrapper.find('input[type="password"]').setValue('pass123')
    await wrapper.find('form').trigger('submit')

    expect(mockLogin).toHaveBeenCalledWith('test@celtic.app', 'pass123')
  })
  describe('Styling', () => {
    it('has correct background gradient', () => {
      const wrapper = mount(LoginForm)
      expect(wrapper.classes()).toContain('bg-gradient-to-b')
      expect(wrapper.classes()).toContain('from-primary')
      expect(wrapper.classes()).toContain('to-surface')
    })
    it('Has to correct logo', () => {
      const wrapper = mount(LoginForm)
      expect(wrapper.find('svg').exists()).toBe(true)
    })
  })
})
