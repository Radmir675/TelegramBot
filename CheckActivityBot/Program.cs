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
        static async Task Main()
        {
            using var cts = new CancellationTokenSource();
            bot = new TelegramBotClient(_token, cancellationToken: cts.Token);

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