using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

using System.Threading.Tasks;
using VkNet.Model.Attachments;

namespace VK_bot.Utils
{
    public static class Download
    {
        private static Regex fileNamePattern = new Regex("[\\|/*?\"<:>]");
        
        public static void Audio(Audio audio, string folder)
        {
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            var url = audio.Url;
            var title = $"{audio.Artist} - {audio.Title}";
            title = fileNamePattern.Replace(title, "");

            var path = $"{folder}/{title}.mp3";
            if (File.Exists(path))
            {
                var fileSize = new FileInfo(path).Length;
                var urlSize = GetFileSize(url.ToString());

                if (fileSize == urlSize)
                { // local = web
                    //Console.WriteLine($"Audio {title} already exists. \tCancel download");
                    return;
                }
            }

            WebClient client = new WebClient();
            //Console.WriteLine($"Downloading {title}");

            try
            {
                client.DownloadFileAsync(url, path);
            }
            catch (Exception e)
            {
                //Console.Write($"\t\t\tError! {e.Message}\n");
                // TODO: Add exception to log
            }
        }

        public static void Photo(Photo photo, string folder)
        {
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            var url = photo.BigPhotoSrc;
            var title = $"{photo.Id}";
            title = fileNamePattern.Replace(title, "");

            var path = $"{folder}/{title}.jpg";
            if (File.Exists(path))
            {
                var fileSize = new FileInfo(path).Length;
                var urlSize = GetFileSize(url.ToString());

                if (fileSize == urlSize)
                { 
                    return;
                }
            }

            WebClient client = new WebClient();

            try
            {
                client.DownloadFileAsync(url, path);
            }
            catch (Exception e)
            {
                // TODO: Add exception to log
            }
        }

        private static long GetFileSize(string url)
        {
            long result = -1;

            var req = WebRequest.Create(url);
            req.Method = "HEAD";
            using (var resp = req.GetResponse())
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