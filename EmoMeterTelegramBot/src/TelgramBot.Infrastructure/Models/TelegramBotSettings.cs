namespace TelegramBot.Infrastructure.Models;

public class TelegramBotSettings
{
    public string Token { get; set; }
    public ApiEndpoints ApiEndpoints { get; set; }
    public string[] AllowedUpdates { get; set; }
}