using System.Collections.Generic;
using System.Text.RegularExpressions;
using TwitchChatConnect.Client;
using TwitchChatConnect.Data;
using TwitchChatConnect.Manager;

namespace TwitchChatConnect.Parser
{
    public class TwitchChatMessageParser
    {
        public string Id { get; }
        public string Sent { get; }
        public TwitchUser User { get; }
        public int Bits { get; }
        public IReadOnlyList<TwitchUserBadge> Badges { get; }

        public TwitchChatMessageParser(TwitchInputLine command)
        {
            Bits = 0;

            Id = TwitchChatRegex.IdMessageRegex.Match(command.Message).Groups[1].Value;
            string badgesText = TwitchChatRegex.MessageRegex.Match(command.Message).Groups[1].Value;
            string displayName = TwitchChatRegex.MessageRegex.Match(command.Message).Groups[2].Value;
            string idUser = TwitchChatRegex.MessageRegex.Match(command.Message).Groups[3].Value;
            string username = TwitchChatRegex.MessageRegex.Match(command.Message).Groups[4].Value;
            Sent = TwitchChatRegex.MessageRegex.Match(command.Message).Groups[5].Value;

            Badges = TwitchChatRegex.BuildBadges(badgesText);
            User = TwitchUserManager.AddUser(username);
            User.SetData(idUser, displayName, Badges);

            if (Sent.Length == 0) return;

            MatchCollection matches = TwitchChatRegex.CheerRegex.Matches(Sent);
            foreach (Match match in matches)
            {
                if (match.Groups.Count != 2) continue; // First group is 'cheerXX', second group is XX.
                string value = match.Groups[1].Value;
                if (!int.TryParse(value, out int bitsAmount)) continue;
                Bits += bitsAmount;
            }
        }
    }
}