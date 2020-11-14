using System;
using TwitchChatConnect.Client;
using UnityEngine;

namespace TwitchChatConnect.Config
{
    [CreateAssetMenu(fileName = "TwitchConnectData", menuName = "TwitchChatConnect/TwitchConnectData", order = 1)]
    public class TwitchConnectData : ScriptableObject
    {
        [SerializeField] private TwitchConnectConfig twitchConnectConfig;
        public TwitchConnectConfig TwitchConnectConfig => twitchConnectConfig;
    }
}