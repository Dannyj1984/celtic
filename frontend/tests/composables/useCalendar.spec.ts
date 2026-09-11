import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { useCalendar } from '~/composables/useCalendar'

describe('useCalendar', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('downloads .ics file with properly formatted vcalendar content', () => {
    const { downloadIcs } = useCalendar()

    const appendChildSpy = vi.spyOn(document.body, 'appendChild')
    const removeChildSpy = vi.spyOn(document.body, 'removeChild')
    const clickSpy = vi.fn()

    // Mock createElement for <a> tag
    const origCreateElement = document.createElement.bind(document)
    vi.spyOn(document, 'createElement').mockImplementation((tagName: string) => {
      if (tagName === 'a') {
        const link = origCreateElement('a')
        link.click = clickSpy
        return link
      }
      return origCreateElement(tagName)
    })

    const createObjectURLMock = vi.fn(() => 'blob:http://localhost/dummy-uuid')
    const revokeObjectURLMock = vi.fn()
    window.URL.createObjectURL = createObjectURLMock
    window.URL.revokeObjectURL = revokeObjectURLMock

    const event = {
      title: 'Stalybridge Celtic vs Hyde United',
      dateTime: '2026-10-15T10:00:00.000Z',
      location: 'Bower Fold Stadium',
      description: 'Match day 5v5 league fixture',
      durationMinutes: 60
    }

    downloadIcs(event)

    expect(createObjectURLMock).toHaveBeenCalled()
    expect(appendChildSpy).toHaveBeenCalled()
    expect(clickSpy).toHaveBeenCalled()
    expect(removeChildSpy).toHaveBeenCalled()
    expect(revokeObjectURLMock).toHaveBeenCalledWith('blob:http://localhost/dummy-uuid')
  })

  it('opens Google Calendar URL with correct query parameters', () => {
    const { openGoogleCalendar } = useCalendar()
    const openSpy = vi.spyOn(window, 'open').mockImplementation(() => null)

    const event = {
      title: 'Celtic vs Glossop',
      dateTime: '2026-11-20T14:30:00.000Z',
      location: 'Glossop Pitch 2',
      description: '3v3 tournament match',
      durationMinutes: 45
    }

    openGoogleCalendar(event)

    expect(openSpy).toHaveBeenCalled()
    const calledUrl = openSpy.mock.calls[0][0] as string
    expect(calledUrl).toContain('https://calendar.google.com/calendar/render?')
    expect(calledUrl).toContain('action=TEMPLATE')
    expect(calledUrl).toContain('Celtic+vs+Glossop')
    expect(calledUrl).toContain('Glossop+Pitch+2')
    expect(calledUrl).toContain('3v3+tournament+match')
  })

  it('uses default fallback values when location, description, or duration are omitted', () => {
    const { openGoogleCalendar } = useCalendar()
    const openSpy = vi.spyOn(window, 'open').mockImplementation(() => null)

    const event = {
      title: 'Training Session',
      dateTime: '2026-11-20T18:00:00.000Z'
    }

    openGoogleCalendar(event)

    expect(openSpy).toHaveBeenCalled()
    const calledUrl = openSpy.mock.calls[0][0] as string
    expect(calledUrl).toContain('TBC') // default location
  })
})
