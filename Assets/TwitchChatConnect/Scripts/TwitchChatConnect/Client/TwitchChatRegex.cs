using System.Collections.Generic;
using System.Text.RegularExpressions;
using TwitchChatConnect.Data;

namespace TwitchChatConnect.Client
{
    public static class TwitchChatRegex
    {
        public const string LOGIN_FAILED_MESSAGE = "Login authentication failed";
        public const string LOGIN_WRONG_REQUEST_MESSAGE = "Improperly formatted auth";
        public const string LOGIN_SUCCESS_MESSAGE = ":tmi.twitch.tv 001";
        public const string LOGIN_WRONG_USERNAME = "The user token is correct but it does not belong to the given username.";
        public const string COMMAND_NOTICE = "NOTICE";
        public const string COMMAND_PING = "PING :tmi.twitch.tv";
        public const string COMMAND_PONG = "PONG :tmi.twitch.tv";
        public const string COMMAND_JOIN = "JOIN";
        public const string COMMAND_PART = "PART";
        public const string COMMAND_MESSAGE = "PRIVMSG";
        public const string CUSTOM_REWARD_TEXT = "custom-reward-id";
        public const char BADGES_SEPARATOR = ',';
        public const char BADGE_SEPARATOR = '/';

        public const string JoinExpression = @":(.+)!.*JOIN";
        public const string PartExpression = @":(.+)!.*PART";
        public const string MessageExpression = @"badges=(.+?);.*display\-name=(.+?);.*user\-id=(.+?);.*:(.*)!.*PRIVMSG.+?:(.*)";
        public const string RewardExpression = @"badges=(.+?);.*custom\-reward\-id=(.+?);.*display\-name=(.+?);.*user\-id=(.+?);.*:(.*)!.*PRIVMSG.+?:(.*)";
        public const string IdMessageExpression = @"msg-id=(.+?);";
        public const string CheerExpression = @"(?:\s|^)cheer([0-9]+)(?:\s|$)";

        public static Regex JoinRegex { get; private set; } = new Regex(JoinExpression);
        public static Regex PartRegex { get; private set; } = new Regex(PartExpression);
        public static Regex MessageRegex { get; private set; } = new Regex(MessageExpression);
        public static Regex RewardRegex { get; private set; } = new Regex(RewardExpression);
        public static Regex IdMessageRegex { get; private set; } = new Regex(IdMessageExpression);
        public static Regex CheerRegex { get; private set; } = new Regex(CheerExpression, RegexOptions.IgnoreCase);

        public static List<TwitchUserBadge> BuildBadges(string badgesText)
        {
            List<TwitchUserBadge> badges = new List<TwitchUserBadge>();
            foreach (string badge in badgesText.Split(BADGES_SEPARATOR))
            {
                string[] badgeSplit = badge.Split(BADGE_SEPARATOR);
                string name = badgeSplit[0];
                if (badgeSplit.Length != 2) continue; // It should contains two parts <badge name>/<version>
                string versionText = badgeSplit[1];
                if (!int.TryParse(versionText, out int version)) version = 0;
                badges.Add(new TwitchUserBadge(name, version));
            }
            return badges;
        }
    }
}