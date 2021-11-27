using System.Collections.Generic;
using TwitchChatConnect.Client;
using TwitchChatConnect.Data;
using TwitchChatConnect.Manager;

namespace TwitchChatConnect.Parser
{
    public class TwitchChatRewardParser
    {
        public string BadgesText { get; }
        public string Id { get; }
        public string Sent { get; }
        public IReadOnlyList<TwitchUserBadge> Badges { get; }
        public TwitchUser User { get; }

        public TwitchChatRewardParser(TwitchInputLine command)
        {
            BadgesText = TwitchChatRegex.RewardRegex.Match(command.Message).Groups[1].Value;
            Id = TwitchChatRegex.RewardRegex.Match(command.Message).Groups[2].Value;
            string displayName = TwitchChatRegex.RewardRegex.Match(command.Message).Groups[3].Value;
            string idUser = TwitchChatRegex.RewardRegex.Match(command.Message).Groups[4].Value;
            string username = TwitchChatRegex.RewardRegex.Match(command.Message).Groups[5].Value;
            Sent = TwitchChatRegex.RewardRegex.Match(command.Message).Groups[6].Value;
            Badges = TwitchChatRegex.BuildBadges(BadgesText);
            User = TwitchUserManager.AddUser(username);
            User.SetData(idUser, displayName, Badges);
        }
    }
}