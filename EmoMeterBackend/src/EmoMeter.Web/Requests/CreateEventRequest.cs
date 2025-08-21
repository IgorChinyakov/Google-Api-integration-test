using EmoMeter.Domain.ValueObjects.Event;

namespace EmoMeter.Web.Requests
{
    public record CreateEventRequest(
        string Title,
        string Description,
        DateTime Start,
        DateTime End,
        bool IsOnline,
        string Location,
        List<string> Participants);
}
