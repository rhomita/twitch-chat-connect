namespace TwitchChatConnect.Data
{
    public class TwitchChatMessage
    {
        public TwitchUser User { get; protected set; }
        public string Message { get; protected set; }

        public TwitchChatMessage()
        {
        }
        
        public TwitchChatMessage(TwitchUser user, string message)
        {
            Message = message;
            User = user;
        }
    }
}