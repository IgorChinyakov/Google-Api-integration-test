using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace EmoMeter.Web.Requests
{
    public class CreateUserRequest
    {
        [JsonPropertyName("Email")]
        public string Email { get; set; }

        [JsonPropertyName("ChatId")]
        public long ChatId { get; set; }

        [JsonPropertyName("NotifyBeforeMinutes")]
        public int NotifyBeforeMinutes { get; set; }
    }
}
