using CSharpFunctionalExtensions;
using EmoMeter.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmoMeter.Domain.ValueObjects.User
{
    public class Login : ValueObject
    {
        public const int LOGIN_MAX_LENGTH = 60;

        public string Value { get; }

        private Login(string value)
        {
            Value = value;
        }

        public static Result<Login, Error> Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value) 
                || value.Length > LOGIN_MAX_LENGTH)
                return Errors.Validation("Login");

            return new Login(value);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
