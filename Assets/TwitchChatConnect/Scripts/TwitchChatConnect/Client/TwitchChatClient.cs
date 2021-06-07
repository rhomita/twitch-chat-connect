using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.IO;
using System.Text.RegularExpressions;
using TwitchChatConnect.Config;
using TwitchChatConnect.Data;
using TwitchChatConnect.Manager;

namespace TwitchChatConnect.Client
{
    public class TwitchChatClient : MonoBehaviour
    {
        [Header("Command prefix, by default is '!' (only 1 character)")] 
        [SerializeField]
        private string _commandPrefix = "!";

        [Header("Optional init Twitch configuration")] 
        [SerializeField]
        private TwitchConnectData _initTwitchConnectData;

        private OnError _onError;
        private OnSuccess _onSuccess;
        private TcpClient _twitchClient;
        private StreamReader _reader;
        private StreamWriter _writer;

        private TwitchConnectConfig _twitchConnectConfig;

        private bool _isAuthenticated;

        private static string COMMAND_NOTICE = "NOTICE";
        private static string COMMAND_PING = "PING :tmi.twitch.tv";
        private static string COMMAND_PONG = "PONG :tmi.twitch.tv";
        private static string COMMAND_JOIN = "JOIN";
        private static string COMMAND_PART = "PART";
        private static string COMMAND_MESSAGE = "PRIVMSG";
        private static string CUSTOM_REWARD_TEXT = "custom-reward-id";
        private static char BADGES_SEPARATOR = ',';
        private static char BADGE_SEPARATOR = '/';

        // For now we compare against the 'failed message' because there is not a message id for this: https://dev.twitch.tv/docs/irc/msg-id
        private static string LOGIN_FAILED_MESSAGE = "Login authentication failed";

        // For now we compare against the 'success message' + the username. https://dev.twitch.tv/docs/irc/guide
        private static string LOGIN_SUCCESS_MESSAGE = ":tmi.twitch.tv 001";

        // Custom error message when the username does not belong to the given user token, but the user token is valid.
        private static string LOGIN_WRONG_USERNAME =
            "The user token is correct but it does not belong to the given username.";

        private readonly Regex
            _joinRegexp = new Regex(@":(.+)!.*JOIN"); // :<user>!<user>@<user>.tmi.twitch.tv JOIN #<channel>

        private readonly Regex
            _partRegexp = new Regex(@":(.+)!.*PART"); // :<user>!<user>@<user>.tmi.twitch.tv PART #<channel>

        private readonly Regex _messageRegexp =
            new Regex(@"badges=(.+?);.*display\-name=(.+?);.*user\-id=(.+?);.*:(.*)!.*PRIVMSG.+:(.*)");

        private readonly Regex _rewardRegexp =
            new Regex(@"badges=(.+?);.*custom\-reward\-id=(.+?);.*display\-name=(.+?);.*user\-id=(.+?);.*:(.*)!.*PRIVMSG.+:(.*)");

        private readonly Regex _idMessageRegexp = new Regex(@"msg-id=(.+?);");

        private Regex _cheerRegexp = new Regex(@"(?:\s|^)cheer([0-9]+)(?:\s|$)", RegexOptions.IgnoreCase);

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

        /// <summary>
        /// Initializes the connection to the Twitch chat.
        /// You must have previously added the configuration to the component.
        /// Invokes onSuccess when the connection is done, otherwise onError will be invoked.
        /// </summary>
        public void Init(OnSuccess onSuccess, OnError onError)
        {
            Init(_initTwitchConnectData.TwitchConnectConfig, onSuccess, onError);
        }

        /// <summary>
        /// Initializes the connection to the Twitch chat.
        /// TwitchConnectData parameter is the init configuration to be able to connect.
        /// Invokes onSuccess when the connection is done, otherwise onError will be invoked.
        /// </summary>
        public void Init(TwitchConnectConfig twitchConnectConfig, OnSuccess onSuccess, OnError onError)
        {
            _twitchConnectConfig = twitchConnectConfig;

            if (IsConnected() && _isAuthenticated)
            {
                onSuccess();
                return;
            }

            // Checks
            if (_twitchConnectConfig == null || !_twitchConnectConfig.IsValid())
            {
                string errorMessage =
                    "TwitchChatClient.Init :: Twitch connect data is invalid, all fields are mandatory.";
                onError(errorMessage);
                return;
            }

            if (String.IsNullOrEmpty(_commandPrefix)) _commandPrefix = "!";

            if (_commandPrefix.Length > 1)
            {
                string errorMessage =
                    $"TwitchChatClient.Init :: Command prefix length should contain only 1 character. Command prefix: {_commandPrefix}";
                onError(errorMessage);
                return;
            }

            _onError = onError;
            _onSuccess = onSuccess;
            Login();
        }

        private void Login()
        {
            _twitchClient = new TcpClient("irc.chat.twitch.tv", 6667);
            _reader = new StreamReader(_twitchClient.GetStream());
            _writer = new StreamWriter(_twitchClient.GetStream());

            _writer.WriteLine($"PASS {_twitchConnectConfig.UserToken}");
            _writer.WriteLine($"NICK {_twitchConnectConfig.Username}");
            _writer.WriteLine($"JOIN #{_twitchConnectConfig.ChannelName}");

            _writer.WriteLine("CAP REQ :twitch.tv/tags");
            _writer.WriteLine("CAP REQ :twitch.tv/commands");
            _writer.WriteLine("CAP REQ :twitch.tv/membership");

            _writer.Flush();
        }

