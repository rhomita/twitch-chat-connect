using System;
using System.Collections;
using UnityEngine;
using System.Net.Sockets;
using System.IO;
using System.Text.RegularExpressions;
using TwitchChatConnect.Data;
using TwitchChatConnect.Manager;

namespace TwitchChatConnect.Client
{
    public class TwitchChatClient : MonoBehaviour
    {
        [Header("config.json file with 'username', 'userToken' and 'channelName'")] 
        [SerializeField] private string configurationPath = "";

        [Header("Command prefix, by default is '!' (only 1 character)")] 
        [SerializeField] private string commandPrefix = "!";

        private TcpClient twitchClient;
        private StreamReader reader;
        private StreamWriter writer;
        private TwitchConnectData twitchConnectData;

        private static string COMMAND_JOIN = "JOIN";
        private static string COMMAND_PART = "PART";
        private static string COMMAND_MESSAGE = "PRIVMSG";
        private static string CUSTOM_REWARD_TEXT = "custom-reward-id";

        private Regex joinRegexp = new Regex(@":(.+)!.*JOIN"); // :<user>!<user>@<user>.tmi.twitch.tv JOIN #<channel>
        private Regex partRegexp = new Regex(@":(.+)!.*PART"); // :<user>!<user>@<user>.tmi.twitch.tv PART #<channel>

        private Regex messageRegexp =
            new Regex(@"display\-name=(.+);emotes.*subscriber=(.+);tmi.*user\-id=(.+);.*:(.*)!.*PRIVMSG.+:(.*)");

        private Regex rewardRegexp =
            new Regex(
                @"custom\-reward\-id=(.+);display\-name=(.+);emotes.*subscriber=(.+);tmi.*user\-id=(.+);.*:(.*)!.*PRIVMSG.+:(.*)");

        public delegate void OnChatMessageReceived(TwitchChatMessage chatMessage);
        public OnChatMessageReceived onChatMessageReceived;
        
        public delegate void OnChatCommandReceived(TwitchChatCommand chatCommand);
        public OnChatCommandReceived onChatCommandReceived;
        
        public delegate void OnChatRewardReceived(TwitchChatReward chatReward);
        public OnChatRewardReceived onChatRewardReceived;

        public delegate void OnError(string errorMessage);

        public delegate void OnSuccess();

        #region Singleton

        public static TwitchChatClient instance { get; private set; }

        void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        #endregion

        void FixedUpdate()
        {
            if (!IsConnected()) return;
            ReadChatLine();
        }

        public void Init(OnSuccess onSuccess, OnError onError)
        {
            if (IsConnected())
            {
                onSuccess();
                return;
            }

            // Checks
            if (configurationPath == "") configurationPath = Application.persistentDataPath + "/config.json";
            if (String.IsNullOrEmpty(commandPrefix)) commandPrefix = "!";

            if (commandPrefix.Length > 1)
            {
                string errorMessage =
                    $"TwitchChatClient.Init :: Command prefix length should contain only 1 character. Command prefix: {commandPrefix}";
                onError(errorMessage);
                return;
            }

            TwitchConfiguration.Load(configurationPath, (data) =>
            {
                twitchConnectData = data;
                Login();
                onSuccess();
            }, message => onError(message));
        }

        private void Login()
        {
            twitchClient = new TcpClient("irc.chat.twitch.tv", 6667);
            reader = new StreamReader(twitchClient.GetStream());
            writer = new StreamWriter(twitchClient.GetStream());

            writer.WriteLine($"PASS {twitchConnectData.UserToken}");
            writer.WriteLine($"NICK {twitchConnectData.Username}");
            writer.WriteLine($"USER {twitchConnectData.Username} 8 * :{twitchConnectData.Username}");
            writer.WriteLine($"JOIN #{twitchConnectData.ChannelName}");

            writer.WriteLine("CAP REQ :twitch.tv/tags");
            writer.WriteLine("CAP REQ :twitch.tv/commands");
            writer.WriteLine("CAP REQ :twitch.tv/membership");

            writer.Flush();
        }

        private void ReadChatLine()
        {
            if (twitchClient.Available <= 0) return;
            string message = reader.ReadLine();

            if (message == null) return;
            if (message.Length == 0) return;

            if (message.Contains("PING"))
            {
                writer.WriteLine($"PONG #{twitchConnectData.ChannelName}");
                writer.Flush();
                return;
            }

            if (message.Contains(COMMAND_MESSAGE))
            {
                if (message.Contains(CUSTOM_REWARD_TEXT))
                {
                    ReadChatReward(message);
                }
                else
                {
                    ReadChatMessage(message);
                }
            }
            else if (message.Contains(COMMAND_JOIN))
            {
                string username = joinRegexp.Match(message).Groups[1].Value;
                TwitchUserManager.AddUser(username);
            }
            else if (message.Contains(COMMAND_PART))
            {
                string username = partRegexp.Match(message).Groups[1].Value;
                TwitchUserManager.RemoveUser(username);
            }
        }

        private void ReadChatMessage(string message)
        {
            string displayName = messageRegexp.Match(message).Groups[1].Value;
            bool isSub = messageRegexp.Match(message).Groups[2].Value == "1";
            string idUser = messageRegexp.Match(message).Groups[3].Value;
            string username = messageRegexp.Match(message).Groups[4].Value;
            string messageSent = messageRegexp.Match(message).Groups[5].Value;

            TwitchUser twitchUser = TwitchUserManager.AddUser(username);
            twitchUser.SetData(idUser, displayName, isSub);

            if (messageSent.Length == 0) return;

            if (messageSent[0] == commandPrefix[0])
            {
                TwitchChatCommand chatCommand = new TwitchChatCommand(twitchUser, messageSent);
                onChatCommandReceived?.Invoke(chatCommand);
            }
            else
            {
                TwitchChatMessage chatMessage = new TwitchChatMessage(twitchUser, messageSent);
                onChatMessageReceived?.Invoke(chatMessage);
            }
        }

        private void ReadChatReward(string message)
        {
            string customRewardId = rewardRegexp.Match(message).Groups[1].Value;
            string displayName = rewardRegexp.Match(message).Groups[2].Value;
            bool isSub = rewardRegexp.Match(message).Groups[3].Value == "1";
            string idUser = rewardRegexp.Match(message).Groups[4].Value;
            string username = rewardRegexp.Match(message).Groups[5].Value;
            string messageSent = rewardRegexp.Match(message).Groups[6].Value;
            
            TwitchUser twitchUser = TwitchUserManager.AddUser(username);
            twitchUser.SetData(idUser, displayName, isSub);
            
            TwitchChatReward chatReward = new TwitchChatReward(twitchUser, messageSent, customRewardId);
            onChatRewardReceived?.Invoke(chatReward);
        }

        public bool SendChatMessage(string message)
        {
            if (!IsConnected()) return false;
            SendTwitchMessage(message);
            return true;
        }

        public bool SendChatMessage(string message, float seconds)
        {
            if (!IsConnected()) return false;
            StartCoroutine(SendTwitchChatMessageWithDelay(message, seconds));
            return true;
        }

        private IEnumerator SendTwitchChatMessageWithDelay(string message, float seconds)
        {
            yield return new WaitForSeconds(seconds);
            SendTwitchMessage(message);
        }

        private void SendTwitchMessage(string message)
        {
            writer.WriteLine($"PRIVMSG #{twitchConnectData.ChannelName} :/me {message}");
            writer.Flush();
        }

        private bool IsConnected()
        {
            return twitchClient != null && twitchClient.Connected;
        }
    }
}