using System.Text.RegularExpressions;

namespace TwitchChatConnect.Data
{
    public class TwitchChatMessage
    {
        public TwitchUser User { get; private set; }
        public string Command { get; private set; }
        public string[] Parameters { get; private set; }

        public TwitchChatMessage(TwitchUser _sender, string[] _parameters)
        {
            Parameters = new string[_parameters.Length - 1];
            Command = _parameters[0].ToLower();

            for (int i = 1; i < _parameters.Length; i++)
            {
                Parameters[i - 1] = Regex.Replace(_parameters[i].ToLower(), "@", "");
            }

            User = _sender;
        }
    }
}