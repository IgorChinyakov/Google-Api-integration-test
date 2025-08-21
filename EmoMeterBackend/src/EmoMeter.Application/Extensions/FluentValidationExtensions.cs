using CSharpFunctionalExtensions;
using EmoMeter.Domain.Shared;
using FluentValidation;
using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EmoMeter.Application.Extensions
{
    public static class FluentValidationExtensions
    {
        public static IRuleBuilderOptionsConditions<T, TProperty> MustBeValueObject<T, TProperty, TValueObject>(
            this IRuleBuilder<T, TProperty> ruleBuilder, Func<TProperty, Result<TValueObject, Error>> factoryMethod)
        {
            return ruleBuilder.Custom((property, context) =>
            {
                var result = factoryMethod(property);
                if (result.IsFailure)
                    context.AddFailure(result.Error.PropertyName, result.Error.Message);

                return;
            });
        }

        public static ErrorsList ToErrorsList(this ValidationResult validationResult)
        {
            var errors = validationResult.Errors
                .Select(e => new Error(
                    e.ErrorMessage, 
                    e.ErrorCode, 
                    e.PropertyName, 
                    ErrorType.Validation));
            return new ErrorsList(errors);
        }
    }
}
