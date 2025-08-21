using EmoMeter.Domain.Shared;
using EmoMeter.Web.Responses;
using System.Net;

namespace EmoMeter.Web.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch(HttpRequestException ex)
            {
                var envelope = ApiResponse.Fail([new ApiError("external.services.error", ex.Message)]);
                context.Response.StatusCode = (int)(ex.StatusCode ?? HttpStatusCode.ServiceUnavailable);
                await context.Response.WriteAsJsonAsync(envelope);
            }
            catch (Exception ex)
            {
                var error = Error.Failure("server.internal", ex.Message);
                var envelope = ApiResponse.Fail([new ApiError(error.Code, error.Message)]);

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsJsonAsync(envelope);
            }
        }
    }
}
