using CSharpFunctionalExtensions;
using EmoMeter.Domain.Shared;
using EmoMeter.Domain.ValueObjects.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmoMeter.Domain.ValueObjects.Event
{
    public class Description : ValueObject
    {
        public const int DESCRIPTION_MAX_LENGTH = 400;

        public string Value { get; }

        private Description(string value)
        {
            Value = value;
        }

        public static Result<Description, Error> Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value)
                || value.Length > DESCRIPTION_MAX_LENGTH)
                return Errors.Validation("Login");

            return new Description(value);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
