using System.Collections.Generic;
using System.IO;
using TwitchChatConnect.Client;
using TwitchChatConnect.Data;
using TwitchChatConnect.HLAPI;
using UnityEditor;
using UnityEngine;

namespace TwitchChatConnect.Editor
{
    public class RewardRegisterWindow : EditorWindow
    {
        [MenuItem("Tools/TwitchChatConnect/RewardRegister")]
        private static void Init()
        {
            RewardRegisterWindow window = GetWindow<RewardRegisterWindow>();
            window.titleContent = new GUIContent("TwitchChatConnect - Reward");
            window.Show();
        }

        private List<RewardData> _inputRewards { get; } = new List<RewardData>();
        private bool _registred { get; set; } = false;

        private void OnDisable()
        {
            _registred = false;
        }

        private void OnGUI()
        {
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("This function is only available in PlayMode", MessageType.Info);
                return;
            }

            if (TwitchChatClient.instance == null)
            {
                EditorGUILayout.HelpBox("No valid TwitchChatClient instance found in the scene", MessageType.Warning);
                return;
            }    

            if (!_registred && GUILayout.Button("Watch")) Watch();

            if (!_registred) return;

            EditorGUILayout.HelpBox("Redeem a reward in the chat", MessageType.Info);

            for (int i = 0; i < _inputRewards.Count; i++)
            {
                GUI.enabled = false;
                EditorGUILayout.TextField("Id", _inputRewards[i].Id);
                GUI.enabled = true;
                _inputRewards[i].Name = EditorGUILayout.TextField("Name", _inputRewards[i].Name);
                _inputRewards[i].Icon = (Sprite)EditorGUILayout.ObjectField("Icon", _inputRewards[i].Icon, typeof(Sprite), false);
            }

            if (_inputRewards.Count > 0 && GUILayout.Button("Save")) SaveReward();
        }
        private void Watch()
        {
            TwitchChatClient.instance.onChatRewardReceived -= OnTwitchInputLine;
            TwitchChatClient.instance.onChatRewardReceived += OnTwitchInputLine;
            _registred = true;
        }

        private void SaveReward()
        {
            string directory = $"{Application.dataPath}/TwitchRewards";
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

            for (int i = 0; i < _inputRewards.Count; i++)
            {
                string name = AssetDatabase.GenerateUniqueAssetPath($"Assets/TwitchRewards/{_inputRewards[i].Name}.asset");              
                AssetDatabase.CreateAsset(TwitchRewardData.Create(_inputRewards[i].Id, _inputRewards[i].Name, _inputRewards[i].Icon), name);
                AssetDatabase.SaveAssets();
            }

            _inputRewards.Clear();
            AssetDatabase.Refresh();
        }

        private void OnTwitchInputLine(TwitchChatReward reward)
        {
            _inputRewards.Add(new RewardData
            {
                Id = reward.CustomRewardId,
                Name = reward.Message,
            });
        }

        protected class RewardData
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public Sprite Icon { get; set; }
        }
    }
}