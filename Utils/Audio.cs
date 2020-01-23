using System;
using System.Linq;

using VkNet;
using VkNet.Model.RequestParams;

using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace VK_bot.Utils
{
    /*
        TODO: Сверка по хешам, дабы повторно не качать одинаковый файл
    */

    public static class Audio
    {
        private static Regex fileNamePattern = new Regex("[\\|/*?\"<:>]");

        public static void DownloadMyAudios(this VkApi api, Client client, string path, long count = 0) 
        {
            if(!Directory.Exists(path)) {
                Directory.CreateDirectory(path);
            }

            if(count == 0) {
                count = api.Audio.GetCount(client.Id);
                if(count == 0) {
                    Console.WriteLine($"User '{client.Name}' has no music");
                    return;
                }
            }

            var audioParams = new AudioGetParams();
            audioParams.OwnerId = client.Id;
            audioParams.Count = count;

            var audios = api.Audio.Get(audioParams);
            var downloaded = 0;

            using (var webClient = new WebClient())
            {
                foreach (var audio in audios)
                {
                    var url = audio.Url.DecodeAudioUrl();
                    var title = $"{audio.Artist} - {audio.Title}";
                    title = fileNamePattern.Replace(title, "");

                    Console.WriteLine($"[{downloaded}/{count}] Downloading {title}");

                    try{
                        webClient.DownloadFile(url, $"{path}/{title}.mp3");
                    }
                    catch(Exception e) {
                        Console.WriteLine($"I cant download this file: {title}");
                        Console.WriteLine($"Reason: {e.Message}");
                    }

                    downloaded++;
                }
            }
        }

        public static Uri DecodeAudioUrl(this Uri audioUrl)
        {
            var segments = audioUrl.Segments.ToList();

            segments.RemoveAt((segments.Count - 1) / 2);
            segments.RemoveAt(segments.Count - 1);

            segments[segments.Count - 1] = segments[segments.Count - 1].Replace("/", ".mp3");

            return new Uri($"{audioUrl.Scheme}://{audioUrl.Host}{string.Join("", segments)}{audioUrl.Query}");
        }
    }
}