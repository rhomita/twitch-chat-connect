using System.Text.RegularExpressions;

namespace TwitchChatConnect.Data
{
    public class TwitchChatCommand : TwitchChatMessage
    {
        public string Command { get; }
        public string[] Parameters { get; }

        public TwitchChatCommand(TwitchUser user, string message, int bits, string idMessage) : base(user, message, bits, idMessage)
        {
            string[] parameters = message.Split(' ');

            Parameters = new string[parameters.Length - 1];
            Command = parameters[0];

            for (int i = 1; i < parameters.Length; i++)
            {
                Parameters[i - 1] = Regex.Replace(parameters[i].ToLower(), "@", "");
            }
        }
    }
}