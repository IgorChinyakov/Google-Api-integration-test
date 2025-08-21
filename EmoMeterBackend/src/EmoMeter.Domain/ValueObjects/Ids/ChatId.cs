using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace EmoMeter.Domain.ValueObjects.Ids
{
    public class ChatId : ValueObject
    {
        //ef core
        private ChatId()
        {
        }

        private ChatId(long value)
        {
            Value = value;
        }

        public BigInteger Value { get; }

        public static ChatId Create(long value)
            => new(value);

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
