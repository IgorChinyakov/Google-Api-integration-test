using Azure;
using EmoMeter.Application.TextAndAudioProcessing;
using EmoMeter.Application.TextAndAudioProcessing.DTOs;
using EmoMeter.Infrastructure.AITextProcessingService;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OpenAI;
using OpenAI.Chat;
using System;
using System.Data;
using System.Dynamic;
using System.Net.Http;
using System.Text;
using System.Text.Json;

public class GptCalendarService : IAiService
{
    private readonly HttpClient _httpClient;
    private readonly GptSettings _settings;

    public GptCalendarService(HttpClient httpClient, IOptions<GptSettings> settings)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
    }

    public async Task<CalendarEvent?> ParseEventFromTextAsync(string inputText)
    {
        var GroqApiUrl = "https://api.groq.com/openai/v1/chat/completions";
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.ApiKey}");

        var request = new
        {
            model = "llama-3.3-70b-versatile",
            messages = new[]
             {
                new { role = "system", content = @"
You are an expert calendar assistant. Extract the following details from the text:
- Title of event
- Begin Date (in format YYYY-MM-DD HH-MM)
- End Date (in format YYYY-MM-DD HH-MM)
- Is online (Default false)
- A very concise description of the meeting
- List of people (Default is Empty array)
- Location (Deffault = home)
- List of members (example: [Petya, Vanya, Ivan])
if the user has sent the url, then the IsOnline = true and location = URL.
Return ONLY a JSON object with these fields:Title, BeginDate, EndDate, IsOnline, Description, Location, Participants. 
If some information is missing, set the field to default value, default year = 2025.
Example: {""Title"":""meeting with friends"", ""Begin date"":""2025-12-25 15:00"", ""End Date"":""2025-12-2516:00 "", ""Description"":""Team meeting"", ""IsOnline"":""true"" ""peopleList"":""{Vanya, Katya, Ivan}"", ""Location"": ""Home""}" },
                new { role = "user", content = inputText }
            },
            response_format = new { type = "json_object" }
        };

        var json = JsonConvert.SerializeObject(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(GroqApiUrl, content);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();

        var groqResponse = JsonConvert.DeserializeObject<GroqResponse>(responseJson);
        var cont = groqResponse?.Choices?.FirstOrDefault()?.Message?.Content;

        if (string.IsNullOrWhiteSpace(cont))
            return new CalendarEvent();

        var eventData = JsonConvert.DeserializeObject<CalendarEvent>(cont);
        return eventData;
    }
}