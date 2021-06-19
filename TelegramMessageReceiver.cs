using System;
using System.Threading.Tasks;
using Telegram.Bot;

namespace Telegram_Text_Bot
{
    public class TelegramReceiver
    {
        private const string TG_KEY = "1711379622:AAHV-6bwjYQXWweQZAHii793Vr1_tZo_JeI";
        
        public TelegramBotClient InitReceiver()
        {
            try
            {
                var botClient = new TelegramBotClient(TG_KEY);
                var me = botClient.GetMeAsync().Result;
                Console.WriteLine($"Init start as user {me.Id}.");

                return botClient;
            }
            catch (Exception e)
            {
                Console.WriteLine("Init failed. " + e);
                throw;
            }
        }
    }
}