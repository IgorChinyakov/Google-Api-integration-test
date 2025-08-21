using System.Text.Json.Serialization;

namespace TelegramBot.Infrastructure.Models;
public record RegisterResponse(string Status, string? AuthUrl);
//{
//    public string Status { get; set; }
//    public string? AuthUrl { get; set; }
//}
