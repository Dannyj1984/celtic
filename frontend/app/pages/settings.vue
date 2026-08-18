<template>
  <div class="space-y-8 max-w-4xl mx-auto pb-20 md:pb-8">
    <!-- Header -->
    <div class="flex items-center justify-between">
      <div>
        <h1 class="text-3xl font-black text-text-primary uppercase tracking-tight">Account Settings</h1>
        <p class="text-text-secondary text-sm mt-1">Manage your account information and security preferences.</p>
      </div>
    </div>

    <!-- Loading State -->
    <div v-if="loading" class="flex justify-center py-12">
      <div class="animate-spin w-8 h-8 rounded-full border-4 border-celtic-green border-t-transparent"></div>
    </div>

    <div v-else-if="fetchError" class="bg-danger/10 border border-danger/20 text-danger p-4 rounded-lg">
      {{ fetchError }}
    </div>

    <template v-else>
      <!-- Personal Information Card -->
      <UCard class="bg-bg-card border border-border-color shadow-sm rounded-xl">
        <template #header>
          <div class="flex items-center gap-3">
            <UIcon name="i-heroicons-user-20-solid" class="w-5 h-5 text-celtic-green" />
            <h2 class="text-lg font-black text-text-primary uppercase tracking-tight">Personal Details</h2>
          </div>
        </template>

        <form @submit.prevent="saveAccountDetails" class="space-y-5">
          <div>
            <label for="fullName" class="block text-xs font-bold text-text-muted uppercase tracking-wider mb-2">Full Name</label>
            <input id="fullName" v-model="accountForm.fullName" type="text" class="input w-full" placeholder="John Doe" required />
          </div>

          <div>
            <label for="email" class="block text-xs font-bold text-text-muted uppercase tracking-wider mb-2">Email Address</label>
            <input id="email" v-model="accountForm.email" type="email" class="input w-full" placeholder="john@example.com" required />
          </div>

          <div>
            <label for="phone" class="block text-xs font-bold text-text-muted uppercase tracking-wider mb-2">Phone Number</label>
            <input id="phone" v-model="accountForm.phone" type="tel" class="input w-full" placeholder="07123456789" />
          </div>

          <div class="flex justify-end pt-2">
            <button type="submit" :disabled="savingAccount" class="btn-primary flex items-center gap-2">
              <span v-if="savingAccount" class="animate-spin w-4 h-4 rounded-full border-2 border-white border-t-transparent"></span>
              {{ savingAccount ? 'Saving...' : 'Save Account Details' }}
            </button>
          </div>
        </form>
      </UCard>

      <!-- Security & Password Card -->
      <UCard class="bg-bg-card border border-border-color shadow-sm rounded-xl">
        <template #header>
          <div class="flex items-center gap-3">
            <UIcon name="i-heroicons-key-20-solid" class="w-5 h-5 text-celtic-gold" />
            <h2 class="text-lg font-black text-text-primary uppercase tracking-tight">Security & Password</h2>
          </div>
        </template>

        <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4 py-2">
          <div>
            <h3 class="text-sm font-bold text-text-primary">Password</h3>
            <p class="text-xs text-text-secondary mt-1">Regularly updating your password helps keep your account secure.</p>
          </div>
          <button @click="openPasswordModal" class="btn-secondary whitespace-nowrap">
            Change Password
          </button>
        </div>
      </UCard>
    </template>

    <!-- Change Password Modal -->
    <div v-if="showPasswordModal" class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm">
      <div class="card p-6 max-w-md w-full bg-surface border border-border shadow-xl space-y-4">
        <div class="flex items-center justify-between">
          <h3 class="text-lg font-black text-text-primary uppercase tracking-tight">Change Password</h3>
          <button @click="showPasswordModal = false" class="text-text-muted hover:text-text-primary text-xl font-bold">&times;</button>
        </div>

        <form @submit.prevent="submitChangePassword" class="space-y-4" data-testid="change-password-form">
          <div>
            <label for="currentPassword" class="block text-xs font-bold text-text-muted uppercase tracking-wider mb-1">Current Password</label>
            <input id="currentPassword" v-model="passwordForm.currentPassword" type="password" class="input w-full" required />
          </div>

          <div>
            <label for="newPassword" class="block text-xs font-bold text-text-muted uppercase tracking-wider mb-1">New Password</label>
            <input id="newPassword" v-model="passwordForm.newPassword" type="password" class="input w-full" required minlength="6" />
          </div>

          <div>
            <label for="confirmPassword" class="block text-xs font-bold text-text-muted uppercase tracking-wider mb-1">Confirm New Password</label>
            <input id="confirmPassword" v-model="passwordForm.confirmPassword" type="password" class="input w-full" required minlength="6" />
          </div>

          <div class="flex justify-end gap-3 pt-4">
            <button type="button" @click="showPasswordModal = false" class="btn-secondary">Cancel</button>
            <button type="submit" :disabled="changingPassword" class="btn-primary">
              {{ changingPassword ? 'Updating...' : 'Update Password' }}
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'

