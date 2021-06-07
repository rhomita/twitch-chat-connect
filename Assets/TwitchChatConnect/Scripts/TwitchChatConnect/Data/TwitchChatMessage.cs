namespace TwitchChatConnect.Data
{
    public class TwitchChatMessage
    {
        private static string MESSAGE_HIGHLIGHTED = "highlighted-message";

        private string _idMessage;

        public TwitchUser User { get; }
        public string Message { get; }
        public int Bits;

        public bool IsHighlighted => _idMessage == MESSAGE_HIGHLIGHTED;

        public TwitchChatMessage(TwitchUser user, string message, int bits, string idMessage = "")
        {
            Message = message;
            User = user;
            Bits = bits;
            _idMessage = idMessage;
        }
    }
}