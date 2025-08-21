using CSharpFunctionalExtensions;
using EmoMeter.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmoMeter.Domain.ValueObjects.Event
{
    public class CreatedAt : ValueObject
    {
        public DateTime Value { get; }

        private CreatedAt(DateTime value)
        {
            Value = value;
        }

        public static CreatedAt Create()
            => new(DateTime.UtcNow);

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
