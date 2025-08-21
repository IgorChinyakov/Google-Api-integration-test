namespace EmoMeter.Application.TextAndAudioProcessing
{
    public interface IAiService
    {
        Task<CalendarEvent?> ParseEventFromTextAsync(string inputText);
    }
}
