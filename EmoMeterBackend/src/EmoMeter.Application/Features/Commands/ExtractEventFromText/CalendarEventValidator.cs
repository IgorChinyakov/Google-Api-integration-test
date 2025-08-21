using FluentValidation;
using EmoMeter.Application.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmoMeter.Domain.ValueObjects.Event;

namespace EmoMeter.Application.Features.Commands.ExtractEventFromText
{
    public class CalendarEventValidator : AbstractValidator<CalendarEvent>
    {
        public CalendarEventValidator()
        {
            RuleFor(c => c.Title)
                .NotNull()
                .MustBeValueObject(Title.Create!);

            RuleFor(c => c.Description)
                .NotNull()
                .MustBeValueObject(Description.Create!);

            RuleFor(c => c.BeginDate).NotNull();

            RuleFor(c => c.EndDate).NotNull();

            RuleFor(c => c.Location)
                .NotNull()
                .MustBeValueObject(Location.Create!);

            RuleFor(c => c.IsOnline)
                .NotNull();

            RuleForEach(c => c.Participants)
                .NotNull()
                .MustBeValueObject(Participant.Create!);
        }
    }
}