        private void ReadChatLine()
        {
            if (_twitchClient.Available <= 0) return;
            string message = _reader.ReadLine();
            if (message == null) return;
            if (message.Length == 0) return;

            if (message.StartsWith($"{LOGIN_SUCCESS_MESSAGE}"))
            {
                if (message.StartsWith($"{LOGIN_SUCCESS_MESSAGE} {_twitchConnectConfig.Username}"))
                {
                    _isAuthenticated = true;
                    _onSuccess?.Invoke();
                    _onSuccess = null;
                    return;
                }

                // In this case the user token is valid, but the username does not belong to the given token.
                _onError?.Invoke(LOGIN_WRONG_USERNAME);
                _onError = null;
                return;
            }

            if (message.Contains(COMMAND_NOTICE) && message.Contains(LOGIN_FAILED_MESSAGE))
            {
                _onError?.Invoke(LOGIN_FAILED_MESSAGE);
                _onError = null;
                return;
            }

            if (message.Contains(COMMAND_PING))
            {
                _writer.WriteLine(COMMAND_PONG);
                _writer.Flush();
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
                string username = _joinRegexp.Match(message).Groups[1].Value;
                TwitchUserManager.AddUser(username);
            }
            else if (message.Contains(COMMAND_PART))
            {
                string username = _partRegexp.Match(message).Groups[1].Value;
                TwitchUserManager.RemoveUser(username);
            }
        }

        private void ReadChatMessage(string message)
        {
            string badgesText = _messageRegexp.Match(message).Groups[1].Value;
            string displayName = _messageRegexp.Match(message).Groups[2].Value;
            string idUser = _messageRegexp.Match(message).Groups[3].Value;
            string username = _messageRegexp.Match(message).Groups[4].Value;
            string messageSent = _messageRegexp.Match(message).Groups[5].Value;

            if (messageSent.Length == 0) return;

            List<TwitchUserBadge> badges = BuildBadges(badgesText);

            TwitchUser twitchUser = TwitchUserManager.AddUser(username);
            twitchUser.SetData(idUser, displayName, badges);

            int bits = 0;
            MatchCollection matches = _cheerRegexp.Matches(messageSent);
            foreach (Match match in matches)
            {
                if (match.Groups.Count != 2) continue; // First group is 'cheerXX', second group is XX.
                string value = match.Groups[1].Value;
                if (!Int32.TryParse(value, out int bitsAmount)) continue;
                bits += bitsAmount;
            }

            string idMessage = _idMessageRegexp.Match(message).Groups[1].Value;
            if (messageSent[0] == _commandPrefix[0])
            {
                TwitchChatCommand chatCommand = new TwitchChatCommand(twitchUser, messageSent, bits, idMessage);
                onChatCommandReceived?.Invoke(chatCommand);
            }
            else
            {
                TwitchChatMessage chatMessage = new TwitchChatMessage(twitchUser, messageSent, bits, idMessage);
                onChatMessageReceived?.Invoke(chatMessage);
            }
        }

        private void ReadChatReward(string message)
        {
            string badgesText = _rewardRegexp.Match(message).Groups[1].Value;
            string customRewardId = _rewardRegexp.Match(message).Groups[2].Value;
            string displayName = _rewardRegexp.Match(message).Groups[3].Value;
            string idUser = _rewardRegexp.Match(message).Groups[4].Value;
            string username = _rewardRegexp.Match(message).Groups[5].Value;
            string messageSent = _rewardRegexp.Match(message).Groups[6].Value;

            List<TwitchUserBadge> badges = BuildBadges(badgesText);

            TwitchUser twitchUser = TwitchUserManager.AddUser(username);
            twitchUser.SetData(idUser, displayName, badges);

            TwitchChatReward chatReward = new TwitchChatReward(twitchUser, messageSent, customRewardId);
            onChatRewardReceived?.Invoke(chatReward);
        }

        private static List<TwitchUserBadge> BuildBadges(string badgesText)
        {
            List<TwitchUserBadge> badges = new List<TwitchUserBadge>();
            foreach (string badge in badgesText.Split(BADGES_SEPARATOR))
            {
                string[] badgeSplit = badge.Split(BADGE_SEPARATOR);
                string name = badgeSplit[0];
                if (badgeSplit.Length != 2) continue; // It should contains two parts <badge name>/<version>
                string versionText = badgeSplit[1];
                if (!Int32.TryParse(versionText, out int version)) version = 0;
                badges.Add(new TwitchUserBadge(name, version));
            }

            return badges;
        }

        /// <summary>
        /// Sends a chat message.
        /// Returns false when it is not connected, otherwise true.
        /// </summary>
        public bool SendChatMessage(string message)
        {
            if (!IsConnected()) return false;
            SendTwitchMessage(message);
            return true;
        }

        /// <summary>
        /// Sends a chat message after X seconds.
        /// Returns false when it is not connected, otherwise true.
        /// </summary>
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
            _writer.WriteLine($"PRIVMSG #{_twitchConnectConfig.ChannelName} :/me {message}");
            _writer.Flush();
        }

        private bool IsConnected()
        {
            return _twitchClient != null && _twitchClient.Connected;
        }
    }
}