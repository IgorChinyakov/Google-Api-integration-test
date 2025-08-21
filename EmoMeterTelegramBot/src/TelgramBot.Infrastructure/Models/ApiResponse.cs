using Newtonsoft.Json.Linq;

namespace TelegramBot.Infrastructure.Models;
public class ApiResponse
{
    public bool Success { get; init; }

    public JToken? Data { get; init; }

    public List<ApiError> Errors { get; init; } = [];
}

//public class ApiResponse
//{
//    public bool Success { get; init; }

//    public List<ApiError> Errors { get; init; } = [];

//    public static ApiResponse Ok() => new() { Success = true };

//    public static ApiResponse Fail(List<ApiError> errors) =>
//        new() { Success = false, Errors = errors };
//}

public record ApiError(string Code, string Message);
