using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;
using EmoMeter.Application.TextAndAudioProcessing.DTOs;
using EmoMeter.Infrastructure.AiAudioProcessingService;
using EmoMeter.Application.TextAndAudioProcessing;

public class STTCalendarService : ITextFromAudioExtractor
{
    private readonly HttpClient _httpClient;
    private readonly STTSettings _settings;

    public STTCalendarService(HttpClient httpClient, IOptions<STTSettings> settings)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
    }

    public async Task<SpeechRecognitionDto?> TranscribeAudioAsync(byte[] audioData)
    {
        var url = $"https://stt.api.cloud.yandex.net/speech/v1/stt:recognize?folderId={_settings.FolderId}&lang={_settings.Language}";

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Api-Key", _settings.ApiKey);

        using (var content = new ByteArrayContent(audioData))
        {
            content.Headers.ContentType = new MediaTypeHeaderValue("audio/ogg");

            var response = await _httpClient.PostAsync(url, content);
            var responseJson = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"Yandex SpeechKit error: {responseJson}");

            var result = JsonSerializer.Deserialize<SpeechRecognitionDto>(responseJson);
            
            return result;
        }
    }
}