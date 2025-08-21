using CSharpFunctionalExtensions;
using EmoMeter.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmoMeter.Domain.ValueObjects.Event
{
    public class Title : ValueObject
    {
        public const int TITLE_MAX_LENGTH = 50;

        public string Value { get; }

        private Title(string value)
        {
            Value = value;
        }

        public static Result<Title, Error> Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value)
                || value.Length > TITLE_MAX_LENGTH)
                return Errors.Validation("Title");

            return new Title(value);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
