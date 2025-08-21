using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmoMeter.Domain.ValueObjects.Ids
{
    public class EventId : ValueObject, IComparable<EventId>
    {
        private EventId(Guid value)
        {
            Value = value;
        }

        public Guid Value { get; }

        public static EventId Create(Guid value)
            => new(value);

        public static EventId New()
            => new(Guid.NewGuid());

        public static EventId Empty()
            => new(Guid.Empty);

        public int CompareTo(EventId? other)
        {
            if (other == null)
                throw new Exception("EventId cannot be null");

            return Value.CompareTo(other.Value);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
