using EmoMeter.Application.Abstractions;
using EmoMeter.Application.Database.DTOs;
using EmoMeter.Application.Features.Commands.ExtractEventFromAudio;
using EmoMeter.Application.Features.Commands.ExtractEventFromText;
using EmoMeter.Web.Extensions;
using EmoMeter.Web.Requests;
using EmoMeter.Web.Responses;
using Microsoft.AspNetCore.Mvc;

namespace EmoMeter.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EventsController : ControllerBase
    {
        [HttpPost("event-extraction-from-text")]
        public async Task<ActionResult> ExtractEventFromText(
            [FromBody] ExtractEventFromTextRequest request,
            [FromServices] ICommandHandler<EventDto, ExtractEventFromTextCommand> handler)
        {
            var command = new ExtractEventFromTextCommand(request.Text);

            var result = await handler.Handle(command);
            if (result.IsFailure)
                return result.Error.ToResponse();

            return ApiResponse.Ok(result.Value);
        }

        [HttpPost("event-extraction-from-audio")]
        public async Task<ActionResult> ExtractEventFromAudio(
            [FromForm] IFormFile file,
            [FromServices] ICommandHandler<string, ExtractEventFromAudioCommand> handler)
        {
            using var stream = file.OpenReadStream();

            var command = new ExtractEventFromAudioCommand(stream);

            var result = await handler.Handle(command);
            if (result.IsFailure)
                return result.Error.ToResponse();

            return ApiResponse.Ok(result.Value);
        }
    }
}
