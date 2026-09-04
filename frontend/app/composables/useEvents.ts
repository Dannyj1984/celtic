import { ref } from 'vue'
import { useAuth } from './useAuth'

export interface Event {
  id: string
  seasonId?: string | null
  seasonName?: string | null
  type: string
  dateTime: string
  location: string
  notes?: string | null
  isCancelled: boolean
  matchId?: string | null
  attendingPlayers: {
    playerId: string
    fullName: string
  }[]
  captain1PlayerId?: string | null
  captain1PlayerName?: string | null
  captain2PlayerId?: string | null
  captain2PlayerName?: string | null
  teamId?: string | null
  teamName?: string | null
}

export function useEvents() {
  const { getAuthHeaders } = useAuth()
  const events = ref<Event[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function fetchEvents() {
    loading.value = true
    error.value = null
    try {
      events.value = await $fetch<Event[]>('/api/events', {
        headers: getAuthHeaders(),
      })
    } catch (err: any) {
      error.value = err?.data?.message || 'Failed to fetch events'
    } finally {
      loading.value = false
    }
  }

  async function createEvent(eventData: any) {
    try {
      const newEvent = await $fetch<Event>('/api/events', {
        method: 'POST',
        headers: getAuthHeaders(),
        body: eventData,
      })
      events.value.push(newEvent)
      return { success: true, event: newEvent }
    } catch (err: any) {
      return { success: false, error: err?.data?.message || 'Failed to create event' }
    }
  }

  async function updateEvent(id: string, eventData: any) {
    try {
      const updatedEvent = await $fetch<Event>(`/api/events/${id}`, {
        method: 'PUT',
        headers: getAuthHeaders(),
        body: eventData,
      })
      const index = events.value.findIndex(e => e.id === id)
      if (index !== -1) {
        events.value[index] = updatedEvent
      }
      return { success: true, event: updatedEvent }
    } catch (err: any) {
      return { success: false, error: err?.data?.message || 'Failed to update event' }
    }
  }

  async function deleteEvent(id: string) {
    try {
      await $fetch(`/api/events/${id}`, {
        method: 'DELETE',
        headers: getAuthHeaders(),
      })
      events.value = events.value.filter(e => e.id !== id)
      return { success: true }
    } catch (err: any) {
      return { success: false, error: err?.data?.message || 'Failed to delete event' }
    }
  }

  async function updateEventAttendance(
    eventId: string,
    playerIds: string[],
    captain1PlayerId?: string | null,
    captain2PlayerId?: string | null
  ) {
    try {
      const updatedEvent = await $fetch<Event>(`/api/events/${eventId}/attendance`, {
        method: 'POST',
        headers: getAuthHeaders(),
        body: {
          playerIds,
          captain1PlayerId: captain1PlayerId || null,
          captain2PlayerId: captain2PlayerId || null,
        },
      })
      const index = events.value.findIndex(e => e.id === eventId)
      if (index !== -1) {
        events.value[index] = updatedEvent
      }
      return { success: true, event: updatedEvent }
    } catch (err: any) {
      return { success: false, error: err?.data?.message || 'Failed to update attendance' }
    }
  }

  return {
    events,
    loading,
    error,
    fetchEvents,
    createEvent,
    updateEvent,
    updateEventAttendance,
    deleteEvent,
  }
}
