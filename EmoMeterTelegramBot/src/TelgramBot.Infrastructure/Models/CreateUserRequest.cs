using System.Text.Json.Serialization;

namespace TelegramBot.Infrastructure.Models;

public class CreateUserRequest
{
    [JsonPropertyName("email")]
    public string Email { get; set; }

    [JsonPropertyName("chat_id")]
    public long ChatId { get; set; }

    [JsonPropertyName("notify_before_minutes")]
    public int NotifyBeforeMinutes { get; set; }
}