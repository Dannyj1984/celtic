/**
 * Composable to generate calendar events via .ics download or Google Calendar URL.
 */
export function useCalendar() {
  const formatIcsDate = (d: Date) =>
    d.toISOString().replace(/[-:]/g, '').replace(/\.\d{3}/, '')

  const formatGoogleDate = (d: Date) =>
    d.toISOString().replace(/[-:]/g, '').replace(/\.\d{3}Z/, 'Z')

  const generateIcsContent = (event: CalendarEvent) => {
    const start = new Date(event.dateTime)
    const end = new Date(start.getTime() + (event.durationMinutes || 90) * 60 * 1000)

    const lines = [
      'BEGIN:VCALENDAR',
      'VERSION:2.0',
      'PRODID:-//Stalybridge Celtic U7//Match Calendar//EN',
      'BEGIN:VEVENT',
      `DTSTART:${formatIcsDate(start)}`,
      `DTEND:${formatIcsDate(end)}`,
      `SUMMARY:${event.title}`,
      `LOCATION:${event.location || 'TBC'}`,
      `DESCRIPTION:${event.description || ''}`,
      `UID:${start.getTime()}@celticfc`,
      'END:VEVENT',
      'END:VCALENDAR'
    ]

    return lines.join('\r\n')
  }

  const downloadIcs = (event: CalendarEvent) => {
    const content = generateIcsContent(event)
    const blob = new Blob([content], { type: 'text/calendar;charset=utf-8' })
    const url = URL.createObjectURL(blob)
    const link = document.createElement('a')
    link.href = url
    link.download = `${event.title.replace(/\s+/g, '_')}.ics`
    document.body.appendChild(link)
    link.click()
    document.body.removeChild(link)
    URL.revokeObjectURL(url)
  }

  const openGoogleCalendar = (event: CalendarEvent) => {
    const start = new Date(event.dateTime)
    const end = new Date(start.getTime() + (event.durationMinutes || 90) * 60 * 1000)

    const params = new URLSearchParams({
      action: 'TEMPLATE',
      text: event.title,
      dates: `${formatGoogleDate(start)}/${formatGoogleDate(end)}`,
      location: event.location || 'TBC',
      details: event.description || ''
    })

    window.open(`https://calendar.google.com/calendar/render?${params.toString()}`, '_blank')
  }

  return { downloadIcs, openGoogleCalendar }
}

interface CalendarEvent {
  title: string
  dateTime: string
  location?: string
  description?: string
  durationMinutes?: number
}
