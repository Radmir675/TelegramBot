using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace CheckActivityBot
{
    class Program
    {
        private static string _token = "8370463377:AAH-d41mSM_dSkfbsYBHapNcg9akE9cXJUI";

        private static ITelegramBotClient bot;
        private static ReceiverOptions _receiverOptions;
        private static User? botInfo;
        private static DateTime startTime = DateTime.Now;
        private static int messagesReceived = 0;

        static async Task Main()
        {
            using var cts = new CancellationTokenSource();
            bot = new TelegramBotClient(_token, cancellationToken: cts.Token);

            // Delete webhook if it exists to allow polling
            await bot.DeleteWebhook(cancellationToken: cts.Token);
            Console.WriteLine("Webhook deleted successfully.");

            botInfo = await bot.GetMe();
            Console.WriteLine($"Hello, World! I am user {botInfo.Id} and my name is {botInfo.FirstName}.");

            _receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new[]
                {
                    UpdateType.Message,
                }
            };

            // Запускаем бота
            bot.StartReceiving(UpdateHandler, ErrorHandler, _receiverOptions, cts.Token);
            Console.WriteLine($"{botInfo.FirstName} запущен!");

            // await StartServerASP(cts);
        }

        private static async Task StartServerASP(CancellationTokenSource cts)
        {
            // Создаем и запускаем веб-сервер
            var builder = WebApplication.CreateBuilder();
            builder.Services.AddCors();

            var app = builder.Build();

            app.UseCors(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

            // Главная страница с информацией о боте
            app.MapGet("/", () =>
            {
                var uptime = DateTime.Now - startTime;
                var html = $@"
<!DOCTYPE html>
<html>
<head>
    <title>Telegram Bot Status</title>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 40px; background: #f5f5f5; }}
        .container {{ max-width: 800px; margin: 0 auto; background: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
        h1 {{ color: #2c3e50; text-align: center; }}
        .status {{ background: #2ecc71; color: white; padding: 10px; border-radius: 5px; text-align: center; margin: 20px 0; }}
        .info {{ background: #ecf0f1; padding: 15px; border-radius: 5px; margin: 10px 0; }}
        .label {{ font-weight: bold; color: #34495e; }}
        .refresh {{ text-align: center; margin-top: 20px; }}
        button {{ background: #3498db; color: white; border: none; padding: 10px 20px; border-radius: 5px; cursor: pointer; }}
        button:hover {{ background: #2980b9; }}
    </style>
</head>
<body>
    <div class='container'>
        <h1>🤖 Telegram Bot Dashboard</h1>
        <div class='status'>✅ Бот работает</div>
        
        <div class='info'>
            <div><span class='label'>Имя бота:</span> {botInfo?.FirstName ?? "N/A"}</div>
        </div>
        
        <div class='info'>
            <div><span class='label'>Username:</span> @{botInfo?.Username ?? "N/A"}</div>
        </div>
        
        <div class='info'>
            <div><span class='label'>ID:</span> {botInfo?.Id ?? 0}</div>
        </div>
        
        <div class='info'>
            <div><span class='label'>Время работы:</span> {uptime.Days}д {uptime.Hours}ч {uptime.Minutes}м {uptime.Seconds}с</div>
        </div>
        
        <div class='info'>
            <div><span class='label'>Получено сообщений:</span> {messagesReceived}</div>
        </div>
        
        <div class='info'>
            <div><span class='label'>Запущен:</span> {startTime:dd.MM.yyyy HH:mm:ss}</div>
        </div>
        
        <div class='refresh'>
            <button onclick='location.reload()'>🔄 Обновить</button>
        </div>
    </div>
</body>
</html>";
                return Results.Content(html, "text/html; charset=utf-8");
            });

            // API endpoint для получения статуса в JSON
            app.MapGet("/api/status", () => new
            {
                Status = "running",
                BotName = botInfo?.FirstName,
                BotUsername = botInfo?.Username,
                BotId = botInfo?.Id,
                Uptime = DateTime.Now - startTime,
                MessagesReceived = messagesReceived,
                StartTime = startTime
            });

            // Запускаем веб-сервер на порту 5000
            app.Urls.Add("http://0.0.0.0:5000");

            Console.WriteLine("Веб-сервер запущен на http://0.0.0.0:5000");
            Console.WriteLine("Нажмите Ctrl+C для остановки...");

            await app.RunAsync(cts.Token);
        }

        private static async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                switch (update.Type)
                {
                    case UpdateType.Message:
                        {
                            messagesReceived++;
                            var message = update.Message;
                            Console.WriteLine($"Получено сообщение от {message?.From?.FirstName}: {message?.Text}");

                            if (message?.Text?.ToLower() == "/start")
                            {
                                await botClient.SendMessage(message.Chat, "Добро пожаловать! Бот работает и готов к использованию.");
                            }
                            else if (message?.Text?.ToLower() == "/status")
                            {
                                var uptime = DateTime.Now - startTime;
                                await botClient.SendMessage(message.Chat,
                                    $"🤖 Статус бота:\n" +
                                    $"✅ Работает: {uptime.Days}д {uptime.Hours}ч {uptime.Minutes}м\n" +
                                    $"📨 Обработано сообщений: {messagesReceived}");
                            }
                            return;
                        }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private static Task ErrorHandler(ITelegramBotClient botClient, Exception error, CancellationToken cancellationToken)
        {
            // Тут создадим переменную, в которую поместим код ошибки и её сообщение 
            var ErrorMessage = error switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => error.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }
    }
}