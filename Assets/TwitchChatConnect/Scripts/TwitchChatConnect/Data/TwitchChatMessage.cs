namespace TwitchChatConnect.Data
{
    public class TwitchChatMessage
    {
        public TwitchUser User { get; }
        public string Message { get; }
        public bool IsHighlighted { get; }
        public int Bits;

        public TwitchChatMessage(TwitchUser user, string message, int bits, bool isHighlighted)
        {
            Message = message;
            User = user;
            Bits = bits;
            IsHighlighted = isHighlighted;
        }
    }
}