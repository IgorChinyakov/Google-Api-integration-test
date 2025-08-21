using CSharpFunctionalExtensions;
using EmoMeter.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmoMeter.Domain.ValueObjects.Event
{
    public class EventDate : ValueObject
    {
        public DateTime Start { get; }

        public DateTime End { get; }

        private EventDate(DateTime start, DateTime end)
        {
            Start = start;
            End = end;
        }

        public static Result<EventDate, Error> Create(DateTime start, DateTime end)
        {
            if (start < DateTime.UtcNow)
                return Errors.Validation("EventStartTime");

            if (end < DateTime.UtcNow && end < start)
                return Errors.Validation("EventEndTime");

            return new EventDate(start, end);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Start;
            yield return End;
        }
    }
}
