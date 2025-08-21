using Microsoft.Extensions.Options;
using Telegram.Bot;
using TelegramBot.Infrastructure.Models;
using TelegramBot.Infrastructure.TelegramBotService;

var builder = WebApplication.CreateBuilder(args);

// 1. Загрузка конфигурации из appsettings.json
builder.Services.Configure<TelegramBotSettings>(
    builder.Configuration.GetSection("TelegramBot")
);

// 2. Регистрация Telegram Bot Client
builder.Services.AddSingleton<ITelegramBotClient>(provider =>
{
    var settings = provider.GetRequiredService<IOptions<TelegramBotSettings>>().Value;
    return new TelegramBotClient(settings.Token);
});

// Add services to the container.
builder.Services.AddHostedService<TelegramBotService>();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
