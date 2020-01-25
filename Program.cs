using System;
using System.Net;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

using VK_bot.Utils;

using VkNet;
using VkNet.Enums.Filters;
using VkNet.Enums.SafetyEnums;
using VkNet.Model;
using VkNet.Model.Attachments;
using VkNet.AudioBypassService.Extensions;

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

                Task.Factory.StartNew(() =>
                { // новый таск на события в целом
                    foreach (var action in poll.Updates)
                    {
                        if (action.Type == GroupUpdateType.MessageNew)
                        {
                            Task.Factory.StartNew(() => VK_bot.Actions.Group.MessageNew.Execute(api, action));
                        }
                        if (action.Type == GroupUpdateType.AudioNew)
                        {
                            Task.Factory.StartNew(() => VK_bot.Actions.Group.AudioNew.Execute(api, action));
                        }
                        if (action.Type == GroupUpdateType.GroupJoin)
                        {
                            Task.Factory.StartNew(() => VK_bot.Actions.Group.Join.Execute(api, action));
                        }
                        if (action.Type == GroupUpdateType.GroupLeave)
                        {
                            Task.Factory.StartNew(() => VK_bot.Actions.Group.Leave.Execute(api, action));
                        }
                        if (action.Type == GroupUpdateType.WallPostNew)
                        {

                        }
                    }
                });
            }
        }
    }
}
