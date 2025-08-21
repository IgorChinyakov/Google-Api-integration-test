using CSharpFunctionalExtensions;
using EmoMeter.Domain.ValueObjects.Event;
using EmoMeter.Domain.ValueObjects.Ids;
using EmoMeter.Domain.ValueObjects.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EmoMeter.Domain.Entities
{
    public class Event : Entity<EventId>
    {
        private readonly List<Participant> _participants = [];

        //ef core
        private Event()
        {
        }

        public Title Title { get; private set; }

        public CreatedAt CreatedAt { get; private set; }

        public EventDate EventDate { get; private set; }

        public Location Location { get; private set; }

        public Description Description { get; private set; }

        public User User { get; private set; } = default!;

        public UserId UserId { get; private set; } = default!;

        public Format Format {  get; private set; }

        public IReadOnlyList<Participant> Participants => _participants;

        public Event(
            EventId id,
            CreatedAt createdAt,
            EventDate eventDate,
            Description description,
            Format format,
            Location location,
            Title title) : base(id)
        {
            CreatedAt = createdAt;
            EventDate = eventDate;
            Description = description;
            Format = format;
            Location = location;
            Title = title;
        }

        public void AddParticipant(Participant participant)
            => _participants.Add(participant);
    }
}
