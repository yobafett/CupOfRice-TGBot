using System;

namespace Telegram_Text_Bot
{
    public class ConsoleLog : ILog
    {
        public void WriteLog(string info)
        {
            try
            {
                var text = DateTime.Now + ": " + info + "\n";
                Console.WriteLine(text);
            }
            catch (Exception e)
            {
                Console.WriteLine("Произошло что-то непонятное, лог не будет записан. Ошибка:\n" + e);
                throw;
            }
        }
    }
}