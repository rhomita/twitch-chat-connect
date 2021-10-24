using System;
using TwitchChatConnect.Config;

namespace TwitchChatConnect.Client
{
    public class TwitchCommand
    {
        public TwitchCommandType Type { get; }
        public string Message { get; }
        public string UserName { get; }

        public TwitchCommand(string message, string prefix)
        {
            Type = TwitchCommandType.UNKNOWN;
            Message = string.Empty;
            UserName = string.Empty;

            if (string.IsNullOrEmpty(message)) return;

            Message = message;
            if (message.StartsWith(TwitchChatRegex.LOGIN_SUCCESS_MESSAGE)) Type = TwitchCommandType.LOGIN;
            else if (message.Contains(TwitchChatRegex.COMMAND_NOTICE) && message.Contains(TwitchChatRegex.LOGIN_FAILED_MESSAGE)) Type = TwitchCommandType.NOTICE;
            else if (message.Contains(TwitchChatRegex.COMMAND_PING)) Type = TwitchCommandType.PING;
            else if (message.Contains(TwitchChatRegex.COMMAND_MESSAGE) && message.Contains(TwitchChatRegex.CUSTOM_REWARD_TEXT)) Type = TwitchCommandType.MESSAGE_REWARD;
            else if (message.Contains(TwitchChatRegex.COMMAND_MESSAGE)) Type = IsCommandPrefix(prefix) ? TwitchCommandType.MESSAGE_COMMAND : TwitchCommandType.MESSAGE_CHAT;
            else if (message.Contains(TwitchChatRegex.COMMAND_JOIN))
            {
                Type = TwitchCommandType.JOIN;
                UserName = TwitchChatRegex.JoinRegex.Match(Message).Groups[1].Value;
            }
            else if (message.Contains(TwitchChatRegex.COMMAND_PART))
            {
                Type = TwitchCommandType.PART;
                UserName = TwitchChatRegex.PartRegex.Match(Message).Groups[1].Value;
            }
        }

        public bool IsValidLogin(TwitchConnectConfig config)
        {
            if (Type != TwitchCommandType.LOGIN) throw new Exception("IsValidLogin can only be used in LOGIN type commands");
            return Message.StartsWith($"{TwitchChatRegex.LOGIN_SUCCESS_MESSAGE} {config.Username}");
        }

        public bool IsCommandPrefix(string prefix)
        {
            string payload = TwitchChatRegex.MessageRegex.Match(Message).Groups[5].Value;
            return payload.StartsWith(prefix);
        }
    }
}
