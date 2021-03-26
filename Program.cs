using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CXXY.Services;
using VkNet;
using VkNet.Enums.SafetyEnums;
using VkNet.Model;
using VkNet.Model.Attachments;
using VkNet.AudioBypassService.Extensions;

using MoreLinq;
using Microsoft.Extensions.DependencyInjection;
using VkNet.Abstractions;
using VkNet.Model.RequestParams;
using VkNet.Utils;

namespace CXXY
{
    class Program
    {
        private const string AccessToken = "180e4d0f90ca3075680cb0fe3a037e0e84f3548639d2bb935918222dce59c7086693dc5a9633e3f5531ad";
        private const ulong GroupId = 183376662;
        public static VkApi Api;

        static void Main(string[] args)
        {
            var vk = new Vk(GroupId, AccessToken);

            while (true)
            {
                var line = Console.ReadLine();
                if (line == ":q!")
                {
                    vk.Stop();
                    break;
                }
            }
            /*var services = new ServiceCollection();
            services.AddAudioBypass();

            Api = new VkApi(services);
            Api.Authorize(new ApiAuthParams
            {
                AccessToken = AccessToken
            });

            while (true)
            {
                var server = Api.Groups.GetLongPollServer(GroupId);
                var poll = Api.Groups.GetBotsLongPollHistory(new BotsLongPollHistoryParams()
                {
                    Server = server.Server,
                    Ts = server.Ts,
                    Key = server.Key,
                    Wait = 25
                });

                if (poll?.Updates == null)
                    continue;

                Task.Factory.StartNew(async () => {
                    foreach (var update in poll.Updates)
                    {
                        if (update.Type != GroupUpdateType.MessageNew) 
                            continue;
                        
                        var message = update.MessageNew.Message;
                        var userName = GetNameById(Api, message.FromId);

                        if (message.Date == null)
                            return;

                        Console.WriteLine($"[{message.Date.Value}] {userName} {message.Text}");

                        if (message.FromId == null)
                            return;
                        
                        var attachmentsCount = await GetMessageAttachments(Api, message, message.FromId.Value);
                        var forwardCount = await RecursiveForwardMessages(Api, message, message.FromId.Value);

                        if (attachmentsCount != 0 || forwardCount != 0)
                            continue;
                            
                        var text = message.Text
                            .ToLower()
                            .Trim();

                        if (text.IndexOf("альбом: ", StringComparison.Ordinal) == 0)
                        {
                            var dirName = "Album/";
                            if (!Directory.Exists(dirName))
                                Directory.CreateDirectory(dirName);

                            var ownerIdC = text.Remove(0, 8);
                            long ownerId = 0;
                            try
                            {
                                ownerId = Convert.ToInt64(ownerIdC);
                            }
                            catch
                            {
                                await Utils.Message.Send(message.FromId, "Проблема с парсингом ID");
                                return;
                            }

                            dirName = $"{dirName}{ownerId}/";
                            if(!Directory.Exists(dirName))
                                Directory.CreateDirectory(dirName);

                            var count = 0;
                            if (ownerId < 0)
                                count = await Api.Photo.GetAlbumsCountAsync(null, ownerId);
                            else
                                count = await Api.Photo.GetAlbumsCountAsync(ownerId);
                            
                            await Utils.Message.Send(message.FromId, ownerId < 0
                                ? $"Сохраняем альбомы сообщества {ownerId}"
                                : $"Сохраняем альбомы пользователя {ownerId}");

                            await Utils.Message.Send(message.FromId, $"Общее количество: {count}");
                            
                            var collection = await Api.Photo.GetAlbumsAsync(new PhotoGetAlbumsParams
                            {
                                // AlbumIds = null,
                                // Count = null,
                                // NeedCovers = null,
                                // NeedSystem = null,
                                // Offset = null,
                                OwnerId = ownerId
                            });
                            
                            collection.ForEach(x =>
                            {
                                var collectionDirectory = $"{dirName}{x.Title}";
                                if(!Directory.Exists(collectionDirectory))
                                    Directory.CreateDirectory(collectionDirectory);
                                
                                
                            });
                        }
                    }
                });
            }*/
        }
    }
}
