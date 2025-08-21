using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EmoMeter.Application.TextAndAudioProcessing.DTOs
{
    public class SpeechRecognitionDto
    {
        [JsonPropertyName("result")]
        public string Result { get; set; }
    }
}
