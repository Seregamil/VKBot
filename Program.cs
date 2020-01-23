using System;
using System.Net;
using System.Linq;

using VK_bot.Utils;

using VkNet;
using VkNet.Enums.Filters;
using VkNet.Model;
using VkNet.AudioBypassService.Extensions;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using VkNet.Model.RequestParams;

namespace VK_bot
{
    class Program
    {
        public const string AccessToken = "ee1d817c3736efca799d562e81e5d5938cc3b45835b70af26bdc1fe56f90e5b0b1c41aa75fe68cc22a33e";
        public const ulong ApplicationId = 6095910;

        static void Main(string[] args)
        {
            var services = new ServiceCollection();
            services.AddAudioBypass();

            var api = new VkApi(services);

            /*Console.WriteLine("Enter your login: ");
            var login = Console.ReadLine();

            Console.WriteLine("Enter your password: ");
            var password = Console.ReadLine();*/

            var login = "79237120807";
            var password = "CXXYmachine1397";

            api.Authorize(new ApiAuthParams
            {
                ApplicationId = ApplicationId,
                Login = login,
                Password = password,
                Settings = Settings.All

            });

            var client = new Client(api.Users.Get(new long[] { api.UserId.Value }).FirstOrDefault());

            Console.WriteLine($"Welcome {client.Name}");

            /*api.Messages.Send(new VkNet.Model.RequestParams.MessagesSendParams {
                UserId = 253627036, //Id получателя
                Message = "Пишу сообщение с приложения", //Сообщение
                RandomId = new Random().Next(999999) //Уникальный идентификатор                
            });

            Console.WriteLine(users.TotalCount);*/

            var audioParams = new AudioGetParams();
            audioParams.OwnerId = client.Id;
            audioParams.Count = 10;

            var audios = api.Audio.Get(audioParams);

            foreach (var audio in audios)
            {
                Audio.Download(audio, $"Audio/{client.Id}");
            }
            
            /*var getLongPollServer = api.Messages.GetLongPollServer();
            Console.WriteLine(getLongPollServer.Key);*/
        }
    }
}
