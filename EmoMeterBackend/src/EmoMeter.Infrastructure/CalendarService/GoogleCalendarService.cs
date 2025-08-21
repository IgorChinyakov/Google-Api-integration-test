using CSharpFunctionalExtensions;
using EmoMeter.Application.Calendar;
using EmoMeter.Application.Calendar.DTOs;
using EmoMeter.Domain.Shared;
using EmoMeter.Domain.ValueObjects.User;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EmoMeter.Infrastructure.CalendarService
{
    public class GoogleCalendarService : ICalendarService
    {
        private readonly HttpClient _httpClient;
        private readonly GoogleCalendarSettings _settings;

        public GoogleCalendarService(HttpClient httpClient, IOptions<GoogleCalendarSettings> settings)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
        }

        // Генерация ссылки для авторизации пользователя
        public string GenerateAuthLink(string state)
        {
            var url = "https://accounts.google.com/o/oauth2/v2/auth" +
                     $"?client_id={_settings.ClientId}" +
                     $"&redirect_uri={Uri.EscapeDataString(_settings.RedirectUri)}" +
                     $"&response_type=code" +
                     $"&scope={Uri.EscapeDataString("https://www.googleapis.com/auth/calendar https://www.googleapis.com/auth/userinfo.email")}" +
                     $"&access_type=offline" +
                     $"&prompt=consent" +
                     $"&state={Uri.EscapeDataString(state)}";

            return url;
        }

        // Обмен authorization code на access и refresh токены
        public async Task<OAuthTokens?> ExchangeCodeForTokensAsync(string code)
        {
            var parameters = new Dictionary<string, string>
            {
                { "grant_type", "authorization_code" },
                { "code", code },
                { "client_id", _settings.ClientId },
                { "client_secret", _settings.ClientSecret },
                { "redirect_uri", _settings.RedirectUri }
            };

            var content = new FormUrlEncodedContent(parameters);
            var response = await _httpClient.PostAsync("https://oauth2.googleapis.com/token", content);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var tokens = JsonSerializer.Deserialize<OAuthTokens>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return tokens;
        }

        public async Task CreateEventAsync(string accessToken, CalendarEventDto calendarEventDto)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var url = "https://www.googleapis.com/calendar/v3/calendars/primary/events";

            var jsonContent = JsonSerializer.Serialize(calendarEventDto);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();
        }

        public async Task<OAuthTokens?> RefreshAccessTokenAsync(string refreshToken)
        {
            var parameters = new Dictionary<string, string>
            {
                { "grant_type", "refresh_token" },
                { "refresh_token", refreshToken },
                { "client_id", _settings.ClientId },
                { "client_secret", _settings.ClientSecret }
            };

            var content = new FormUrlEncodedContent(parameters);
            var response = await _httpClient.PostAsync("https://oauth2.googleapis.com/token", content);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            var tokens = JsonSerializer.Deserialize<OAuthTokens>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return tokens;
        }
    }
}
