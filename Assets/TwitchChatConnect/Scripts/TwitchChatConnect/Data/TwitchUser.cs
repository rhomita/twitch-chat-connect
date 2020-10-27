namespace TwitchChatConnect.Data
{
    public class TwitchUser
    {
        public string Username { get; private set; }
        public string Id { get; private set; }
        public bool IsSub { get; private set; }

        private string displayName;

        public string DisplayName
        {
            get => displayName ?? Username;
            private set => displayName = value;
        }

        public TwitchUser(string username)
        {
            Username = username;
        }

        public void SetData(string id, string displayName, bool isSub)
        {
            this.displayName = displayName;
            IsSub = isSub;
            Id = id;
        }
        
        
    }
}