definePageMeta({
  layout: 'app',
})

useHead({
  title: 'Settings - Stalybridge Celtic U7',
})

const { getAuthHeaders, fetchUser } = useAuth()
const toast = useToast()

const loading = ref(true)
const fetchError = ref<string | null>(null)
const savingAccount = ref(false)

const accountForm = reactive({
  fullName: '',
  email: '',
  phone: ''
})

const showPasswordModal = ref(false)
const changingPassword = ref(false)
const passwordForm = reactive({
  currentPassword: '',
  newPassword: '',
  confirmPassword: ''
})

onMounted(async () => {
  await fetchAccountData()
})

async function fetchAccountData() {
  loading.value = true
  fetchError.value = null
  try {
    const data = await $fetch<any>('/api/parent/account', {
      headers: getAuthHeaders()
    })
    accountForm.fullName = data.fullName || ''
    accountForm.email = data.email || ''
    accountForm.phone = data.phone || ''
  } catch (err: any) {
    fetchError.value = 'Failed to load account details.'
  } finally {
    loading.value = false
  }
}

async function saveAccountDetails() {
  savingAccount.value = true
  try {
    const updated = await $fetch<any>('/api/parent/account', {
      method: 'PUT',
      headers: getAuthHeaders(),
      body: {
        fullName: accountForm.fullName,
        email: accountForm.email,
        phone: accountForm.phone
      }
    })
    accountForm.fullName = updated.fullName
    accountForm.email = updated.email
    accountForm.phone = updated.phone
    
    if (fetchUser) await fetchUser()

    toast.add({ title: 'Success', description: 'Account details updated successfully.', color: 'green' })
  } catch (err: any) {
    toast.add({ title: 'Error', description: err?.data?.message || 'Failed to update account details.', color: 'red' })
  } finally {
    savingAccount.value = false
  }
}

function openPasswordModal() {
  passwordForm.currentPassword = ''
  passwordForm.newPassword = ''
  passwordForm.confirmPassword = ''
  showPasswordModal.value = true
}

async function submitChangePassword() {
  if (passwordForm.newPassword !== passwordForm.confirmPassword) {
    toast.add({ title: 'Error', description: 'Passwords do not match.', color: 'red' })
    return
  }

  changingPassword.value = true
  try {
    await $fetch('/api/auth/change-password', {
      method: 'POST',
      headers: getAuthHeaders(),
      body: {
        currentPassword: passwordForm.currentPassword,
        newPassword: passwordForm.newPassword
      }
    })
    toast.add({ title: 'Success', description: 'Password changed successfully.', color: 'green' })
    showPasswordModal.value = false
  } catch (err: any) {
    toast.add({ title: 'Error', description: err?.data?.message || 'Failed to change password.', color: 'red' })
  } finally {
    changingPassword.value = false
  }
}
</script>
