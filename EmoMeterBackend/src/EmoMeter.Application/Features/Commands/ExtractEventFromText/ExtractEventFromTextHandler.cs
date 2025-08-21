using CSharpFunctionalExtensions;
using EmoMeter.Application.Abstractions;
using EmoMeter.Application.Database.DTOs;
using EmoMeter.Application.Extensions;
using EmoMeter.Application.TextAndAudioProcessing;
using EmoMeter.Domain.Shared;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmoMeter.Application.Features.Commands.ExtractEventFromText
{
    public class ExtractEventFromTextHandler : ICommandHandler<EventDto, ExtractEventFromTextCommand>
    {
        private readonly IAiService _aiService;
        private readonly IValidator<ExtractEventFromTextCommand> _commandValidator;
        private readonly IValidator<CalendarEvent> _eventValidator;

        public ExtractEventFromTextHandler(
            IAiService aiService,
            IValidator<ExtractEventFromTextCommand> validator,
            IValidator<CalendarEvent> eventValidator)
        {
            _aiService = aiService;
            _commandValidator = validator;
            _eventValidator = eventValidator;
        }

        public async Task<Result<EventDto, ErrorsList>> Handle(
            ExtractEventFromTextCommand command, CancellationToken cancellationToken = default)
        {
            var commandValidationResult = await _commandValidator.ValidateAsync(command, cancellationToken);
            if (!commandValidationResult.IsValid)
                return commandValidationResult.ToErrorsList();

            var extractedEvent = await _aiService.ParseEventFromTextAsync(command.Text);
            if (extractedEvent == null)
                return Error.Failure(
                    "unable.to.recognize.message",
                    "Message is unable to be recognized and parsed to event").ToErrorsList();

            var eventValidationResult = await _eventValidator.ValidateAsync(extractedEvent, cancellationToken);
            if(!eventValidationResult.IsValid)
                return eventValidationResult.ToErrorsList();

            var eventDto = new EventDto(
                extractedEvent.Title!,
                extractedEvent.Description!,
                extractedEvent.BeginDate!.Value,
                extractedEvent.EndDate!.Value,
                extractedEvent.IsOnline!.Value,
                extractedEvent.Location!,
                extractedEvent.Participants);

            return eventDto;
        }
    }
}
