using CSharpFunctionalExtensions;
using EmoMeter.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmoMeter.Domain.ValueObjects.Event
{
    public class Location : ValueObject
    {
        public const int LOCATION_MAX_LENGTH = 400;

        public string Value { get; }

        private Location(string value)
        {
            Value = value;
        }

        public static Result<Location, Error> Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value)
                || value.Length > LOCATION_MAX_LENGTH)
                return Errors.Validation("Login");

            return new Location(value);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
