using System;

namespace TwitchChatConnect.Data
{
    [System.Serializable]
    public class TwitchConnectData
    {
        public string Username { get; set; }
        public string UserToken { get; set; }
        public string ChannelName { get; set; }

        public bool IsValid()
        {
            return !String.IsNullOrEmpty(Username) &&
                   !String.IsNullOrEmpty(UserToken) &&
                   !String.IsNullOrEmpty(ChannelName);
        }
    }
}