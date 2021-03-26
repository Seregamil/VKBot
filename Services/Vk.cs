using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CXXY.Services.VkServices;
using Microsoft.Extensions.DependencyInjection;
using MoreLinq.Extensions;
using VkNet;
using VkNet.AudioBypassService.Extensions;
using VkNet.Enums.SafetyEnums;
using VkNet.Model;
using VkNet.Model.Attachments;
using VkNet.Model.RequestParams;

namespace CXXY.Services
{
    public class Vk
    {
        private VkApi _api;
        private ulong _groupId;
        private string _accessToken;
        
        private readonly Thread _thread;
        private static bool _doesEnabled = true;
        
        public Vk(ulong groupId, string accessToken)
        {
            _groupId = groupId;
            _accessToken = accessToken;
            
            var services = new ServiceCollection();
            services.AddAudioBypass();

            _api = new VkApi(services);
            _api.Authorize(new ApiAuthParams
            {
                AccessToken = accessToken
            });

            _thread = new Thread(VkThread);
            _thread.Start();
        }

        public void Stop()
        {
            _doesEnabled = false;
        }
        
        private async void VkThread()
        {
            Console.WriteLine($"Thread VK API was initialized");

            while (_doesEnabled)
            {
                var server = await _api.Groups.GetLongPollServerAsync(_groupId);
                var poll = await _api.Groups
                    .GetBotsLongPollHistoryAsync(new BotsLongPollHistoryParams()
                    {
                        Server = server.Server,
                        Ts = server.Ts,
                        Key = server.Key,
                        Wait = 25
                    });

                if (poll?.Updates == null)
                    continue;

                foreach (var update in poll.Updates)
                {
                    if (update.Type != GroupUpdateType.MessageNew)
                        continue;

                    var message = update.MessageNew.Message;
                    var userName = await GetNameById(message.FromId);

                    if (message.Date == null)
                        return;

                    Console.WriteLine($"[{message.Date.Value}] {userName} {message.Text}");

                    if (message.FromId == null)
                        return;

                    var attachmentsCount = await GetMessageAttachments(message, message.FromId.Value);
                    var forwardCount = await RecursiveForwardMessages(message, message.FromId.Value);

                    if (attachmentsCount != 0 || forwardCount != 0)
                        continue;
                }
            }

            Console.WriteLine($"Thread VK API was disposed");
        }
        
        private async Task<int> RecursiveForwardMessages(Message message, long ownerId, string level = "\t")
        {
            var forwardMessages = message.ForwardedMessages;
            if (forwardMessages.Count <= 0) 
                return forwardMessages.Count;
            
            foreach (var forwardMessage in forwardMessages)
            {
                var userName = await GetNameById(forwardMessage.FromId);

                if (forwardMessage.Date != null)
                    Console.WriteLine($"{level}[{forwardMessage.Date.Value}] {userName} {forwardMessage.Text}");
                
                await GetMessageAttachments(forwardMessage, ownerId);

                if (forwardMessage.ForwardedMessages.Count > 0)
                    await RecursiveForwardMessages(forwardMessage, ownerId, level + "\t");
            }

            return forwardMessages.Count;
        }

        private async Task<int> GetMessageAttachments(Message message, long ownerId)
        {
            if (message.Attachments.Count == 0)
            {
                return 0;
            }

            var attachments = message.Attachments;
            if (message.Date == null) 
                return message.Attachments.Count;
            
            var date = message.Date.Value;

            foreach (var attachment in attachments)
            {
                if (attachment.Type == typeof(Audio))
                {
                    var audio = (Audio) attachment.Instance;
                    
                    var url = audio.Url;
                    
                    var shortUrl = _api.Utils
                        .GetShortLink(url, false)
                        .ShortUrl;
                    
                    var audioName = $"Artist: {audio.Artist}\n\tTitle: {audio.Title}\n\tLink: {shortUrl}";

                    Console.WriteLine($"\t[{date}] Audio attachment: {audioName}");

                    await this.SendMessage(message.FromId, audioName);
                    await Task.Factory.StartNew(() => new Downloader().Audio(audio, $"Audio/{message.FromId}"));
                    continue;
                }

                if (attachment.Type == typeof(Document))
                {
                    continue;
                }

                if (attachment.Type == typeof(Photo))
                {
                    var photo = (Photo)attachment.Instance;
                    var owner = await GetNameById(photo.OwnerId);

                    Console.WriteLine($"[{date}] Photo from {owner}");

                    if (message.FromId == null)
                        continue;
                    
                    await Task.Factory.StartNew(async () =>
                    {
                        var msg = string.Empty;

                        var sizes = photo.Sizes
                            .OrderBy(x => x.Height)
                            .DistinctBy(x => x.Height).ToList();

                        foreach(var p in sizes) {
                            var shortUrl = _api.Utils.GetShortLink(p.Url, false).ShortUrl;
                            msg += $"Size: {p.Height}x{p.Width} - Url: {shortUrl}\n";
                        }

                        await this.SendMessage(ownerId, msg);
                    });
                    
                    await Task.Factory.StartNew(async () => await new Downloader().Photo(photo, $"Photo/{owner}_{message.FromId.Value}"));
                    continue;
                }

                if (attachment.Type == typeof(AudioMessage))
                {
                    var audio = (AudioMessage)attachment.Instance;
                    
                    var owner = await GetNameById(audio.OwnerId);
                    var duration = TimeSpan.FromSeconds(audio.Duration);

                    var url = audio.LinkMp3;
                    var shortUrl = _api.Utils.GetShortLink(url, false).ShortUrl;

                    Console.WriteLine($"[{date}] Audio message from {owner}. Duration: {duration}. Link: {shortUrl}");
                    await SendMessage(ownerId, $"Owner: {owner}\nDuration: {duration}\nLink: {shortUrl}");

                    var folderDate = date.ToString("dd-MM-yyyy");
                    if (message.FromId == null) 
                        continue;
                    
                    await Task.Factory.StartNew(async () => await new Downloader().AudioMessage(audio,
                            $"AudioMessage/{owner}_{message.FromId.Value}/{folderDate}"));
                }
            }

            return message.Attachments.Count;
        }

        private async Task<string> GetNameById(long? id)
        {
            var userId = (long)id;
            var handle = _api.Users
                .Get(new long[] { userId })
                .FirstOrDefault();
            var name = $"{handle.FirstName} {handle.LastName}";
            return name;
        }

        public async Task SendMessage(long? userId, string msg)
        {
            await _api.Messages.SendAsync(new MessagesSendParams
            {
                RandomId = new Random().Next(),
                UserId = userId,
                Message = msg
            });
        }
    }
}