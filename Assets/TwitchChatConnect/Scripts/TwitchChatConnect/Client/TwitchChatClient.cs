using System;
using System.Collections;
using System.IO;
using System.Net.Sockets;
using TwitchChatConnect.Config;
using TwitchChatConnect.Data;
using TwitchChatConnect.Manager;
using UnityEngine;
using static PlasticPipe.PlasticProtocol.Messages.NegotiationCommand;

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

        private const string COMMAND_PONG = "PONG :tmi.twitch.tv";

        // For now we compare against the 'failed message' because there is not a message id for this: https://dev.twitch.tv/docs/irc/msg-id
        private const string LOGIN_FAILED_MESSAGE = "Login authentication failed";

        // Custom error message when the username does not belong to the given user token, but the user token is valid.
        private const string LOGIN_WRONG_USERNAME = "The user token is correct but it does not belong to the given username.";

        public string CommandPrefix => _commandPrefix;

        public delegate void OnChatMessageReceived(TwitchChatMessage chatMessage);

        public OnChatMessageReceived onChatMessageReceived { get; set; }

        public delegate void OnChatCommandReceived(TwitchChatCommand chatCommand);

        public OnChatCommandReceived onChatCommandReceived { get; set; }

        public delegate void OnChatRewardReceived(TwitchChatReward chatReward);

        public OnChatRewardReceived onChatRewardReceived { get; set; }

        public delegate void OnError(string errorMessage);

        public delegate void OnSuccess();

        #region Singleton

        public static TwitchChatClient instance { get; private set; }

        private void Awake()
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

        private void FixedUpdate()
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

            if (string.IsNullOrEmpty(_commandPrefix)) _commandPrefix = "!";

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

        private void OnApplicationQuit() => CloseTcpClient();

        private void OnDestroy() => CloseTcpClient();

        private void CloseTcpClient()
        {
            if (_twitchClient == null) return;
            try
            {
                _twitchClient.Close();
                _twitchClient.Dispose();
                _twitchClient = null;
                Debug.LogWarning($"Twitch Disconnect");
            }
            catch { }
        }

        private void ReadChatLine()
        {
            if (_twitchClient.Available <= 0) return;
            string source = _reader.ReadLine();
            TwitchCommand command = new TwitchCommand(source, _commandPrefix);

            if (command.Type != TwitchCommandType.UNKNOWN)
            {
                
                Debug.LogWarning($"{command.UserName}@{command.Type}:{command.Message}");
            }

            switch (command.Type)
            {
                case TwitchCommandType.LOGIN:
                    if (command.IsValidLogin(_twitchConnectConfig))
                    {
                        _isAuthenticated = true;
                        _onSuccess?.Invoke();
                        _onSuccess = null;
                        Debug.Log("<color=green>¡Success Twitch Connection!</color>");
                    }
                    else
                    {
                        _onError?.Invoke(LOGIN_WRONG_USERNAME);
                        _onError = null;                        
                        Debug.Log("<color=red>¡Error Twitch Connection!</color>");
                    }
                    break;

                case TwitchCommandType.NOTICE:
                    _onError?.Invoke(LOGIN_FAILED_MESSAGE);
                    _onError = null;
                    break;

                case TwitchCommandType.PING:
                    _writer.WriteLine(COMMAND_PONG);
                    _writer.Flush();
                    break;

                case TwitchCommandType.MESSAGE_COMMAND:
                    {
                        TwitchChatMessagePayload payload = new TwitchChatMessagePayload(command);
                        TwitchChatCommand chatCommand = new TwitchChatCommand(payload.User, payload.Sent, payload.Bits, payload.Id);
                        onChatCommandReceived?.Invoke(chatCommand);
                    }
                    break;

                case TwitchCommandType.MESSAGE_CHAT:
                    {
                        TwitchChatMessagePayload payload = new TwitchChatMessagePayload(command);
                        TwitchChatMessage chatMessage = new TwitchChatMessage(payload.User, payload.Sent, payload.Bits, payload.Id);
                        onChatMessageReceived?.Invoke(chatMessage);
                    }
                    break;

                case TwitchCommandType.MESSAGE_REWARD:
                    {
                        TwitchChatRewardPayload payload = new TwitchChatRewardPayload(command);
                        TwitchChatReward chatReward = new TwitchChatReward(payload.User, payload.Sent, payload.Id);
                        onChatRewardReceived?.Invoke(chatReward);
                    }
                    break;

                case TwitchCommandType.JOIN: TwitchUserManager.AddUser(command.UserName); break;

                case TwitchCommandType.PART: TwitchUserManager.RemoveUser(command.UserName); break;
            }
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