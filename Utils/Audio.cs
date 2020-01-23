using System;
using System.Linq;

using VkNet;
using VkNet.Model.RequestParams;

using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.ComponentModel;

namespace VK_bot.Utils
{
    public static class Audio
    {
        private static Regex fileNamePattern = new Regex("[\\|/*?\"<:>]");
        public static void Download(VkNet.Model.Attachments.Audio audio, string folder)
        {
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            var url = DecodeUrl(audio);
            var title = $"{audio.Artist} - {audio.Title}";
            title = fileNamePattern.Replace(title, "");

            var path = $"{folder}/{title}.mp3";
            if(File.Exists(path)) {
                var fileSize = new FileInfo(path).Length;
                var urlSize = GetFileSize(url.ToString());

                if(fileSize == urlSize) { // local = web
                    Console.WriteLine($"Audio {title} already exists. \tCancel download");
                    return;
                }
            }

            WebClient client = new WebClient();
            Console.Write($"Downloading {title}");
            
            try {
                client.DownloadFile(url, path);
                Console.Write("\t\t\tSuccess!\n");
            }
            catch(Exception e) {
                Console.Write($"\t\t\tError! See log by [{DateTime.Now}]\n");
                // TODO: Add exception to log
            }
        }

        public static Uri DecodeUrl(VkNet.Model.Attachments.Audio audio)
        {
            var audioUrl = audio.Url;
            var segments = audioUrl.Segments.ToList();

            segments.RemoveAt((segments.Count - 1) / 2);
            segments.RemoveAt(segments.Count - 1);

            segments[segments.Count - 1] = segments[segments.Count - 1].Replace("/", ".mp3");

            return new Uri($"{audioUrl.Scheme}://{audioUrl.Host}{string.Join("", segments)}{audioUrl.Query}");
        }

        private static long GetFileSize(string url)
        {
            long result = -1;

            System.Net.WebRequest req = System.Net.WebRequest.Create(url);
            req.Method = "HEAD";
            using (System.Net.WebResponse resp = req.GetResponse())
            {
                if (long.TryParse(resp.Headers.Get("Content-Length"), out long ContentLength))
                {
                    result = ContentLength;
                }
            }

            return result;
        }
    }
}