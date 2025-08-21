using EmoMeter.Application.TextAndAudioProcessing.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmoMeter.Application.TextAndAudioProcessing
{
    public interface ITextFromAudioExtractor
    {
        Task<SpeechRecognitionDto?> TranscribeAudioAsync(byte[] audioData);
    }
}
