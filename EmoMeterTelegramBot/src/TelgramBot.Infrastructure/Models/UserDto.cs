namespace TelegramBot.Infrastructure.Models;

public class UserDto
{
    public long? UserId { get; set; }
    public string? Email { get; set; }
    public int? DefaultNotifyBeforeMinute { get; set; }
}