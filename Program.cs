using System;
using Telegram.Bot;
using Telegram.Bot.Args;

namespace Telegram_Text_Bot
{
    class Program
    {
        private static ILog _logger;
        private static TelegramBotClient _tgBotClient;
        
        static void Main(string[] args)
        {
            _logger = new ConsoleLog();

            _tgBotClient = new TelegramReceiver().InitReceiver();
            _tgBotClient.OnMessage += BotOnMessage;
            _tgBotClient.StartReceiving();
            
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
            
            _tgBotClient.StopReceiving();
        }
        
        private static async void BotOnMessage(object sender, MessageEventArgs e)
        {
            if (e.Message.Text == null) return;
            
            _logger.WriteLog($"Пришло сообщение в чат {e.Message.Chat.Id} от пользователя {e.Message.Chat.Username}.");
            _logger.WriteLog($"Входящее сообщение: {e.Message.Text}.");

            var textProcessor = new ChingChongText(_logger);
            var result = textProcessor.TransformText(e.Message.Text);
                
            await _tgBotClient.SendTextMessageAsync(
                chatId: e.Message.Chat,
                text: result
            );
        }
    }
}