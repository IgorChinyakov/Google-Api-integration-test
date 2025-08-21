using CSharpFunctionalExtensions;
using EmoMeter.Application.Abstractions;
using EmoMeter.Application.Calendar;
using EmoMeter.Application.Calendar.DTOs;
using EmoMeter.Application.Database;
using EmoMeter.Application.Database.DTOs;
using EmoMeter.Application.Extensions;
using EmoMeter.Domain.Entities;
using EmoMeter.Domain.Shared;
using EmoMeter.Domain.ValueObjects.Event;
using EmoMeter.Domain.ValueObjects.Ids;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmoMeter.Application.Features.Commands.CreateEvent
{
    public class CreateEventHandler :
        ICommandHandler<EventDto, CreateEventCommand>
    {
        private readonly IUsersRepository _usersRepository;
        private readonly IValidator<CreateEventCommand> _validator;
        private readonly ICalendarService _calendarService;

        public CreateEventHandler(
            IUsersRepository usersRepository,
            IValidator<CreateEventCommand> validator,
            ICalendarService calendarService)
        {
            _usersRepository = usersRepository;
            _validator = validator;
            _calendarService = calendarService;
        }

        public async Task<Result<EventDto, ErrorsList>> Handle(
            CreateEventCommand command,
            CancellationToken cancellationToken = default)
        {
            var validationResult = await _validator.ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
                return validationResult.ToErrorsList();

            var userResult = await _usersRepository.GetByChatId(
                ChatId.Create(command.ChatId),
                cancellationToken);
            if (userResult.IsFailure)
                return Errors.NotFound("User").ToErrorsList();

            var user = userResult.Value;

            var eventId = EventId.New();
            var createdAt = CreatedAt.Create();
            var isConfirmed = IsConfirmed.Create(true);

            var eventDate = EventDate.Create(
                DateTime.SpecifyKind(command.CreateEventDto.Start, DateTimeKind.Utc),
                DateTime.SpecifyKind(command.CreateEventDto.End, DateTimeKind.Utc)).Value;

            var description = Description.Create(command.CreateEventDto.Description).Value;
            var location = Location.Create(command.CreateEventDto.Location).Value;
            var format = Format.Create(
                command.CreateEventDto.IsOnline == true ?
                Format.FormatType.Online :
                Format.FormatType.Offline);
            var title = Title.Create(command.CreateEventDto.Title).Value;

            var @event = new Event(
                eventId,
                createdAt,
                eventDate,
                description,
                format,
                location,
                title);

            user.AddEvent(@event);

            foreach (var participant in command.CreateEventDto.Participants)
                @event.AddParticipant(Participant.Create(participant).Value);

            var calendarEvent = new CalendarEventDto
            {
                Summary = command.CreateEventDto.Title,
                Description = description.Value,
                Start = new Start { DateTime = eventDate.Start.ToString("yyyy-MM-ddTHH:mm:ss"), TimeZone = "Europe/Moscow" },
                End = new End { DateTime = eventDate.End.ToString("yyyy-MM-ddTHH:mm:ss"), TimeZone = "Europe/Moscow" },
                Attendees = command.CreateEventDto.Participants.Select(
                    p => new Attendee { Email = p + "@gmail.com", DisplayName = p }).ToList(),
                Location = command.CreateEventDto.Location,
                Reminder = new Reminders
                {
                    Overrides = [new Overrides
                    {
                        Method = "popup",
                        Minutes = user.NotifyBeforeMinutes.Value
                    }]
                }
            };

            await _calendarService.CreateEventAsync(user.AuthorizationCredentials!.AccessToken, calendarEvent);

            await _usersRepository.Save(cancellationToken);

            return command.CreateEventDto;
        }
    }
}
