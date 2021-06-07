using System.Text.RegularExpressions;

namespace TwitchChatConnect.Data
{
    public class TwitchChatReward : TwitchChatMessage
    {
        public string CustomRewardId { get; }

        public TwitchChatReward(TwitchUser user, string message, string customRewardId) : base(user, message, 0)
        {
            CustomRewardId = customRewardId;
        }
    }
}