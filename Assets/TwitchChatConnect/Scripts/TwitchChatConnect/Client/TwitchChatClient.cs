using System;
using System.Collections;
using UnityEngine;
using System.Net.Sockets;
using System.IO;
using TwitchChatConnect.Data;

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

        public delegate void OnChatMessageReceived(TwitchChatMessage chatMessage);
        public OnChatMessageReceived onChatMessageReceived;

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

            writer.WriteLine("PASS " + twitchConnectData.UserToken);
            writer.WriteLine("NICK " + twitchConnectData.Username);
            writer.WriteLine("USER " + twitchConnectData.Username + " 8 * :" + twitchConnectData.Username);
            writer.WriteLine("JOIN #" + twitchConnectData.ChannelName);
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
                writer.WriteLine("PONG #" + twitchConnectData.ChannelName);
                writer.Flush();
                return;
            }

            if (message.Contains("PRIVMSG"))
            {
                writer.WriteLine("PONG #" + twitchConnectData.ChannelName);
                writer.Flush();
                ReadChatMessage(message);
                return;
            }
            
            // Implement JOIN and PART. (They are not working very well https://dev.twitch.tv/docs/irc/guide#generic-irc-commands)
        }

        private void ReadChatMessage(string message)
        {
            int splitPoint = message.IndexOf(commandPrefix, 1);
            if (splitPoint == -1) return;

            string username = message.Substring(0, splitPoint);
            splitPoint = message.IndexOf(":", 1);
            message = message.Substring(splitPoint + 1);
            string[] messages = message.Split(' ');

            if (messages.Length == 0 || messages[0][0] != commandPrefix[0]) return;

            username = username.Substring(1);

            TwitchChatMessage chatMessage = new TwitchChatMessage(username, messages);
            onChatMessageReceived?.Invoke(chatMessage);
        }

        /*
         * Sends a chat message.
         */
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
            writer.WriteLine("PRIVMSG #" + twitchConnectData.ChannelName + " :/me " + message);
            writer.Flush();
        }

        private bool IsConnected()
        {
            return twitchClient != null && twitchClient.Connected;
        }
    }
}