using EmoMeter.Application.Calendar;
using EmoMeter.Application.Database;
using EmoMeter.Application.TextAndAudioProcessing;
using EmoMeter.Domain.Shared;
using EmoMeter.Infrastructure.AiAudioProcessingService;
using EmoMeter.Infrastructure.AITextProcessingService;
using EmoMeter.Infrastructure.CalendarService;
using EmoMeter.Infrastructure.DbContexts;
using EmoMeter.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmoMeter.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDatabase(configuration);
            services.AddServices(configuration);

            return services;
        }

        private static IServiceCollection AddDatabase(
            this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped(_ => new ApplicationDbContext(
                configuration.GetConnectionString(Constants.DATABASE)!));
            services.AddScoped<IUsersRepository, UsersRepository>();

            return services;
        }

        private static IServiceCollection AddServices(
            this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IAiService, GptCalendarService>();

            services.AddScoped<ICalendarService, GoogleCalendarService>();
            services.AddHttpClient<ICalendarService, GoogleCalendarService>();

            services.AddScoped<ITextFromAudioExtractor, STTCalendarService>();
            services.AddHttpClient<ITextFromAudioExtractor, STTCalendarService>();

            services.Configure<STTSettings>(configuration.GetSection(STTSettings.STT));
            services.Configure<GoogleCalendarSettings>(configuration.GetSection(GoogleCalendarSettings.GOOGLE));
            services.Configure<GptSettings>(configuration.GetSection(GptSettings.GPT));

            return services;
        }
    }
}
