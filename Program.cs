using System;
using System.Net;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

using VkNet;
using VkNet.Enums.Filters;
using VkNet.Enums.SafetyEnums;
using VkNet.Model;
using VkNet.Model.Attachments;
using VkNet.AudioBypassService.Extensions;

using MoreLinq;

using Microsoft.Extensions.DependencyInjection;
using VkNet.Model.RequestParams;


namespace VK_bot
{
    class Program
    {
        public const string AccessToken = "ee1d817c3736efca799d562e81e5d5938cc3b45835b70af26bdc1fe56f90e5b0b1c41aa75fe68cc22a33e";
        public const ulong GroupId = 183376662;

        static void Main(string[] args)
        {
            var services = new ServiceCollection();
            services.AddAudioBypass();

            var api = new VkApi(services);
            api.Authorize(new ApiAuthParams
            {
                AccessToken = AccessToken
            });

            while (true)
            {
                var server = api.Groups.GetLongPollServer(GroupId);
                var poll = api.Groups.GetBotsLongPollHistory(new BotsLongPollHistoryParams()
                {
                    Server = server.Server,
                    Ts = server.Ts,
                    Key = server.Key,
                    Wait = 25
                });

                if (poll?.Updates == null)
                    continue;

                Task.Factory.StartNew(() => {
                    foreach (var update in poll.Updates)
                    {
                        if (update.Type == GroupUpdateType.MessageNew)
                        {
                            var message = update.MessageNew.Message;
                            var userName = GetNameById(api, message.FromId);
                            
                            Console.WriteLine($"[{message.Date.Value}] {userName} {message.Text}");
                            
                            var attachmentsCount = GetMessageAttachments(api, message, message.FromId.Value);
                            var forwardCount = RecursiveForwardMessages(api, message, message.FromId.Value);

                            if(attachmentsCount != 0 || forwardCount != 0)
                                continue;


                            var text = message.Text.Trim();
                            
                            var indexOfDash = text.IndexOf('-');
                            if(indexOfDash != -1 && (indexOfDash == 2 || indexOfDash == 3 )) {
                                var translateText = text;
                                var fromLang = translateText.Substring(0, indexOfDash); // get val of from
                            
                                if(Utils.Translate.LanguageCodes.Contains(fromLang)) {
                                    translateText = translateText.Remove(0, indexOfDash + 1); // rem this val
                                    indexOfDash = translateText.IndexOf(' '); // get pos of next ' '
                                    
                                    var toLang = translateText.Substring(0, indexOfDash);

                                    if(Utils.Translate.LanguageCodes.Contains(toLang)) {
                                        translateText = translateText.Remove(0, indexOfDash).Trim().ToLower(); // rem this val
                                        
                                        Task.Factory.StartNew(() =>
                                        {
                                            var resultOfTranslate = Utils.Translate.Google(translateText, toLang, fromLang);
                                            Console.WriteLine($"Translate {fromLang}-{toLang} {resultOfTranslate}");

                                            api.Messages.Send(new MessagesSendParams
                                            {
                                                RandomId = new Random().Next(),
                                                UserId = message.FromId,
                                                Message = $"{translateText}\n{resultOfTranslate}"
                                            });
                                        });
                                    }
                                }
                            }

                            // chatbot
                            // TODO: https://nlpub.mipt.ru/Russian_Distributional_Thesaurus
                        }
                    }
                });
            }
        }

        public static int RecursiveForwardMessages(VkApi api, Message message, long ownerId, string level = "\t")
        {
            var forwardMessages = message.ForwardedMessages;
            if (forwardMessages.Count > 0)
            {
                foreach (var forwardMessage in forwardMessages)
                {
                    var userName = GetNameById(api, forwardMessage.FromId);

                    Console.WriteLine($"{level}[{forwardMessage.Date.Value}] {userName} {forwardMessage.Text}");
                    GetMessageAttachments(api, forwardMessage, ownerId);

                    if (forwardMessage.ForwardedMessages.Count > 0)
                        RecursiveForwardMessages(api, forwardMessage, ownerId, level + "\t");
                }
            }

            return forwardMessages.Count;
        }

        public static int GetMessageAttachments(VkApi api, Message message, long ownerId)
        {
            if (message.Attachments.Count == 0)
            {
                return 0;
            }

            var attachments = message.Attachments;
            var date = message.Date.Value;

            foreach (var attachment in attachments)
            {
                if (attachment.Type == typeof(Audio))
                {
                    var audio = (Audio)attachment.Instance;
                    var url = audio.Url;
                    var shortUrl = api.Utils.GetShortLink(url, false).ShortUrl;
                    var audioName = $"Artist: {audio.Artist}\n\tTitle: {audio.Title}\n\tLink: {shortUrl}";

                    Console.WriteLine($"\t[{date}] Audio attachment: {audioName}");
                    
                    Task.Factory.StartNew(() => {
                            api.Messages.Send(new MessagesSendParams
                            {
                                RandomId = new Random().Next(),
                                UserId = message.FromId,
                                Message = audioName
                            });
                        }
                    );

                    Task.Factory.StartNew(() => Utils.Download.Audio(audio, $"Audio/{message.FromId}"));
                    continue;
                }

                if (attachment.Type == typeof(Document))
                {
                    var doc = (Document)attachment.Instance;

                    continue;
                }

                if (attachment.Type == typeof(Photo))
                {
                    var photo = (Photo)attachment.Instance;
                    var owner = GetNameById(api, photo.OwnerId);

                    Console.WriteLine($"[{date}] Photo from {owner}");

                    Task.Factory.StartNew(() => Utils.Download.Photo(photo, $"Photo/{owner}_{message.FromId.Value}"));

                    Task.Factory.StartNew(() =>
                    {
                        var msg = string.Empty;

                        var sizes = photo.Sizes.OrderBy(x => x.Height).DistinctBy(x => x.Height).ToList();

                        foreach(var p in sizes) {
                            var shortUrl = api.Utils.GetShortLink(p.Url, false).ShortUrl;
                            msg += $"Size: {p.Height}x{p.Width} - Url: {shortUrl}\n";
                        }

                        api.Messages.Send(new MessagesSendParams
                        {
                            RandomId = new Random().Next(),
                            UserId = ownerId,
                            Message = msg
                        });
                    });
                    continue;
                }

                if (attachment.Type == typeof(AudioMessage))
                {
                    var audio = (AudioMessage)attachment.Instance;
                    
                    var owner = GetNameById(api, audio.OwnerId);
                    var duration = TimeSpan.FromSeconds(audio.Duration);

                    var url = audio.LinkMp3;
                    var shortUrl = api.Utils.GetShortLink(url, false).ShortUrl;

                    Console.WriteLine($"[{date}] Audio message from {owner}. Duration: {duration}. Link: {shortUrl}");

                    Task.Factory.StartNew(() =>
                    {
                        api.Messages.Send(new MessagesSendParams
                        {
                            RandomId = new Random().Next(),
                            UserId = ownerId,
                            Message = $"Owner: {owner}\nDuration: {duration}\nLink: {shortUrl}"
                        });
                    });

                    var folderDate = date.ToString("dd-MM-yyyy");
                    Task.Factory.StartNew(() => Utils.Download.AudioMessage(audio, $"AudioMessage/{owner}_{message.FromId.Value}/{folderDate}"));
                    continue;
                }
            }
            return message.Attachments.Count;
        }

        public static string GetNameById(VkApi api, long? id)
        {
            long userId = (long)id;
            var handle = api.Users.Get(new long[] { userId }).FirstOrDefault();
            var name = $"{handle.FirstName} {handle.LastName}";
            return name;
        }
    }
}
