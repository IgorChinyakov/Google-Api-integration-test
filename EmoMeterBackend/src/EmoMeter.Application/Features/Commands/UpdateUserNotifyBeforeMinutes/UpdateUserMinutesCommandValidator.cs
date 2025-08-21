using EmoMeter.Application.Extensions;
using EmoMeter.Domain.ValueObjects.Shared;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmoMeter.Application.Features.Commands.UpdateUserNotifyBeforeMinutes
{
    public class UpdateUserMinutesCommandValidator : 
        AbstractValidator<UpdateUserNotifyBeforeMinutesCommand>
    {
        public UpdateUserMinutesCommandValidator()
        {
            RuleFor(u => u.Minutes).MustBeValueObject(NotifyBeforeMinutes.Create);
        }
    }
}
