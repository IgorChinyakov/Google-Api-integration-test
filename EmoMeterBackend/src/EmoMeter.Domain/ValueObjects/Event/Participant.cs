using CSharpFunctionalExtensions;
using EmoMeter.Domain.Shared;
using EmoMeter.Domain.ValueObjects.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmoMeter.Domain.ValueObjects.Event
{
    public class Participant : ValueObject
    {
        public const int PARTICIPANT_NAME_MAX_LENGTH = 60;

        public string Name { get; }

        private Participant(string name)
        {
            Name = name;
        }

        public static Result<Participant, Error> Create(string name)
        {
            if (string.IsNullOrWhiteSpace(name)
                || name.Length > PARTICIPANT_NAME_MAX_LENGTH)
                return Errors.Validation("Participant name");

            return new Participant(name);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Name;
        }
    }
}
