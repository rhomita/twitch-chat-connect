using System;
using UnityEngine;

namespace TwitchChatConnect.Config
{
    [Serializable]
    public class TwitchConnectConfig
    {
        [SerializeField] private string username;
        [SerializeField] private string userToken;
        [SerializeField] private string channelName;

        public string Username => username.ToLower();
        public string UserToken => userToken;
        public string ChannelName => channelName;

        public TwitchConnectConfig(string username, string userToken, string channelName)
        {
            this.username = username;
            this.userToken = userToken;
            this.channelName = channelName;
        }
        
        public bool IsValid()
        {
            return !String.IsNullOrEmpty(Username) &&
                   !String.IsNullOrEmpty(UserToken) &&
                   !String.IsNullOrEmpty(ChannelName);
        }
    }
}