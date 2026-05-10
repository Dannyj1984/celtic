import { ref } from 'vue'
import { useAuth } from './useAuth'

export function useNotifications() {
  const { getAuthHeaders, user } = useAuth()
  const config = useRuntimeConfig()
  const isSupported = ref(false)
  const isSubscribed = ref(false)
  const loading = ref(false)

  if (process.client) {
    isSupported.value = 'serviceWorker' in navigator && 'PushManager' in window
  }

  async function checkSubscription() {
    if (!isSupported.value) return
    
    const registration = await navigator.serviceWorker.ready
    const subscription = await registration.pushManager.getSubscription()
    isSubscribed.value = !!subscription
    return subscription
  }

  async function subscribe() {
    if (!isSupported.value || !user.value) return
    
    loading.value = true
    try {
      const permission = await Notification.requestPermission()
      if (permission !== 'granted') {
        throw new Error('Permission not granted')
      }

      const registration = await navigator.serviceWorker.ready
      const subscription = await registration.pushManager.subscribe({
        userVisibleOnly: true,
        applicationServerKey: urlBase64ToUint8Array(config.public.vapidPublicKey)
      })

      const jsonSub = subscription.toJSON()
      
      await $fetch('/api/notifications/subscribe', {
        method: 'POST',
        headers: getAuthHeaders(),
        body: {
          endpoint: subscription.endpoint,
          p256dh: jsonSub.keys?.p256dh,
          auth: jsonSub.keys?.auth
        }
      })

      isSubscribed.value = true
    } catch (err) {
      console.error('Failed to subscribe to push notifications', err)
      throw err
    } finally {
      loading.value = false
    }
  }

  async function unsubscribe() {
    if (!isSupported.value) return
    
    loading.value = true
    try {
      const registration = await navigator.serviceWorker.ready
      const subscription = await registration.pushManager.getSubscription()
      
      if (subscription) {
        await $fetch('/api/notifications/unsubscribe', {
          method: 'POST',
          headers: getAuthHeaders(),
          body: { endpoint: subscription.endpoint }
        })
        await subscription.unsubscribe()
      }
      
      isSubscribed.value = false
    } catch (err) {
      console.error('Failed to unsubscribe', err)
    } finally {
      loading.value = false
    }
  }

  function urlBase64ToUint8Array(base64String: string) {
    const padding = '='.repeat((4 - base64String.length % 4) % 4)
    const base64 = (base64String + padding)
      .replace(/\-/g, '+')
      .replace(/_/g, '/')

    const rawData = window.atob(base64)
    const outputArray = new Uint8Array(rawData.length)

    for (let i = 0; i < rawData.length; ++i) {
      outputArray[i] = rawData.charCodeAt(i)
    }
    return outputArray
  }

  return {
    isSupported,
    isSubscribed,
    loading,
    checkSubscription,
    subscribe,
    unsubscribe
  }
}
