import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount, flushPromises } from '@vue/test-utils'

vi.stubGlobal('useHead', vi.fn())
vi.stubGlobal('definePageMeta', vi.fn())

const mockGetAuthHeaders = vi.fn(() => ({ Authorization: 'Bearer test-token' }))
const mockFetchUser = vi.fn(() => Promise.resolve())
vi.stubGlobal('useAuth', () => ({ getAuthHeaders: mockGetAuthHeaders, fetchUser: mockFetchUser }))

const mockToastAdd = vi.fn()
vi.stubGlobal('useToast', () => ({ add: mockToastAdd }))

const mockAccountData = {
  fullName: 'Jane Doe',
  email: 'jane@example.com',
  phone: '07777777777'
}

const mockFetch = vi.fn()
vi.stubGlobal('$fetch', mockFetch)

import SettingsPage from '~/pages/settings.vue'

describe('Parent Settings Page', () => {
  const stubs = {
    UIcon: true,
    UCard: { template: '<div><header><slot name="header" /></header><div><slot /></div></div>' },
    NuxtLink: { template: '<a><slot /></a>' }
  }

  beforeEach(() => {
    vi.clearAllMocks()
    mockFetch.mockResolvedValue(mockAccountData)
  })

  it('fetches parent account data on mount and renders fields', async () => {
    const wrapper = mount(SettingsPage, { global: { stubs } })
    await flushPromises()

    expect(mockFetch).toHaveBeenCalledWith('/api/parent/account', {
      headers: { Authorization: 'Bearer test-token' }
    })

    const fullNameInput = wrapper.find('#fullName').element as HTMLInputElement
    const emailInput = wrapper.find('#email').element as HTMLInputElement
    const phoneInput = wrapper.find('#phone').element as HTMLInputElement

    expect(fullNameInput.value).toBe('Jane Doe')
    expect(emailInput.value).toBe('jane@example.com')
    expect(phoneInput.value).toBe('07777777777')
  })

  it('saves updated account details on form submission', async () => {
    mockFetch.mockImplementation((url, opts) => {
      if (url === '/api/parent/account' && opts?.method === 'PUT') {
        return Promise.resolve({
          fullName: 'Jane Smith',
          email: 'janesmith@example.com',
          phone: '07888888888'
        })
      }
      return Promise.resolve(mockAccountData)
    })

    const wrapper = mount(SettingsPage, { global: { stubs } })
    await flushPromises()

    await wrapper.find('#fullName').setValue('Jane Smith')
    await wrapper.find('#email').setValue('janesmith@example.com')
    await wrapper.find('form').trigger('submit')
    await flushPromises()

    expect(mockFetch).toHaveBeenCalledWith('/api/parent/account', {
      method: 'PUT',
      headers: { Authorization: 'Bearer test-token' },
      body: {
        fullName: 'Jane Smith',
        email: 'janesmith@example.com',
        phone: '07777777777'
      }
    })

    expect(mockToastAdd).toHaveBeenCalledWith({
      title: 'Success',
      description: 'Account details updated successfully.',
      color: 'green'
    })
  })

  it('opens Change Password modal and submits new password', async () => {
    const wrapper = mount(SettingsPage, { global: { stubs } })
    await flushPromises()

    const changePasswordBtn = wrapper.findAll('button').find(b => b.text().includes('Change Password'))
    expect(changePasswordBtn?.exists()).toBe(true)

    await changePasswordBtn!.trigger('click')
    await flushPromises()

    expect(wrapper.text()).toContain('Change Password')

    await wrapper.find('#currentPassword').setValue('oldPass123')
    await wrapper.find('#newPassword').setValue('newPass123')
    await wrapper.find('#confirmPassword').setValue('newPass123')

    mockFetch.mockResolvedValueOnce({ message: 'Password changed successfully.' })

    const form = wrapper.find('[data-testid="change-password-form"]')
    await form.trigger('submit')
    await flushPromises()

    expect(mockFetch).toHaveBeenCalledWith('/api/auth/change-password', {
      method: 'POST',
      headers: { Authorization: 'Bearer test-token' },
      body: {
        currentPassword: 'oldPass123',
        newPassword: 'newPass123'
      }
    })

    expect(mockToastAdd).toHaveBeenCalledWith({
      title: 'Success',
      description: 'Password changed successfully.',
      color: 'green'
    })
  })
})
