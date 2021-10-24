﻿using System.Collections.Generic;
using TwitchChatConnect.Client;
using TwitchChatConnect.Data;
using TwitchChatConnect.HLAPI;
using UnityEngine;

namespace TwitchChatConnect.Example.MiniGame
{
    public class GameExpert : MonoBehaviour
    {
        private TwitchCommandHandler _twitchCommanHandler { get; } = new TwitchCommandHandler();

        private void Start()
        {
            TwitchChatClient.instance.Init(() =>
            {
                TwitchChatClient.instance.onChatMessageReceived += OnChatMessageReceived;
                TwitchChatClient.instance.onChatCommandReceived += _twitchCommanHandler.ProcessCommand;
                TwitchChatClient.instance.onChatRewardReceived += OnChatRewardReceived;

                MatchManager.instance.onMatchEnd += OnMatchEnd;
                MatchManager.instance.onMatchBegin += OnMatchBegin;
            }, Debug.LogError);

            _twitchCommanHandler.Register<TwitchCommandEmpty>("start", OnStart);
            _twitchCommanHandler.Register<TwitchCommandMove>("move", OnMove);
        }

        private void OnMove(TwitchCommandMove template)
        {
            if (!MatchManager.instance.HasStarted) return;
            GameUI.instance.UpdateUser(template.Command.User);
            MatchManager.instance.Move(template.Value);
        }

        private void OnStart(TwitchCommandEmpty template)
        {
            if (MatchManager.instance.HasStarted) return;
            MatchManager.instance.Begin();
        }

        private void OnChatRewardReceived(TwitchChatReward chatReward)
        {
        }

        private void OnChatMessageReceived(TwitchChatMessage chatMessage)
        {
        }

        private void OnMatchBegin()
        {
            TwitchChatClient.instance.SendChatMessage("A new game has started");
        }

        void OnMatchEnd(float secondsElapsed)
        {
            TwitchChatClient.instance.SendChatMessage("---------------");
            TwitchChatClient.instance.SendChatMessage($"The game has ended, it took {secondsElapsed} seconds.");
            foreach (KeyValuePair<TwitchUser, UserInfo> user in GameUI.instance.Users)
            {
                TwitchChatClient.instance.SendChatMessage(user.Value.GetText());
            }
            TwitchChatClient.instance.SendChatMessage("---------------");
            GameUI.instance.Reset();
        }
    }
    public class TwitchCommandMove : TwitchCommandPayload
    {
        private static Dictionary<string, Vector3> _directions = new Dictionary<string, Vector3>
            {
                { "up", Vector3.forward },
                { "down", Vector3.back },
                { "right", Vector3.right },
                { "left", Vector3.left }
            };

        [TwitchCommandProperty(0, true)]
        private string _direction
        {
            get => string.Empty;
            set
            {
                _directions.TryGetValue(value, out Vector3 dir);
                Value = dir;
            }
        }

        public Vector3 Value { get; private set; }
    }
}