using System;

using VK_bot.Utils;

using VkNet;
using VkNet.Model.GroupUpdate;

namespace VK_bot.Actions.Group
{
    public class Join
    {
        public static void Execute(VkApi api, GroupUpdate action)
        { 
            var memberId = action.GroupJoin.UserId.Value;
            var memberName = UserFunctions.GetNameById(api, memberId);

            if(action.GroupJoin.JoinType == VkNet.Enums.GroupJoinType.Join) {
                Console.WriteLine($"[{DateTime.Now}] Member {memberName}[ID: {memberId}] joined in group!");
                TextMessage.Send(api, memberId, "Добро пожаловать в группу!\nТут короче список-хуисок команд");
            }
        }
    }
}