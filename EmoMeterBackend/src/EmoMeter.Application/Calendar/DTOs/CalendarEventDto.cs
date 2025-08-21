using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EmoMeter.Application.Calendar.DTOs
{
    public record CalendarEventDto
    {
        [JsonPropertyName("summary")]
        public string Summary { get; set; } = default!;

        [JsonPropertyName("description")]
        public string Description { get; set; } = default!;

        [JsonPropertyName("start")]
        public Start Start { get; set; } = default!;

        [JsonPropertyName("end")]
        public End End { get; set; } = default!;

        [JsonPropertyName("attendees")]
        public List<Attendee> Attendees { get; set; } = [];

        [JsonPropertyName("location")]
        public string Location { get; set; } = default!;

        [JsonPropertyName("reminders")]
        public Reminders Reminder { get; set; } = default!;
    }
}
