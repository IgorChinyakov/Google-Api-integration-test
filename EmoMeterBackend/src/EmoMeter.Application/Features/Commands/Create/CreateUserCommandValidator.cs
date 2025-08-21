using EmoMeter.Application.Extensions;
using EmoMeter.Domain.ValueObjects.Shared;
using EmoMeter.Domain.ValueObjects.User;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmoMeter.Application.Features.Commands.Create
{
    public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
    {
        public CreateUserCommandValidator()
        {
            RuleFor(c => c.Email).MustBeValueObject(Email.Create);

            RuleFor(c => c.NotifyBeforeMinutes).MustBeValueObject(NotifyBeforeMinutes.Create);
        }
    }
}
