using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmoMeter.Infrastructure.CalendarService
{
    public class GoogleCalendarSettings
    {
        public const string GOOGLE = "Google";

        public string ClientId { get; set; } = string.Empty;
        
        public string ClientSecret { get; set; } = string.Empty;

        public string RedirectUri { get; set; } = string.Empty;
    }
}
