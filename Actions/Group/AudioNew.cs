using System;

using VkNet;
using VkNet.Model.GroupUpdate;

namespace VK_bot.Actions.Group
{
    public class AudioNew
    {
        public static void Execute(VkApi api, GroupUpdate action)
        {
            var audio = action.Audio;
            var duration = TimeSpan.FromSeconds(audio.Duration);
            bool? isLicensed = false;

            if (audio.IsLicensed != null)
                isLicensed = audio.IsLicensed;

            Console.WriteLine($"[{DateTime.Now}] New audio in group!");
            Console.WriteLine($"\tAudio id: {audio.Id}");
            Console.WriteLine($"\t{audio.Artist} - {audio.Title}");
            Console.WriteLine($"\tDuration: {duration}");
            Console.WriteLine($"\tLicensed? {isLicensed}");
        }
    }
}