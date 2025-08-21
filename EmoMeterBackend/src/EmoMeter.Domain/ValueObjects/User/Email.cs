using CSharpFunctionalExtensions;
using EmoMeter.Domain.Shared;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EmoMeter.Domain.ValueObjects.User
{
    public class Email : ValueObject
    {
        public const int EMAIL_MAX_LENGTH = 200;
        
        private Email(string value)
        {
            Value = value;
        }

        public string Value { get; }

        public static Result<Email, Error> Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value)
                || !Regex.IsMatch(value, @"^[^@\s]+@[^@\s]+\.[^@\s]+$")
                || value.Length > EMAIL_MAX_LENGTH)
                return Errors.Validation("Email");

            return new Email(value);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
