using CSharpFunctionalExtensions;
using EmoMeter.Domain.Shared;
using EmoMeter.Web.Responses;
using Microsoft.AspNetCore.Mvc;

namespace EmoMeter.Web.Extensions
{
    public static class ResponseExtensions
    {
        public static ActionResult ToResponse(this ErrorsList errors)
        {
            if (!errors.Any())
            {
                return new ObjectResult(ApiResponse.Fail([]))
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }

            var distinctErrorTypes = errors.Select(e => e.ErrorType).Distinct().ToList();

            var statusCode = distinctErrorTypes.Count() > 1 ?
                StatusCodes.Status500InternalServerError :
                GetStatusCodeForErrorType(distinctErrorTypes.First());

            var apiErrors = errors.Select(e => new ApiError(e.Code, e.Message)).ToList();

            return new ObjectResult(ApiResponse.Fail(apiErrors))
            {
                StatusCode = statusCode
            };
        }

        private static int GetStatusCodeForErrorType(ErrorType type)
            => type switch
            {
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.Conflict => StatusCodes.Status409Conflict,
                ErrorType.Failure => StatusCodes.Status500InternalServerError,
                ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
                _ => StatusCodes.Status500InternalServerError
            };
    }
}
