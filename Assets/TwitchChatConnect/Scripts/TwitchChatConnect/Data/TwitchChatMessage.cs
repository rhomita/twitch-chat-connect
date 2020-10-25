using System.Text.RegularExpressions;

public class TwitchChatMessage
{
    public string sender { get; private set; }
    public string command { get; private set; }
    public string[] parameters { get; private set; }

    public TwitchChatMessage(string _sender, string[] _parameters)
    {
        parameters = new string[_parameters.Length - 1];
        command = _parameters[0].ToLower();

        for (int i = 1; i < _parameters.Length; i++)
        {
            parameters[i - 1] = Regex.Replace(_parameters[i].ToLower(), "@", "");
        }

        sender = _sender.ToLower();
    }
}