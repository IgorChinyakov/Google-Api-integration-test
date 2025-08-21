
namespace TelegramBot.Infrastructure.Models
{
    public record CreateEventDto(
        string Title,
        string Description,
        DateTime Start,
        DateTime End,
        bool IsOnline,
        string Location,
        List<string> Participants);
}
