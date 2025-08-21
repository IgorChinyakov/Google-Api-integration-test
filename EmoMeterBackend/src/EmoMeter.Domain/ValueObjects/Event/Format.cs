using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmoMeter.Domain.ValueObjects.Event
{
    public class Format : ValueObject
    {
        public FormatType Value { get; }

        private Format(FormatType value)
        {
            Value = value;
        }

        public static Format Create(FormatType value)
            => new(value);

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public enum FormatType
        {
            Online,
            Offline
        }
    }
}
