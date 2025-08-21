using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Requests.Abstractions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Infrastructure.Models;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TelegramBot.Infrastructure.TelegramBotService
{
    public class TelegramBotService : BackgroundService
    {
        private readonly TelegramBotSettings _settings;
        private readonly ITelegramBotClient _botClient;
        private readonly HttpClient _httpClient;
        private readonly ReceiverOptions _receiverOptions;
        private readonly ConcurrentDictionary<string, CreateEventDto> _pendingEvents = new ConcurrentDictionary<string, CreateEventDto>();

        public TelegramBotService(IOptions<TelegramBotSettings> settings)
        {
            _settings = settings.Value;
            _botClient = new TelegramBotClient(_settings.Token);
            _httpClient = new HttpClient();
            _receiverOptions = new ReceiverOptions // Также присваем значение настройкам бота
            {
                AllowedUpdates = _settings.AllowedUpdates
                .Select(Enum.Parse<UpdateType>)
                .ToArray(),
                DropPendingUpdates = true,
            };
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var cts = new CancellationTokenSource();
            _botClient.StartReceiving(HandleUpdateAsync, errorHandler: HandlePollingErrorAsync, _receiverOptions, cts.Token); // Запускаем бота

            var me = await _botClient.GetMe(); // Создаем переменную, в которую помещаем информацию о нашем боте.
            Console.WriteLine($"{me.FirstName} запущен!");
            await Task.Delay(-1); // Устанавливаем бесконечную задержку, чтобы наш бот работал постоянно
        }

        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                if (update.CallbackQuery is { } callbackQuery)
                {
                    await BotOnCallbackQuery(callbackQuery);
                    return;
                }
                // Обрабатываем только текстовые сообщения
                if (update.Message is not { } message)
                    return;

                var chatId = message.Chat.Id;
                var userId = message.From!.Id;
                string text = message.Text;

                // Сначала проверяем голосовые
                if (message.Voice is { } voiceMessage)
                {
                    await HandleVoiceMessage(chatId, voiceMessage);
                    return;
                }

                // Затем проверяем текстовые сообщения
                if (message.Text is not { } messageText)
                    return; // Если это не текст и не голос - выходим
                if (text.Equals("/start", StringComparison.OrdinalIgnoreCase))
                {
                    await HandleStartCommand(chatId, message.From.FirstName);
                    return;
                }
                
                switch (messageText)
                {
                    case "Информация о пользователе":
                        await GetUserFromServer(chatId);
                        break;

                    case "Запланированные события":
                        await RequestEvent(chatId);
                        break;

                    case "Изменить уведомление":
                        await RequestNewNotifyTime(chatId);
                        break;
                    case "Создать событие":
                        await _botClient.SendMessage(
            chatId: chatId,
            text: "Введите описание события");
                        break;
                    default:
                        // Обработка ответов на запросы бота
                        if (message.ReplyToMessage != null)
                        {
                            await HandleUserReply(chatId, message.ReplyToMessage.Text, messageText);
                        }
                        else
                        {
                            // Обработка свободного текста (например, описание события)
                            await HandleEventCreation(chatId, messageText);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private async Task HandleStartCommand(long chatId, string firstName)
        {
            var welcomeMessage = $"👋 Привет, {firstName}!\n\n" +
                                    "Добро пожаловать в CalendarBot!\n\n";
            await _botClient.SendMessage(
                chatId: chatId,
                text: welcomeMessage,
                replyMarkup: new ReplyKeyboardRemove());
            if (!await CheckIfRegistered(chatId))
            {
                await HandleUnauthorized(chatId);
            }
            else
            {
                await _botClient.SendMessage(
                    chatId: chatId,
                    text: "Вы уже зарегистрированы");
                await SendMainMenu(chatId);
            }
        }

        private async Task<bool> CheckForCancellation(long chatId)
        {
            // Получаем последние сообщения пользователя
            var updates = await _botClient.GetUpdates(offset: -1);

            foreach (var update in updates)
            {
                if (update.Message?.Chat.Id == chatId &&
                    update.Message.Text?.ToLower() == "/cancel")
                {
                    return true;
                }
            }
            return false;
        }

        public async Task HandleVoiceMessage(long chatId, Voice voiceMessage)
        {
            try
            {
                // 1. Скачиваем голосовое сообщение
                var file = await _botClient.GetFile(voiceMessage.FileId);
                using var memoryStream = new MemoryStream();
                await _botClient.DownloadFile(file.FilePath, memoryStream);
                byte[] audioBytes = memoryStream.ToArray();

                // 2. Подготовка запроса к STT-сервису (как в вашем примере с AIProcessingApiUrl)
                var content = new MultipartFormDataContent();
                var audioContent = new ByteArrayContent(audioBytes);
                audioContent.Headers.ContentType = MediaTypeHeaderValue.Parse("audio/ogg");
                content.Add(audioContent, "file", "voice_message.ogg");

                // 3. Отправка на STT-эндпоинт (аналогично вашему PostAsync)
                var sttResponse = await _httpClient.PostAsync(
                    _settings.ApiEndpoints.SttProcessingApiUrl, // Ваш эндпоинт для распознавания речи
                    content);
                //var response = await _httpClient.PostAsync(_settings.ApiEndpoints.AiProcessingApiUrl, content);
                var responseContent = await sttResponse.Content.ReadAsStringAsync();
                var responseObject = JsonConvert.DeserializeObject<Models.ApiResponse>(responseContent);

                if (sttResponse.StatusCode == HttpStatusCode.Unauthorized)
                {
                    await HandleUnauthorized(chatId);
                }

                if (responseObject is null || !responseObject.Success)
                {
                    var errorMsg = responseObject?.Errors?.FirstOrDefault()!.Message ?? "Неизвестная ошибка";
                    await _botClient.SendMessage(chatId, $"Ошибка ❌: {errorMsg}");
                    return;
                }

                // 4. Получаем распознанный текст
                var sttResult = responseObject.Data;
                string recognizedText = sttResult.ToString(); // Или другое поле, которое возвращает ваш API

                if (string.IsNullOrEmpty(recognizedText))
                {
                    await _botClient.SendMessage(chatId, "❌ Не удалось распознать текст.");
                    return;
                }
                // 5. Передаем текст в метод HandleEventCreate
                await HandleEventCreation(chatId, recognizedText);
            }
            catch (Exception ex)
            {
                await _botClient.SendMessage(
                    chatId,
                    $"⚠️ Ошибка: {ex.Message}");

                Console.WriteLine($"[Ошибка] HandleVoiceUpdate: {ex}\n" +
                                 $"ChatId: {chatId}\n");
            }
        }
        private async Task GetUserFromServer(long chatId)
        {
            try
            {
                var response = await _httpClient.GetAsync(string.Format($"{_settings.ApiEndpoints.userInfoApiUrl}/{chatId}/user-info"));
                var responseContent = await response.Content.ReadAsStringAsync();
                var responseObject = JsonConvert.DeserializeObject<Models.ApiResponse>(responseContent);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    await HandleUnauthorized(chatId);
                }
                var createdUser = responseObject.Data.ToObject<CreateUserRequest>();
                if (createdUser is null || createdUser is not CreateUserRequest)
                {
                    await _botClient.SendMessage(chatId, "Ошибка вывода информации, попробуйте позже.");
                    return;
                }
                else
                {
                    var eventInfo = new StringBuilder();

                    eventInfo.AppendLine($"📌 *Почта:* {createdUser.Email}");
                    eventInfo.AppendLine($"📅 *Время напоминания:* {createdUser.NotifyBeforeMinutes} минут");

                    await _botClient.SendMessage(
                        chatId: chatId,
                        text: eventInfo.ToString(),
                        parseMode: ParseMode.Markdown);
                }
                return;
            }
            catch
            {
                return;
            }
        }

        private async Task HandleUnauthorized(long chatId)
        {
        
            try
            {
                await _botClient.SendMessage(
                    chatId: chatId,
                    text: "🔒 Вы не авторизованы.",
                    replyMarkup: new ReplyKeyboardRemove()
                    );
                // Создаем кнопку для запроса email
                var keyboard = new InlineKeyboardMarkup(
                    InlineKeyboardButton.WithCallbackData("📧 Авторизоваться", "request_email_auth")
                );

                await _botClient.SendMessage(
                    chatId: chatId,
                    text: "🔒 Для доступа к боту требуется авторизация",
                    replyMarkup: keyboard
                    );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Auth error: {ex}");
                await _botClient.SendMessage(chatId, "⚠ Ошибка при запросе авторизации");
            }
        }
        private async Task<bool> CheckIfRegistered(long chatId)
        {
            try
            {
                var confirmUrl = $"{_settings.ApiEndpoints.checkifregisterApiUrl}/{chatId}/registration-check";
                var emptyContent = new StringContent(string.Empty, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(confirmUrl, emptyContent);
                var responseContent = await response.Content.ReadAsStringAsync();
                var responseObject = JsonConvert.DeserializeObject<Models.ApiResponse>(responseContent);
                if (responseObject is null)
                    return false;
                return responseObject.Success;
            }
            catch
            {
                return false;
            }
        }

        private async Task HandleEmailRegistration(long chatId, string email)
        {
            const int defaultNotifyBeforeMinute = 60; // 60 минут до события
            const int checkIntervalSeconds = 10;
            const int maxAttempts = 20; // 20 попыток ≈ 3 минуты 20 секунд
            const int reminderInterval = 3; // Напоминать каждые 3 проверки (30 секунд)

            try
            {
                var registrationData = new CreateUserRequest
                {
                    ChatId = chatId,
                    Email = email,
                    NotifyBeforeMinutes = defaultNotifyBeforeMinute
                };

                // Сериализуем в JSON
                var json = JsonConvert.SerializeObject(registrationData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Отправляем POST запрос для регистрации
                var response = await _httpClient.PostAsync(_settings.ApiEndpoints.registrationApiUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();
                var responseObject = JsonConvert.DeserializeObject<Models.ApiResponse>(responseContent);

                if (responseObject is null || !responseObject.Success)
                {
                    var errorMsg = responseObject?.Errors?.FirstOrDefault()!.Message ?? "Неизвестная ошибка";
                    await _botClient.SendMessage(chatId, $"Ошибка ❌: {errorMsg}");
                    return;
                }

                // Проверяем успешность запроса
                if (responseObject!.Success)
                {
                    var dto = responseObject.Data.ToObject<RegisterResponse>();
                    if (dto is null || dto is not RegisterResponse)
                    {
                        // Обработка ошибок
                        await _botClient.SendMessage(
                            chatId: chatId,
                            text: $"Произошла ошибка при регистрации, попробуйте позже");
                        return;
                    }
                    if (dto.Status == "ok")
                    {
                        await _botClient.SendMessage(chatId, "Вы уже авторизованы ✅");
                    }
                    else
                    {
                        await _botClient.SendMessage(chatId, $"Авторизуйтесь по ссылке: {dto.AuthUrl}");

                        int attempts = 0;
                        bool isRegistered = false;
                        bool wasCancelled = false;

                        while (attempts < maxAttempts && !isRegistered && !wasCancelled)
                        {

                            wasCancelled = await CheckForCancellation(chatId);
                            if (wasCancelled) break;

                            attempts++;
                            await Task.Delay(checkIntervalSeconds * 1000);

                            if (await CheckIfRegistered(chatId))
                            {
                                isRegistered = true;
                                await _botClient.SendMessage(
                                    chatId: chatId,
                                    text: "✅ Регистрация успешно подтверждена! Теперь вы можете пользоваться всеми функциями бота.");

                                break;
                            }
                            // Периодические уведомления
                            if (attempts % reminderInterval == 0)
                            {
                                var remainingTime = TimeSpan.FromSeconds((maxAttempts - attempts) * checkIntervalSeconds);
                                await _botClient.SendMessage(
                                    chatId: chatId,
                                    text: $"⏳ Ожидаем подтверждения... Осталось времени: {remainingTime:mm\\:ss}\n" +
                                            "Для отмены введите /cancel");

                            }
                        }

                        if (!isRegistered)
                        {
                            await _botClient.SendMessage(
                                chatId: chatId,
                                text: "⌛ Время ожидания истекло. Регистрация не была подтверждена.\n" +
                                        "Если вы перешли по ссылке, но регистрация не завершилась, попробуйте позже или обратитесь в поддержку.");
                            return;
                        }

                    }
                    await SendMainMenu(chatId);
                }
            }
            catch (Exception ex)
            {
                // Обработка ошибок
                await _botClient.SendMessage(
                    chatId: chatId,
                    text: $"Произошла ошибка при регистрации: {ex.Message}");
            }

        }

        private async Task HandleUserReply(long chatId, string replyToMessage, string userReply)
        {
            if (replyToMessage.Contains("Введите ваш email:"))
            {
                await HandleEmailRegistration(chatId, userReply);
                return;
            }
            else if (replyToMessage.Contains("Введите новое время уведомления"))
            {
                if (int.TryParse(userReply, out int notifyTime))
                {
                    await UpdateUserNotifyTime(chatId, notifyTime);
                }
                else
                {
                    await _botClient.SendMessage(chatId, "Пожалуйста, введите число (минуты).");
                }
            }
            await SendMainMenu(chatId);
        }

        private async Task UpdateUserNotifyTime(long chatId, int minutes)
        {
            var json = JsonConvert.SerializeObject(new UpdateUserNotifyBeforeMinutesRequest(minutes));
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{_settings.ApiEndpoints.updateUserApiUrl}/{chatId}/user-info/notification", content);
            var responseContent = await response.Content.ReadAsStringAsync();
            var responseObject = JsonConvert.DeserializeObject<Models.ApiResponse>(responseContent);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                await HandleUnauthorized(chatId);
            }
            if (responseObject is null || !responseObject.Success)
            {
                await _botClient.SendMessage(chatId, "Ошибка изменения параметра, попробуйте позже.");
                return;
            }
            else
            {
                await _botClient.SendMessage(chatId, "Параметр успешно обновлён.");
                return;
            }
        }
        private async Task HandleEventCreation(long chatId, string inputtext)
        {
            //if (!await CheckIfRegistered(chatId)) return;


            // Отправляем POST запрос для создания события
            //var content = new StringContent(JsonConvert.SerializeObject(newEvent), Encoding.UTF8, "application/json");
            //var json = JsonConvert.SerializeObject(new ExtractEventFromTextRequest(text));
            //var content = new StringContent(json, Encoding.UTF8, "application/json");
            var request = new ExtractEventFromTextRequest(inputtext);

            var response = await _httpClient.PostAsJsonAsync(_settings.ApiEndpoints.AiProcessingApiUrl, request);
            var responseContent = await response.Content.ReadAsStringAsync();
            var responseObject = JsonConvert.DeserializeObject<Models.ApiResponse>(responseContent);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                await HandleUnauthorized(chatId);
            }

            if (responseObject is null || !responseObject.Success)
            {
                var errorMsg = responseObject?.Errors?.FirstOrDefault()!.Message ?? "Неизвестная ошибка";
                await _botClient.SendMessage(chatId, $"Ошибка ❌: {errorMsg}");
                return;
            }
            var createdEvent = responseObject.Data.ToObject<CreateEventDto>();
            if (createdEvent is null || createdEvent is not CreateEventDto)
            {
                await _botClient.SendMessage(chatId, "Ошибка создания события, попробуйте позже.");
                return;
            }
            //Сохранение события в временное хранилище.
            string eventId = Guid.NewGuid().ToString("N");
            _pendingEvents.TryAdd(eventId, createdEvent);


            //Преобразование даты к строке и формирование URL
            string startString = createdEvent.Start.ToString("o"); // ISO 8601 format
            string endString = createdEvent.End.ToString("yyyy-MM-ddTHH:mm:ss");

            // 2. Кодируем для URL (важно для специальных символов)
            startString = WebUtility.UrlEncode(startString);
            endString = WebUtility.UrlEncode(endString);
            response = await _httpClient.GetAsync($"{_settings.ApiEndpoints.GetEventsUrl}/{chatId}/events?Page=1&PageSize=5&Start={startString}&End={endString}");

            responseContent = await response.Content.ReadAsStringAsync();
            responseObject = JsonConvert.DeserializeObject<ApiResponse>(responseContent);
            var dtolist = JsonConvert.DeserializeObject<PagedList<CreateEventDto>>(responseObject.Data.ToString());

            if (responseObject is null || !responseObject.Success)
            {
                var errorMsg = responseObject?.Errors?.FirstOrDefault()!.Message ?? "Неизвестная ошибка";
                await _botClient.SendMessage(chatId, $"Ошибка ❌: {errorMsg}");
                return;
            }

            //Проверка регистрации
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                await HandleUnauthorized(chatId);
            }


            // Проверяем наличие событий
            if (dtolist.Items != null && dtolist.Items.Any())
            {
                await _botClient.SendMessage(chatId, "🚨 На это время уже запланировано событие, подтвердите только если вы уверены, что планируте несколько событий на одно время!");
            }

            //Создание Inline клавиатуры.
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Подтвердить", $"confirm_event:{eventId}"),
                    InlineKeyboardButton.WithCallbackData("Отклонить", $"reject_event:{eventId}")
                }
            });

            //Вывод события.
            var eventInfo = new StringBuilder();
            eventInfo.AppendLine("✅ *Подтвердите создание события:*");
            eventInfo.AppendLine();
            eventInfo.AppendLine($"📌 *Название:* {createdEvent.Title}");
            eventInfo.AppendLine($"📅 *Дата начала:* {createdEvent.Start:dd.MM.yyyy} {createdEvent.Start:HH:mm}");
            eventInfo.AppendLine($"📅 *Дата конца:* {createdEvent.End:dd.MM.yyyy} {createdEvent.End:HH:mm}");
            eventInfo.AppendLine(createdEvent.IsOnline ? "🌐 *Онлайн-мероприятие*" : "🏢 *Оффлайн-мероприятие*");
            eventInfo.AppendLine($"📍 *Место:* {createdEvent.Location}");
            eventInfo.AppendLine($"📝 *Описание:* {createdEvent.Description}");

            if (createdEvent.Participants != null && createdEvent.Participants.Any())
            {
                eventInfo.AppendLine();
                eventInfo.AppendLine("👥 *Участники:*");
                foreach (var participant in createdEvent.Participants)
                {
                    eventInfo.AppendLine($"- {participant}");
                }
            }

            await _botClient.SendMessage(
                chatId: chatId,
                text: eventInfo.ToString(),
                parseMode: ParseMode.Markdown,
                replyMarkup: inlineKeyboard);
        }
        private async Task BotOnCallbackQuery(CallbackQuery callbackQuery)
        {
            long chatId = callbackQuery.Message.Chat.Id;
            string data = callbackQuery.Data;

            try
            {
                if (data.StartsWith("confirm_event:"))
                {
                    string eventId = data.Substring("confirm_event:".Length);
                    await _botClient.AnswerCallbackQuery(callbackQuery.Id, "Обрабатываю запрос...");
                    if (_pendingEvents.TryGetValue(eventId, out CreateEventDto eventObj))
                    {
                        await ConfirmEventCreation(chatId, eventObj);
                        _pendingEvents.TryRemove(eventId, out _);
                    }
                    else
                    {
                        await _botClient.SendMessage(chatId, "Событие не найдено или время подтверждения истекло");
                    }
                    return;
                }
                else if (data.StartsWith("reject_event:"))
                {
                    string eventId = data.Substring("reject_event:".Length);
                    await _botClient.AnswerCallbackQuery(callbackQuery.Id, "Обрабатываю запрос...");
                    if (_pendingEvents.TryGetValue(eventId, out var eventObj))
                    {
                        await RejectEventCreation(chatId, eventObj);
                        _pendingEvents.TryRemove(eventId, out _);
                    }
                    else
                    {
                        await _botClient.SendMessage(chatId, "Событие не найдено");
                    }
                    return;
                }
                else if (callbackQuery.Data == "request_email_auth")
                {
                    // Отвечаем на callback (убираем "часики")
                    await _botClient.AnswerCallbackQuery(callbackQuery.Id);

                    // Запрашиваем email
                    await _botClient.SendMessage(
                        chatId: callbackQuery.Message.Chat.Id,
                        text: "Введите ваш email:",
                        replyMarkup: new ForceReplyMarkup { Selective = true });
                }
                else if (data.StartsWith("next_event:"))
                {
                    int newOffset = int.Parse(data.Substring("next_event:".Length));
                    await RequestEvent(chatId, newOffset);
                }
                else if (data.StartsWith("cancel_event:"))
                {
                    // Просто удаляем кнопки
                    await _botClient.EditMessageReplyMarkup(
                        chatId: chatId,
                        messageId: callbackQuery.Message.MessageId,
                        replyMarkup: null);
                    await SendMainMenu(chatId);
                }
                else
                {
                    switch (data)
                    {
                        case "user_info":
                            await GetUserFromServer(chatId);
                            break;
                        case "change_email":
                            await RequestNewEmail(chatId);
                            break;
                        case "change_notify":
                            await RequestNewNotifyTime(chatId);
                            break;
                        case "create_event":
                            await _botClient.SendMessage(
                    chatId: chatId,
                    text: "Введите описание события:");
                            break;
                        default:
                            await _botClient.SendMessage(chatId, "Неизвестная команда.");
                            break;    // Другие обработчики...
                    }
                }

                await _botClient.AnswerCallbackQuery(callbackQuery.Id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
                try
                {
                    await _botClient.AnswerCallbackQuery(
                        callbackQuery.Id,
                        "Произошла ошибка обработки запроса",
                        showAlert: true);
                }
                catch { } // Игнорируем ошибки при отправке уведомления об ошибке
            }
        }

        private async Task SendMainMenu(long chatId)
        {
            // Создаем Reply-клавиатуру
            var replyKeyboard = new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton[]
                {
                    new KeyboardButton("Информация о пользователе"),
                    new KeyboardButton("Запланированные события")
                },
                new KeyboardButton[]
                {
                    new KeyboardButton("Изменить уведомление"),
                    new KeyboardButton("Создать событие")
                }
            })
            {
                ResizeKeyboard = true,
                OneTimeKeyboard = false,
                InputFieldPlaceholder = "Выберите действие" // Подсказка в поле ввода
            };

            await _botClient.SendMessage(
                chatId: chatId,
                text: "Главное меню:",
                replyMarkup: replyKeyboard);
        }

        private async Task RequestNewEmail(long chatId)
        {
            await _botClient.SendMessage(
                chatId: chatId,
                text: "Введите новый email:",
                replyMarkup: new ForceReplyMarkup { Selective = true });
        }

        private async Task RequestNewNotifyTime(long chatId)
        {
            await _botClient.SendMessage(
                chatId: chatId,
                text: "Введите новое время уведомления (в минутах):",
                replyMarkup: new ForceReplyMarkup { Selective = true });
        }

        private async Task ConfirmEventCreation(long chatId, CreateEventDto eventdto)
        {
            // Отправляем POST запрос для подтверждения события
            var confirmUrl = $"{_settings.ApiEndpoints.eventConfirmationApiUrl}/{chatId}/events";
            var response = await _httpClient.PostAsJsonAsync(confirmUrl, eventdto);
            var responseContent = await response.Content.ReadAsStringAsync();
            var responseObject = JsonConvert.DeserializeObject<Models.ApiResponse>(responseContent);
            if (responseObject.Success)
            {
                await _botClient.SendMessage(
                    chatId: chatId,
                    text: "Событие успешно создано!");
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                await HandleUnauthorized(chatId);
            }
            else
            {
                await _botClient.SendMessage(
                    chatId: chatId,
                    text: "Ошибка при подтверждении события.");
            }
        }

        private async Task RejectEventCreation(long chatId, CreateEventDto eventId)
        {
            // Можно отправить запрос на удаление события или просто проигнорировать
            await _botClient.SendMessage(
                chatId: chatId,
                text: "Создание события отменено.");
        }

        private Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Polling error: {exception.Message}");
            return Task.CompletedTask;
        }

        private async Task RequestEvent(long chatId, int offset = 1)
        {
            try
            {
                // 1. Отправляем запрос к API
                var response = await _httpClient.GetAsync($"{_settings.ApiEndpoints.GetEventsUrl}/{chatId}/events?page={offset}&pageSize=5");

                var responseContent = await response.Content.ReadAsStringAsync();
                var responseObject = JsonConvert.DeserializeObject<Models.ApiResponse>(responseContent);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    await HandleUnauthorized(chatId);
                }

                if (responseObject is null || !responseObject.Success)
                {
                    var errorMsg = responseObject?.Errors?.FirstOrDefault()!.Message ?? "Неизвестная ошибка";
                    await _botClient.SendMessage(chatId, $"Ошибка ❌: {errorMsg}");
                    return;
                }

                // 2. Десериализуем ответ
                var content = JsonConvert.DeserializeObject<PagedList<CreateEventDto>>(responseObject.Data.ToString());
                var events = content.Items;

                if (events == null || !events.Any())
                {
                    await _botClient.SendMessage(chatId, offset == 0
                        ? "📭 Нет запланированных событий"
                        : "🏁 Вы просмотрели все события");
                    await SendMainMenu(chatId);
                    return;
                }
                await _botClient.SendMessage(
                    chatId: chatId,
                    text: "Список запланированных событий:",
                    replyMarkup: new ReplyKeyboardRemove());

                // 3. Формируем сообщение
                var eventInfo = new StringBuilder();
                foreach (var evt in events)
                {
                    eventInfo.AppendLine();
                    eventInfo.AppendLine($"📌 *Название:* {evt.Title}");
                    eventInfo.AppendLine($"📅 *Дата начала:* {evt.Start:dd.MM.yyyy} {evt.Start:HH:mm}");
                    eventInfo.AppendLine($"📅 *Дата конца:* {evt.End:dd.MM.yyyy} {evt.End:HH:mm}");
                    eventInfo.AppendLine(evt.IsOnline ? "🌐 *Онлайн-мероприятие*" : "🏢 *Оффлайн-мероприятие*");
                    eventInfo.AppendLine($"📍 *Место:* {evt.Location}");
                    eventInfo.AppendLine($"📝 *Описание:* {evt.Description}");

                    if (evt.Participants != null && evt.Participants.Any())
                    {
                        eventInfo.AppendLine();
                        eventInfo.AppendLine("👥 *Участники:*");
                        foreach (var participant in evt.Participants)
                        {
                            eventInfo.AppendLine($"- {participant}");
                        }
                    }
                    eventInfo.AppendLine();
                }
                
                // 4. Создаем inline-кнопки

                if (!content.HasNextPage)
                {
                    await _botClient.SendMessage(
                        chatId: chatId,
                        text: "Все события выведены",
                        replyMarkup: null);
                    var inlineKeyboard = new InlineKeyboardMarkup(InlineKeyboardButton.WithCallbackData("Назад.", $"cancel_event:{offset + 1}"));
                    await _botClient.SendMessage(
                    chatId: chatId,
                    text: eventInfo.ToString(),
                    parseMode: ParseMode.Markdown,
                    replyMarkup: inlineKeyboard);

                }
                else
                {
                    var inlineKeyboard = new InlineKeyboardMarkup(new[]
                    {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Далее ▶", $"next_event:{offset + 1}"),
                            InlineKeyboardButton.WithCallbackData("Отмена ❌", $"cancel_event:{offset + 1}")
                        }
                    });
                    await _botClient.SendMessage(
                    chatId: chatId,
                    text: eventInfo.ToString(),
                    parseMode: ParseMode.Markdown,
                    replyMarkup: inlineKeyboard);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in RequestEvent: {ex}");
                await _botClient.SendMessage(chatId, "⚠ Произошла ошибка при загрузке событий");
                await SendMainMenu(chatId);
            }
        }
    }
}
