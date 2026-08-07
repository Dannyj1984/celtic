using Celtic.Api.DTOs;

namespace Celtic.Api.Services;

public interface IEventService
{
    Task<List<EventDto>> GetAllEventsAsync();
    Task<EventDto> GetEventByIdAsync(Guid id);
    Task<EventDto> CreateEventAsync(CreateEventRequest request);
    Task<EventDto> UpdateEventAsync(Guid id, UpdateEventRequest request);
    Task<EventDto> UpdateEventAttendanceAsync(Guid eventId, List<Guid> playerIds, string adminUserId);
    Task DeleteEventAsync(Guid id);
}
