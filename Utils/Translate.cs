namespace VK_bot.Utils
{
    using System;
    using System.Net;
    using System.Web;
    
    public class Translate
    {
        public static string[] LanguageCodes = new string[] {
            "am", "ar", "eu", "bn", "bg", "ca", "hr", "cs", "da", "nl", "en", "et", "fil", "fi", "fr", "de", "el", "gu", "iw", "hi", "hu", "is", "id", "it", "ja", "kn", "ko", "lv", "lt", "ms", "ml", "mr", "no", "pl", "ro", "ru", "sr", "sk", "sl", "es", "sw", "sv", "ta", "te", "th", "tr", "ur", "uk", "vi", "cy", "chr"
        };

        public static string Google(string text, string toLanguage = "en", string fromLanguage = "ru" )
        {
            var url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl={fromLanguage}&tl={toLanguage}&dt=t&q={HttpUtility.UrlEncode(text)}";
            var webClient = new WebClient
            {
                Encoding = System.Text.Encoding.UTF8
            };
            var result = webClient.DownloadString(url);
            try
            {
                result = result.Substring(4, result.IndexOf("\"", 4, StringComparison.Ordinal) - 4);
                return result;
            }
            catch
            {
                return "Error";
            }
        }
    }
}