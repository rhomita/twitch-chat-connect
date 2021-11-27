using NUnit.Framework;
using TwitchChatConnect.Client;
using TwitchChatConnect.Data;
using TwitchChatConnect.HLAPI;
using TwitchChatConnect.Parser;

namespace TwitchChatConnect.Tests
{
    public class HLAPITest
    {
        private const string COMMAND_PREFIX = "!";
        private const string COMMAND_NAME = "!test";
        private const string COMMAND_PARAMS = "string 0 0.2";

        private TwitchCommandHandler _handler { get; set; }
        private TwitchChatCommand _command { get; set; }

        private void Initialize()
        {
            string payload = $"@badge-info=subscriber/1;badges=moderator/1,subscriber/0;client-nonce=60576a3018294221da54321a7397db58;color=;display-name=francoe1;emotes=;first-msg=0;flags=;id=9e4e255d-3c8d-4cdb-88ca-62ab7a3c37a9;mod=1;room-id=130747120;subscriber=1;tmi-sent-ts=1635094303777;turbo=0;user-id=147383910;user-type=mod :francoe1!francoe1@francoe1.tmi.twitch.tv PRIVMSG #rhomita :{COMMAND_NAME} {COMMAND_PARAMS}";
            TwitchInputLine command = new TwitchInputLine(payload, COMMAND_PREFIX);
            TwitchChatMessageParser message = new TwitchChatMessageParser(command);
            _command = new TwitchChatCommand(message.User, message.Sent, message.Bits, message.Id);
            _handler = new TwitchCommandHandler(COMMAND_PREFIX);
        }

        [Test(Description = "Checks that a command can be registered.")]
        public void RegisterTest()
        {
            Initialize();
            Assert.IsTrue(_handler.Register<TwitchCommandEmpty>(COMMAND_NAME, (payload) => { }));
        }

        [Test(Description = "Checks that a command cannot be registered twice.")]
        public void RegisterDuplicatedTest()
        {
            Initialize();
            Assert.IsTrue(_handler.Register<TwitchCommandEmpty>(COMMAND_NAME, (payload) => { }));
            Assert.IsFalse(_handler.Register<TwitchCommandEmpty>(COMMAND_NAME, (payload) => { }));
        }

        [Test(Description = "Checks that a registered command can be processed.")]
        public void ProcessCommand()
        {
            Initialize();
            TwitchCommandEmpty commandPayload = null;
            _handler.Register<TwitchCommandEmpty>(COMMAND_NAME, (payload) => commandPayload = payload);
            _handler.ProcessCommand(_command);
            Assert.IsNotNull(commandPayload);
        }
    }
}