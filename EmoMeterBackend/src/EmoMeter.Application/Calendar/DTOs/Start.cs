using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EmoMeter.Application.Calendar.DTOs
{
    public record Start
    {
        [JsonPropertyName("dateTime")]
        public string DateTime { get; set; }

        [JsonPropertyName("timeZone")]
        public string TimeZone { get; set; }
    }
}
