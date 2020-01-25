namespace VK_bot.Utils
{
    using System;
    using System.Net;
    using System.Linq;
    using System.Collections.ObjectModel;
    using System.Collections.Generic;

    using VK_bot.Utils;

    using VkNet;
    using VkNet.Model;
    using VkNet.Model.Attachments;
    using VkNet.Model.RequestParams;

    public class TextMessage
    {
        private VkNet.Model.Message message;
        public TextMessageBody Body;

        public VkNet.Model.Message Handle => message;

        public TextMessage(VkApi api, VkNet.Model.Message message)
        {
            this.message = message;
            Body = new TextMessageBody(message);
        }

        public struct TextMessageBody
        {
            public string Text;
            public DateTime Date;
            public bool? Out; // 0 - input; 1 - output; reply - without value
            public ReadOnlyCollection<Attachment> Attachments;
            public bool HasAttachments;

            public TextMessageBody(VkNet.Model.Message message)
            {
                this.Text = message.Text;
                this.Date = DateTime.Now;
                this.Out = message.Out;

                this.Attachments = message.Attachments;
                this.HasAttachments = Attachments.Count == 0 ? false : true;
            }
        }

        public static void Send(VkApi api, long userId, string message, IEnumerable<VkNet.Model.Attachments.MediaAttachment> attachments = null)
        {
            api.Messages.Send(new MessagesSendParams
            {
                RandomId = new Random().Next(),
                UserId = userId,
                Message = message,
                Attachments = attachments
            });
        }
    }
}