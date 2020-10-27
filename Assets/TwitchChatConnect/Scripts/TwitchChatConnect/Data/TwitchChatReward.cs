using System.Text.RegularExpressions;

namespace TwitchChatConnect.Data
{
    public class TwitchChatReward : TwitchChatMessage
    {
        public string CustomRewardId { get; private set; }

        public TwitchChatReward(TwitchUser user, string message, string customRewardId)
        {
            Message = message;
            User = user;
            CustomRewardId = customRewardId;
        }
    }
}