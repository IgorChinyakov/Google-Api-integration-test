using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmoMeter.Domain.ValueObjects.Ids
{
    public class UserId : ValueObject, IComparable<UserId>
    {
        //ef core
        public UserId()
        {
        }

        private UserId(Guid value)
        {
            Value = value;
        }

        public Guid Value { get; }

        public static UserId Create(Guid value)
            => new(value);

        public static UserId New()
            => new(Guid.NewGuid());

        public static UserId Empty()
            => new(Guid.Empty);

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public int CompareTo(UserId? other)
        {
            if (other == null)
                throw new Exception("UserId cannot be null");

            return Value.CompareTo(other.Value);
        }
    }
}
