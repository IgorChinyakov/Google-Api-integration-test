using EmoMeter.Application.Extensions;
using EmoMeter.Domain.ValueObjects.Event;
using EmoMeter.Domain.ValueObjects.Ids;
using EmoMeter.Domain.ValueObjects.User;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmoMeter.Application.Features.Commands.CreateEvent
{
    public class CreateEventCommandValidator : AbstractValidator<CreateEventCommand>
    {
        public CreateEventCommandValidator()
        {
            RuleFor(c => new { c.CreateEventDto.Start, c.CreateEventDto.End })
                .MustBeValueObject(e => EventDate.Create(e.Start, e.End));

            RuleFor(c => c.CreateEventDto.Description)
                .MustBeValueObject(Description.Create);

            RuleFor(c => c.CreateEventDto.Title)
                .MustBeValueObject(Title.Create);

            RuleFor(c => c.CreateEventDto.Location)
                .MustBeValueObject(Location.Create);

            RuleForEach(c => c.CreateEventDto.Participants)
                .MustBeValueObject(Participant.Create);
        }
    }
}
