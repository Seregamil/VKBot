using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Text.RegularExpressions;

using System.Threading.Tasks;

using MoreLinq;
using VkNet.Model.Attachments;

namespace VK_bot.Utils
{
    public static class Download
    {
        private static Regex fileNamePattern = new Regex("[\\|/*?\"<:>]");
        
        public static void AudioMessage(AudioMessage audio, string folder) {
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            var url = audio.LinkMp3;
            var title = $"{audio.Id.Value}";
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


        public static void Photo(Photo photoArray, string folder) {
            // Photo/Sergey Milantyev/650x442
            var date = photoArray.CreateTime.Value.ToString("dd-MM-yyyy (hh-mm-ss)");
            var sizes = photoArray.Sizes.OrderBy(x => x.Height).DistinctBy(x => x.Height).ToList();

            foreach(var photo in sizes) {
                var path = $"{folder}/{photo.Height}x{photo.Width}";
                
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                path = path + $"/{date}.jpg";

                if (File.Exists(path))
                {
                    var fileSize = new FileInfo(path).Length;
                    var urlSize = GetFileSize(photo.Url.ToString());

                    if (fileSize == urlSize)
                    { // local = web
                        return;
                    }
                }

                WebClient client = new WebClient();

                try
                {
                    client.DownloadFileAsync(photo.Url, path);
                }
                catch (Exception e)
                {
                    // TODO: Add exception to log
                }
            }
        }

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