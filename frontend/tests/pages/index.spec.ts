import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount } from '@vue/test-utils'
import { ref } from 'vue'

vi.stubGlobal('useHead', vi.fn())
vi.stubGlobal('definePageMeta', vi.fn())
vi.stubGlobal('$fetch', vi.fn())

const mockIsAuthenticated = ref(true)
vi.stubGlobal('useAuth', () => ({ isAuthenticated: mockIsAuthenticated }))
vi.stubGlobal('navigateTo', vi.fn())

import IndexPage from '~/pages/index.vue'

describe('Index Page', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('redirects to /dashboard when authenticated', () => {
    mockIsAuthenticated.value = true
    mount(IndexPage)
    expect(navigateTo).toHaveBeenCalledWith('/dashboard', { replace: true })
  })

  it('redirects to /login when not authenticated', () => {
    mockIsAuthenticated.value = false
    mount(IndexPage)
    expect(navigateTo).toHaveBeenCalledWith('/login', { replace: true })
  })

  it('renders redirecting text', () => {
    mockIsAuthenticated.value = true
    const wrapper = mount(IndexPage)
    expect(wrapper.text()).toContain('Redirecting')
    expect(wrapper.text()).toContain('Celtic FC')
  })
})
