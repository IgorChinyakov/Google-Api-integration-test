using EmoMeter.Application.Abstractions;
using EmoMeter.Application.Database.DTOs;
using EmoMeter.Application.Features.Commands.CheckRegistration;
using EmoMeter.Application.Features.Commands.Create;
using EmoMeter.Application.Features.Commands.CreateEvent;
using EmoMeter.Application.Features.Commands.ExtractEventFromAudio;
using EmoMeter.Application.Features.Commands.ExtractEventFromText;
using EmoMeter.Application.Features.Commands.GetAccessAndRefreshTokens;
using EmoMeter.Application.Features.Commands.UpdateUserNotifyBeforeMinutes;
using EmoMeter.Application.Features.Queries.GetEventsWithPagination;
using EmoMeter.Application.Features.Queries.GetUserInfo;
using EmoMeter.Web.Extensions;
using EmoMeter.Web.Requests;
using EmoMeter.Web.Responses;
using Microsoft.AspNetCore.Mvc;

namespace EmoMeter.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        [HttpPost]
        public async Task<ActionResult> Register(
            [FromBody] CreateUserRequest request,
            [FromServices] ICommandHandler<string, CreateUserCommand> handler,
            CancellationToken cancellationToken = default)
        {
            var command = new CreateUserCommand(request.Email, request.ChatId, request.NotifyBeforeMinutes);

            var result = await handler.Handle(command, cancellationToken);
            if (result.IsFailure)
                return result.Error.ToResponse();

            if (result.Value == "ok")
                return ApiResponse.Ok(new RegisterResponse("ok", null));
            else
                return ApiResponse.Ok(new RegisterResponse("need_auth", result.Value));
        }

        [HttpPost("{chatId:long}/registration-check")]
        public async Task<ActionResult> CheckIfRegistered(
            [FromRoute] long chatId,
            [FromServices] ICommandHandler<CheckUserRegistrationCommand> handler,
            CancellationToken cancellationToken = default)
        {
            var command = new CheckUserRegistrationCommand(chatId);

            var result = await handler.Handle(command, cancellationToken);
            if (result.IsFailure)
                return result.Error.ToResponse();

            return ApiResponse.Ok();
        }

        [HttpGet("{chatId:long}/user-info")]
        public async Task<ActionResult> GetUserInfo(
            [FromRoute] long chatId,
            [FromServices] IQueryHandlerWithResult<UserDto, GetUserInfoQuery> handler,
            CancellationToken cancellationToken = default)
        {
            var query = new GetUserInfoQuery(chatId);

            var result = await handler.Handle(query, cancellationToken);
            if (result.IsFailure)
                return result.Error.ToResponse();

            return ApiResponse.Ok(result.Value);
        }

        [HttpGet("callback")]
        public async Task<ActionResult> GetAccessAndRefreshTokens(
            [FromQuery] string code,
            [FromQuery] string state,
            [FromServices] ICommandHandler<CreateCallbackCommand> handler,
            CancellationToken cancellationToken = default)
        {
            var command = new CreateCallbackCommand(code, state);

            var result = await handler.Handle(command, cancellationToken);
            if (result.IsFailure)
                return result.Error.ToResponse();

            return Content("ok");
        }

        [HttpGet("{chatId:long}/events")]
        public async Task<ActionResult> GetUserEvents(
            [FromRoute] long chatId,
            [FromQuery] GetUserEventsRequest request,
            [FromServices] IQueryHandler<EventDto, GetEventsWithPaginationQuery> handler, 
            CancellationToken cancellationToken = default)
        {
            var query = new GetEventsWithPaginationQuery(
                chatId, 
                request.Page, 
                request.PageSize, 
                request.Start, 
                request.End);

            var result = await handler.Handle(query, cancellationToken);

            return ApiResponse.Ok(result);
        }

        [HttpPost("{chatId:long}/events")]
        public async Task<ActionResult> AddEvent(
            [FromBody] CreateEventRequest request,
            [FromRoute] long chatId,
            [FromServices] ICommandHandler<EventDto, CreateEventCommand> handler)
        {
            var command = new CreateEventCommand(
                chatId,
                new EventDto(
                    request.Title, 
                    request.Description, 
                    request.Start, 
                    request.End, 
                    request.IsOnline, 
                    request.Location, 
                    request.Participants));

            var result = await handler.Handle(command);
            if (result.IsFailure)
                return result.Error.ToResponse();

            return ApiResponse.Ok(result.Value);
        }

        [HttpPut("{chatId:long}/user-info/notification")]
        public async Task<ActionResult> UpdateUserNotificationMinutes(
            [FromRoute] long chatId,
            [FromBody] UpdateUserNotifyBeforeMinutesRequest request,
            [FromServices] ICommandHandler<UpdateUserNotifyBeforeMinutesCommand> handler,
            CancellationToken cancellationToken = default)
        {
            var command = new UpdateUserNotifyBeforeMinutesCommand(chatId, request.Minutes);

            var result = await handler.Handle(command, cancellationToken);
            if (result.IsFailure)
                return result.Error.ToResponse();

            return ApiResponse.Ok();
        }
    }
}
