namespace TwitchChatConnect.Data
{
    public class TwitchUserBadge
    {
        public TwitchUserBadge(string name, int version)
        {
            Name = name;
            Version = version;
        }
        
        public string Name { get; }
        public int Version { get; }
    }
}