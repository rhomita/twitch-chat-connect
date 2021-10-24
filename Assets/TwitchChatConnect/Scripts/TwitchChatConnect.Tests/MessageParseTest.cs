using NUnit.Framework;
using System.Text.RegularExpressions;
using TwitchChatConnect.Client;

public class MessageParseTest
{
    private const string JOIN_PAYLOAD = @"< JOIN #<channel>> 
    :<user>!<user>@<user>.tmi.twitch.tv JOIN #<channel>
    > :<user>.tmi.twitch.tv 353 <user> = #<channel> :<user>
    > :<user>.tmi.twitch.tv 366 <user> #<channel> :End of /NAMES list";

    private const string PART_PAYLOAD = @"<PART #<channel>> :<user>!<user>@<user>.tmi.twitch.tv PART #<channel>";

    [Test]
    public void JoinRegex()
    {
        string username = TwitchChatRegex.JoinRegex.Match(JOIN_PAYLOAD).Groups[1].Value;
        Assert.IsTrue(username == "<user>");
    }

    [Test]
    public void PartRegex()
    {       
        Match match = TwitchChatRegex.PartRegex.Match(PART_PAYLOAD);
        Assert.IsTrue(match.Value == ":<user>!<user>@<user>.tmi.twitch.tv PART");
    }
}
