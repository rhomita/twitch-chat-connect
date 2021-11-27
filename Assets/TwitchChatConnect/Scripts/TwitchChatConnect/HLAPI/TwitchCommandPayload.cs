using TwitchChatConnect.Data;

namespace TwitchChatConnect.HLAPI
{
    public abstract class TwitchCommandPayload
    {
        public TwitchChatCommand Command { get; private set; }

        public static TwitchCommandPayload Empty => new TwitchCommandEmpty();

        protected virtual void Initialize()
        {
        }
    }

    public class TwitchCommandEmpty : TwitchCommandPayload { }
}