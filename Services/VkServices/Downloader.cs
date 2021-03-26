using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MoreLinq.Extensions;
using VkNet.Model.Attachments;

namespace CXXY.Services.VkServices
{
    public class Downloader
    {
        private static readonly Regex FileNamePattern = new Regex("[\\|/*?\"<:>]");
        
        public Downloader() {}
        
        public async Task AudioMessage(AudioMessage audio, string folder) 
        {
            if(audio.Id == null)
                return;
            
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            var url = audio.LinkMp3;

            var title = $"{audio.Id.Value}";
            title = FileNamePattern.Replace(title, "");

            var path = $"{folder}/{title}.mp3";
            if (File.Exists(path))
            {
                var fileSize = new FileInfo(path).Length;
                var urlSize = await GetFileSize(url.ToString());

                if (fileSize == urlSize)
                { // local = web
                    return;
                }

                File.Delete(path);
            }

            var client = new WebClient();
            await client.DownloadFileTaskAsync(url, path);
        }

        public async Task Photo(Photo photoArray, string folder)
        {
            if(photoArray.CreateTime == null)
                return;
            
            var date = photoArray.CreateTime.Value.ToString("dd-MM-yyyy (hh-mm-ss)");

            var sizes = photoArray.Sizes
                .OrderBy(x => x.Height)
                .DistinctBy(x => x.Height)
                .ToList();

            foreach (var photo in sizes)
            {
                var path = $"{folder}/{photo.Height}x{photo.Width}";

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                path += $"/{date}.jpg";

                if (File.Exists(path))
                {
                    var fileSize = new FileInfo(path).Length;
                    var urlSize = await GetFileSize(photo.Url.ToString());

                    if (fileSize == urlSize)
                    {
                        // local = web
                        return;
                    }
                    File.Delete(path);
                }

                var client = new WebClient();
                await client.DownloadFileTaskAsync(photo.Url, path);
            }
        }

        public async Task Audio(Audio audio, string folder)
        {
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            var url = audio.Url;
            var title = $"{audio.Artist} - {audio.Title}";
            title = FileNamePattern.Replace(title, "");

            var path = $"{folder}/{title}.mp3";
            if (File.Exists(path))
            {
                var fileSize = new FileInfo(path).Length;
                var urlSize = await GetFileSize(url.ToString());

                if (fileSize == urlSize)
                { // local = web
                    return;
                }
                File.Delete(path);
            }

            var client = new WebClient();
            await client.DownloadFileTaskAsync(url, path);
        }

        private async Task<long> GetFileSize(string url)
        {
            long result = -1;

            var req = WebRequest.Create(url);
            req.Method = "HEAD";
            using var resp = await req.GetResponseAsync();
            
            if (long.TryParse(resp.Headers.Get("Content-Length"), out var contentLength))
                result = contentLength;
            
            return result;
        }
    }
}