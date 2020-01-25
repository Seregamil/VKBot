using System;

using VK_bot.Utils;

using VkNet;
using VkNet.Model.GroupUpdate;

namespace VK_bot.Actions.Group
{
    public class Leave
    {
        public static void Execute(VkApi api, GroupUpdate action)
        {
            var memberId = action.GroupLeave.UserId.Value;
            var memberName = UserFunctions.GetNameById(api, memberId);

            Console.WriteLine($"[{DateTime.Now}] Member {memberName}[ID: {memberId}] leaved from group!");
            TextMessage.Send(api, memberId, "Ну и пiщов тi нахой!\n:*");
        }
    }
}