using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using VK_bot.Utils;

using VkNet;
using VkNet.Model;
using VkNet.Model.Attachments;
using VkNet.Model.GroupUpdate;

namespace VK_bot.Actions.Group
{
    public class MessageNew
    {
        public static void Execute(VkApi api, GroupUpdate action) {
            // text message
            var message = new TextMessage(api, action.MessageNew.Message);

            var userId = message.Handle.FromId.Value;
            var userName = UserFunctions.GetNameById(api, userId);

            var messageText = message.Body.Text;
            var resultMessage = new List<string>();

            Console.WriteLine($"[{message.Body.Date}] {userName}: {messageText}");
            Console.WriteLine($"{message.Handle.ReplyMessage}");

            if (message.Body.HasAttachments)
            {
                var attachments = message.Body.Attachments;
                foreach (var attachment in attachments)
                {
                    if (attachment.Type == typeof(Audio))
                    { // audio message for download and sending download link
                        var audio = (Audio)attachment.Instance;
                        Console.WriteLine($"\tAudio id: {audio.Id}: {audio.Artist} - {audio.Title}");

                        var link = api.Utils.GetShortLink(audio.Url, false).ShortUrl.ToString();
                        resultMessage.Add($"{audio.Artist} - {audio.Title}: {link}");

                        Task.Factory.StartNew(() => Download.Audio(audio, $"Audio/{userId}_({userName})"));
                    }

                    if(attachment.Type == typeof(Photo)) 
                    {
                        var photo = (Photo)attachment.Instance;
                        Console.WriteLine($"\tPhoto owner: {UserFunctions.GetNameById(api, photo.OwnerId)}");

                        var link = api.Utils.GetShortLink(photo.BigPhotoSrc, false).ShortUrl.ToString();
                        resultMessage.Add($"PhotoId: {photo.Id} - {link}");

                        Task.Factory.StartNew(() => Download.Photo(photo, $"Photo/{userId}_({userName})"));
                    }
                }
            }

            //var userId = message.From.Id;
            if (resultMessage.Count != 0)
            {
                TextMessage.Send(api, userId, string.Join("\n", resultMessage));
                return;
            }

            TextMessage.Send(api, userId, "chat-bot not implemented yet");
        }
    }
}