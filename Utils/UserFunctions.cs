namespace VK_bot.Utils
{
    using System.Linq;
    using VkNet;

    public class UserFunctions
    {
        public static string GetNameById(VkApi api, long? id) {
            long userId = (long)id;
            var handle = api.Users.Get(new long[] { userId }).FirstOrDefault();
            var name = $"{handle.FirstName} {handle.LastName}";
            return name;
        }
    }
}