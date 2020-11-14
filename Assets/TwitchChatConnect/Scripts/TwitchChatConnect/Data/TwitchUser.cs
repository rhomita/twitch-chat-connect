namespace TwitchChatConnect.Data
{
    public class TwitchUser
    {
        public string Username { get; private set; }
        public string Id { get; private set; }
        public bool IsSub { get; private set; }

        private string _displayName;

        public string DisplayName => _displayName ?? Username;

        public TwitchUser(string username)
        {
            Username = username;
        }

        public void SetData(string id, string displayName, bool isSub)
        {
            _displayName = displayName;
            IsSub = isSub;
            Id = id;
        }
        
        
    }
}