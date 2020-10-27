using System.Text.RegularExpressions;

namespace TwitchChatConnect.Data
{
    public class TwitchChatCommand : TwitchChatMessage
    {
        public string Command { get; private set; }
        public string[] Parameters { get; private set; }

        public TwitchChatCommand(TwitchUser user, string message)
        {
            User = user;
            Message = message;

            string[] parameters = message.Split(' ');

            Parameters = new string[parameters.Length - 1];
            Command = parameters[0].ToLower();

            for (int i = 1; i < parameters.Length; i++)
            {
                Parameters[i - 1] = Regex.Replace(parameters[i].ToLower(), "@", "");
            }

        }
    }
}