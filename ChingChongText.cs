using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using Newtonsoft.Json.Linq;

namespace Telegram_Text_Bot
{
    public class ChingChongText : ITextProcessor
    {
        private ILog _logger;
        
        private static Dictionary<string, string> _phrases = new Dictionary<string, string>();
        private static double _lastRequestTime;
        private static readonly string[] VerbsEndings = {"ать","ять","оть","еть","уть","гнуть","у","ю","ем","им","ешь","ишь",
            "ете","ите","ет","ит","ут","ют","ал","ял","ала","яла","али","яли","ол","ел","ола","ела","оли","ели","ул",
            "ула","ули"};

        public ChingChongText(ILog logger)
        {
            this._logger = logger;
        }

        public string TransformText(string source)
        {
            var text = source.Normalize().Trim();
            var words = ParseText(text);
                
            // Пока не нужно заменять форму глаголов, всёравно хуёво работает. {
            //string replacedString = ReplaceVerbs(words);
            //Console.WriteLine("После замены формы глаголов: " + replacedString);
            // }
                
            var str = new StringBuilder();
            foreach (var word in words)
            {
                str.Append(ReplacePhrase(word));
                str.Append(' ');
            }

            _logger.WriteLog("После замены слов: " + str);
                
            //string resultString = ListToString(words);
                
            //Delay between req's {
            var curTime = DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            if ((curTime - 5) < _lastRequestTime)
            {
                Thread.Sleep(5000);
            }
            _lastRequestTime = curTime;
            _logger.WriteLog("Время последнего запроса к гуглу: " + _lastRequestTime);
            // }
                
            var result = TranslateText(str.ToString(), "ru", "zh-CN");
            Thread.Sleep(1000);
            result = TranslateText(result, "zh-CN", "ru");
            _logger.WriteLog($"Исходящее сообщение: {result}.");

            return result;
        }
        
        private List<string> ParseText(string text)
        {
            List<string> words = new List<String>();
            var result = new StringBuilder();
            var word = new StringBuilder();
            char space = ' ';
            
            foreach (var symbol in text)
            {
                if (symbol == space)
                {
                    words.Add(word.ToString());
                    word.Clear();
                } 
                else
                {
                    word.Append(symbol);
                }
            }
            words.Add(word.ToString());
            
            return words;
        }
        
        private bool CheckEnding(string word, string ending)
        {
            var wordEnding = new StringBuilder();

            if (word.Length < 3) return false;
            if ((word.Length - ending.Length) < 0) return false;
            
            for (var i = word.Length - ending.Length; i < word.Length; i++)
            {
                wordEnding.Append(word[i]);
            }
            
            return wordEnding.ToString().Equals(ending);
        }
        
        private string ListToString(List<string> words)
        {
            var result = new StringBuilder();
            
            for (int i = 0; i < words.Count; i++)
            {
                result.Append($"{i+1}: {words[i]} \n");
            }

            return result.ToString();
        }
        
        private string ReplaceVerbs(List<string> words)
        {
            var result = new StringBuilder();
            bool isVerb;
            
            
            foreach (var word in words)
            {
                isVerb = false;
                
                foreach (var ending in VerbsEndings)
                {
                    if (CheckEnding(word, ending))
                    {
                        var newWord = ReplaceEnding(word, ending);
                        result.Append(newWord);
                        result.Append(" ");
                        isVerb = true;
                        break;
                    }
                }
                
                if(isVerb) continue;
                result.Append(word);
                result.Append(" ");
            }

            return result.ToString();
        }

        private string ReplaceEnding(string word, string ending)
        {
            var result = word.Replace(ending, "ть");
            return result;
        }
        
