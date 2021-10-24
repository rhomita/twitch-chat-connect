namespace TwitchChatConnect.Client
{
    public enum TwitchCommandType
    {
        UNKNOWN,
        LOGIN,
        NOTICE,
        PING,
        PONG,
        JOIN,
        PART,
        MESSAGE_REWARD,
        MESSAGE_CHAT,
        MESSAGE_COMMAND,
    }
}
