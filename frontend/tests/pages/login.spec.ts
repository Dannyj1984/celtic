import { describe, it, expect, vi } from 'vitest'
import { mount } from '@vue/test-utils'
import { defineComponent, ref } from 'vue'

// Create a simplified login component for testing
// (We test the logic, not the Nuxt-specific wiring)
const LoginForm = defineComponent({
  template: `
    <form @submit.prevent="handleLogin">
      <input id="email" v-model="email" type="email" data-testid="email-input" />
      <input id="password" v-model="password" type="password" data-testid="password-input" />
      <div v-if="error" data-testid="error-message">{{ error }}</div>
      <button type="submit" data-testid="submit-btn" :disabled="loading">
        {{ loading ? 'Signing in...' : 'Sign In' }}
      </button>
    </form>
  `,
  setup() {
    const email = ref('')
    const password = ref('')
    const loading = ref(false)
    const error = ref<string | null>(null)

    async function handleLogin() {
      if (!email.value || !password.value) {
        error.value = 'Email and password are required'
        return
      }
      loading.value = true
      error.value = null

      try {
        // Simulated login call
        const response = await fetch('/api/auth/login', {
          method: 'POST',
          body: JSON.stringify({ email: email.value, password: password.value }),
        })

        if (!response.ok) {
          error.value = 'Invalid email or password'
          return
        }
      } catch {
        error.value = 'Network error'
      } finally {
        loading.value = false
      }
    }

    return { email, password, loading, error, handleLogin }
  },
})

describe('LoginForm', () => {
  it('renders email and password inputs', () => {
    const wrapper = mount(LoginForm)

    expect(wrapper.find('[data-testid="email-input"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="password-input"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="submit-btn"]').exists()).toBe(true)
  })

  it('shows Sign In text on button', () => {
    const wrapper = mount(LoginForm)
    expect(wrapper.find('[data-testid="submit-btn"]').text()).toBe('Sign In')
  })

  it('does not show error message initially', () => {
    const wrapper = mount(LoginForm)
    expect(wrapper.find('[data-testid="error-message"]').exists()).toBe(false)
  })

  it('shows error when form is submitted with empty fields', async () => {
    const wrapper = mount(LoginForm)

    await wrapper.find('form').trigger('submit')
    await wrapper.vm.$nextTick()

    expect(wrapper.find('[data-testid="error-message"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="error-message"]').text()).toBe('Email and password are required')
  })

  it('binds email input correctly', async () => {
    const wrapper = mount(LoginForm)
    const input = wrapper.find('[data-testid="email-input"]')

    await input.setValue('test@example.com')

    expect((input.element as HTMLInputElement).value).toBe('test@example.com')
  })

  it('calls API on valid form submission', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true, json: () => Promise.resolve({}) })
    vi.stubGlobal('fetch', fetchMock)

    const wrapper = mount(LoginForm)

    await wrapper.find('[data-testid="email-input"]').setValue('admin@celtic.app')
    await wrapper.find('[data-testid="password-input"]').setValue('Admin123!')
    await wrapper.find('form').trigger('submit')

    expect(fetchMock).toHaveBeenCalledWith('/api/auth/login', expect.objectContaining({
      method: 'POST',
    }))

    vi.unstubAllGlobals()
  })
})