        public string TranslateText(string input, string inLang, string outLang)
        {
            try
            {
                string url = $"https://translate.googleapis.com/translate_a/single?client=gtx" +
                             $"&sl={inLang}" +
                             $"&tl={outLang}&dt=t" +
                             $"&q={Uri.EscapeUriString(input)}&op=translate";
                
                HttpClient httpClient = new HttpClient();
                string jsonText = httpClient.GetStringAsync(url).Result;
                
                JArray jsonArray = JArray.Parse(jsonText);
                StringBuilder result = new StringBuilder();
                
                foreach (var tmp in jsonArray[0])
                {
                    result.Append(tmp[0]);
                }

                return result.ToString();
            }
            catch (Exception e)
            {
                Console.WriteLine("Веб-запрос обосрался. \n" + " Exception message: " + e);
                _logger.WriteLog("Веб-запрос обосрался. \n" + " Exception message: " + e);
                throw;
            }
        }
        
        private string SubstringBetweenSymbols(string str, char preSymbol, char postSymbol)
        {
            int? preSymbolIndex = null;
            int? postSymbolIndex = null;
            
            for (int i = 0; i < str.Length; i++)
            {
                if (i == 0 && preSymbol == char.MinValue)
                {
                    preSymbolIndex = -1;
                }
                if (str[i] == preSymbol && !(preSymbolIndex.HasValue && preSymbol == postSymbol))
                {
                    preSymbolIndex = i;
                }
                if (str[i] == postSymbol && preSymbolIndex.HasValue && preSymbolIndex != i)
                {
                    postSymbolIndex = i;
                }
                if (i == str.Length - 1 && postSymbol == char.MinValue)
                {
                    postSymbolIndex = str.Length;
                }


                if (preSymbolIndex.HasValue && postSymbolIndex.HasValue)
                {
                    var result = str.Substring(preSymbolIndex.Value + 1, postSymbolIndex.Value - preSymbolIndex.Value - 1);
                    return result;
                }
            }
            
            return string.Empty;
        }

        private string ReplacePhrase(string word)
        {
            try
            {
                var keyMatch = false;
                
                foreach (var key in _phrases.Keys)
                {
                    if (word.ToLower() == key) keyMatch = true;
                }

                return keyMatch ? _phrases[word.ToLower()] : word;
            }
            catch (Exception e)
            {
                Console.WriteLine("После этого слова словарь обосрался: " + word + ". \n Exception message: " + e);
                _logger.WriteLog("После этого слова словарь обосрался: " + word + ". \n Exception message: " + e);
                throw;
            }

        }
        
        private void FillDictionary()
        {
            _phrases.Add("анон", "простой Иван город Тверь");
            _phrases.Add("хуй", "нефритовый стержень");
            _phrases.Add("тян", "кошачья жена");
            _phrases.Add("президент", "великий вождь");
            _phrases.Add("обоссал", "осуждение партией");
            _phrases.Add("двачую", "двойные чаепитие");
            _phrases.Add("куколд", "китайский муж обосранная жена");
            _phrases.Add("15", "плюс социальный кредит");
            _phrases.Add("модератор", "секретарь компартии");
            _phrases.Add("еда", "кормление рис");
            _phrases.Add("лол", "много смех");
            _phrases.Add("хохлы", "грязный уйгур");
            _phrases.Add("ролл", "кручение");
            _phrases.Add("битард", "товарищ");
            _phrases.Add("тянка", "китайский жена");
            _phrases.Add("трипл", "тройное попадание");
            _phrases.Add("бамп", "поднятие");
            _phrases.Add("рубль", "юань");
            _phrases.Add("путин", "плешивый моль много ботокса");
            _phrases.Add("айфон", "сяоми");
            _phrases.Add("хит", "удар");
            _phrases.Add("топовый", "горячее ударение");
            _phrases.Add("лучше", "сильный рост");
            _phrases.Add("заебись", "благосостояние");
        }

        private string RemoveSpecialChars(string text)
        {
            string result = text.Replace("\a", " ");
            result = result.Replace("\b", " ");
            result = result.Replace("\t", " ");
            result = result.Replace("\r", " ");
            result = result.Replace("\v", " ");
            result = result.Replace("\f", " ");
            result = result.Replace("\n", " ");
            
            return result;
        }
    }
}