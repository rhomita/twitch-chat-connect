using NUnit.Framework;
using TwitchChatConnect.Client;
using TwitchChatConnect.Data;
using TwitchChatConnect.HLAPI;
using TwitchChatConnect.Parser;
using UnityEngine;

namespace TwitchChatConnect.Tests
{
    public class HLAPIAttributes
    {
        private const string COMMAND_PREFIX = "!";
        private const string COMMAND_NAME = "!test";

        private TwitchCommandHandler _handler { get; set; }
        private TwitchChatCommand _command { get; set; }

        protected class TwitchCommandTestAttributes : TwitchCommandPayload
        {
            [TwitchCommandProperty(0, true)]
            public string ParamString { get; set; }

            [TwitchCommandProperty(1, true)]
            public int ParamInt { get; set; }

            [TwitchCommandProperty(2, true)]
            public float ParamFloat { get; set; }

            [TwitchCommandProperty(3, true)]
            public float SecondParamFloat { get; set; }
        }

        private void Initialize(string commandParams)
        {
            string payload = $"@badge-info=subscriber/1;badges=moderator/1,subscriber/0;client-nonce=60576a3018294221da54321a7397db58;color=;display-name=francoe1;emotes=;first-msg=0;flags=;id=9e4e255d-3c8d-4cdb-88ca-62ab7a3c37a9;mod=1;room-id=130747120;subscriber=1;tmi-sent-ts=1635094303777;turbo=0;user-id=147383910;user-type=mod :francoe1!francoe1@francoe1.tmi.twitch.tv PRIVMSG #rhomita :{COMMAND_NAME} {commandParams}";
            TwitchInputLine command = new TwitchInputLine(payload, COMMAND_PREFIX);
            TwitchChatMessageParser message = new TwitchChatMessageParser(command);
            _command = new TwitchChatCommand(message.User, message.Sent, message.Bits, message.Id);
            _handler = new TwitchCommandHandler(COMMAND_PREFIX);
        }

        [Test(Description = "Verifies that all values can be parsed.")]
        public void ParseValuesTest()
        {
            Initialize("string 5165 0,2 0.3");
            TwitchCommandTestAttributes commandPayload = null;
            _handler.Register<TwitchCommandTestAttributes>(COMMAND_NAME, (payload) => commandPayload = payload);
            _handler.ProcessCommand(_command);

            Assert.IsTrue(commandPayload.ParamString == "string", "invalid STRING parse");
            Assert.IsTrue(commandPayload.ParamInt == 5165, "invalid INT parse");
            Assert.IsTrue(commandPayload.ParamFloat == 0.2f, "invalid FLOAT parse");
            Assert.IsTrue(commandPayload.SecondParamFloat == 0.3f, "invalid FLOAT parse");
        }

        [Test(Description = "Checks that an exception is thrown when a required parameter is not sent.")]
        public void RequiredParamTest()
        {
            Initialize("string");
            TwitchCommandTestAttributes commandPayload = null;
            bool hasError = false;
            _handler.Register<TwitchCommandTestAttributes>(COMMAND_NAME, (payload) => commandPayload = payload);
            _handler.Logger.ErrorHandler += e => hasError = true;
            _handler.ProcessCommand(_command);
            Assert.IsTrue(hasError);
        }
    }
}