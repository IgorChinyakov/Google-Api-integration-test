using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EmoMeter.Application.Calendar.DTOs
{
    public class Reminders
    {
        [JsonPropertyName("useDefault")]
        public bool UseDefault { get; set; } = false;

        [JsonPropertyName("overrides")]
        public List<Overrides> Overrides { get; set; }
    }

    public class Overrides
    {
        [JsonPropertyName("method")]
        public string Method { get; set; }

        [JsonPropertyName("minutes")]
        public int Minutes { get; set; }
    }
}
