using CSharpFunctionalExtensions;
using EmoMeter.Domain.Shared;

namespace EmoMeter.Domain.ValueObjects.Shared
{
    public class NotifyBeforeMinutes : ValueObject
    {
        //ef core
        public NotifyBeforeMinutes()
        {
        }

        private NotifyBeforeMinutes(int value)
        {
            Value = value;
        }

        public int Value { get; }

        public static Result<NotifyBeforeMinutes, Error> Create(int value)
        {
            if (value < 0)
                return Errors.Validation("NotifyBeforeMinutes");

            return new NotifyBeforeMinutes(value);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}