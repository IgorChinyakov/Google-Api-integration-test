using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmoMeter.Domain.ValueObjects.Event
{
    public class IsConfirmed : ValueObject
    {
        public bool Value { get; }

        private IsConfirmed(bool value)
        {
            Value = value;
        }

        public static IsConfirmed Create(bool value)
            => new(value);

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
