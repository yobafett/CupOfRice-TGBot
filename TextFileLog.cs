using System;
using System.IO;
using System.Text;

namespace Telegram_Text_Bot
{
    public class TextFileLog : ILog
    {
        private string _logName;
        
        public TextFileLog(string logName)
        {
            _logName = logName;
            CreateLog();
        }

        private void CreateLog()
        {
            if (!Directory.Exists("logs"))
            {
                Directory.CreateDirectory("logs");
            };
            
            StringBuilder name = new StringBuilder();
            name.Append(@"logs\");
            name.Append(DateTime.Now.ToString("hh.mm.ss_dd.MM.yyyy"));
            name.Append(".txt");
            _logName = name.ToString();

            try
            {
                StreamWriter sw = new StreamWriter(_logName, false, System.Text.Encoding.Default);
                var text = "logfile: " + _logName + "\n";
                sw.WriteLine(text);
                sw.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Произошло что-то непонятное, лог не будет создан. Ошибка:\n" + e);
                throw;
            }
        }

        public void WriteLog(string info)
        {
            try
            {
                StreamWriter sw = new StreamWriter(_logName, true, System.Text.Encoding.Default);
                var text = DateTime.Now + ": " + info + "\n";
                sw.WriteLine(text);
                sw.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Произошло что-то непонятное, лог не будет записан. Ошибка:\n" + e);
                throw;
            }
        }
    }
}