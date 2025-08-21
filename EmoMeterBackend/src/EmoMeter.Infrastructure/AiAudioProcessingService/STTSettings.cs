using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmoMeter.Infrastructure.AiAudioProcessingService
{
    public class STTSettings
    {
        public const string STT = "STT";

        public string ApiKey { get; set; }

        public string FolderId { get; set; }

        public string Language { get; set; }
    }
}
