using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using System.Net;
using System.Text;

namespace CheckActivityBot
{
    class Program
    {
        private static string _token = "8370463377:AAH-d41mSM_dSkfbsYBHapNcg9akE9cXJUI";

        private static ITelegramBotClient bot;
        private static ReceiverOptions _receiverOptions;
        private static HttpListener httpListener;
        static async Task Main()
        {
            using var cts = new CancellationTokenSource();
            bot = new TelegramBotClient(_token, cancellationToken: cts.Token);

            // Delete webhook if it exists to allow polling
            await bot.DeleteWebhook(cancellationToken: cts.Token);
            Console.WriteLine("Webhook deleted successfully.");

            var me = await bot.GetMe();
            Console.WriteLine($"Hello, World! I am user {me.Id} and my name is {me.FirstName}.");

            _receiverOptions = new ReceiverOptions // Также присваем значение настройкам бота
            {
                AllowedUpdates = new[] // Тут указываем типы получаемых Update`ов, о них подробнее расказано тут https://core.telegram.org/bots/api#update
                {
                    UpdateType.Message, // Сообщения (текст, фото/видео, голосовые/видео сообщения и т.д.)
                }
            };



            // UpdateHander - обработчик приходящих Update`ов
            // ErrorHandler - обработчик ошибок, связанных с Bot API
            bot.StartReceiving(UpdateHandler, ErrorHandler, _receiverOptions, cts.Token); // Запускаем бота

            // Start web server for deployment
            StartWebServer();

            Console.WriteLine($"{me.FirstName} запущен!");

            Console.ReadLine();

        }
        private static async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            // Обязательно ставим блок try-catch, чтобы наш бот не "падал" в случае каких-либо ошибок
            try
            {
                // Сразу же ставим конструкцию switch, чтобы обрабатывать приходящие Update
                switch (update.Type)
                {
                    case UpdateType.Message:
                        {
                            Console.WriteLine("Пришло сообщение!");
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

        private static void StartWebServer()
        {
            httpListener = new HttpListener();
            httpListener.Prefixes.Add("http://0.0.0.0:5000/");
            httpListener.Start();
            
            Task.Run(() =>
            {
                while (httpListener.IsListening)
                {
                    try
                    {
                        var context = httpListener.GetContext();
                        var response = context.Response;
                        
                        string responseString = "Bot is running!";
                        byte[] buffer = Encoding.UTF8.GetBytes(responseString);
                        
                        response.ContentLength64 = buffer.Length;
                        response.ContentType = "text/plain";
                        response.StatusCode = 200;
                        
                        var output = response.OutputStream;
                        output.Write(buffer, 0, buffer.Length);
                        output.Close();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Web server error: {ex.Message}");
                    }
                }
            });
            
            Console.WriteLine("Web server started on http://0.0.0.0:5000/");
        }

        //static async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken ct)
        //{
        //    if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
        //    {
        //        var message = update.Message;
        //        if (message.Text.ToLower() == "/start")
        //        {
        //            await bot.SendMessage(message.Chat, "Добро пожаловать на борт, добрый путник!");
        //            return;
        //        }
        //        await bot.SendMessage(message.Chat, "Здоров, братан! И тебе не хворать!");
        //    }
        //}
    }
}