using CSharpFunctionalExtensions;
using EmoMeter.Application.Calendar.DTOs;
using EmoMeter.Domain.Shared;
using EmoMeter.Domain.ValueObjects.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmoMeter.Application.Calendar
{
    public interface ICalendarService
    {
        Task CreateEventAsync(string accessToken, CalendarEventDto calendarEventDto);

        Task<OAuthTokens?> ExchangeCodeForTokensAsync(string code);

        string GenerateAuthLink(string state);

        Task<OAuthTokens?> RefreshAccessTokenAsync(string refreshToken);
    }
}
