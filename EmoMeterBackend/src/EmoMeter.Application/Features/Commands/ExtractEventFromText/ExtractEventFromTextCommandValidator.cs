using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmoMeter.Application.Features.Commands.ExtractEventFromText
{
    public class ExtractEventFromTextCommandValidator : 
        AbstractValidator<ExtractEventFromTextCommand>
    {
        public ExtractEventFromTextCommandValidator()
        {
            RuleFor(e => e.Text).Must(text => string.IsNullOrWhiteSpace(text) == false);
        }
    }
}
