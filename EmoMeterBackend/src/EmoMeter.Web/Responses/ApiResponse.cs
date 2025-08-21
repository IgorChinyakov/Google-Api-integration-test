using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;

namespace EmoMeter.Web.Responses
{
    public class ApiResponse
    {
        public bool Success { get; init; }

        public object? Data { get; init; }

        public List<ApiError> Errors { get; init; } = [];

        public static ApiResponse Ok(object? data = null) => new() { Success = true, Data = data };

        public static ApiResponse Fail(List<ApiError> apiErrors) => new() { Success = false, Errors = apiErrors };

        public static implicit operator ActionResult(ApiResponse result)
        {
            return new ObjectResult(result)
            { 
                StatusCode = StatusCodes.Status200OK
            };
        }
    }

    public record ApiError(string Code, string Message);
}
