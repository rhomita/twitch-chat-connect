using Newtonsoft.Json;
using System.IO;
using UnityEngine;

namespace TwitchChatConnect.Data
{
    public static class TwitchConfiguration
    {
        public delegate void OnError(string errorMessage);

        public delegate void OnSuccess(TwitchConnectData twitchConnectData);

        public static void Load(string path, OnSuccess onSuccess, OnError onError)
        {
            if (!File.Exists(path))
            {
                string errorMessage =
                    $"TwitchConfiguration.Load :: Error loading the data, path does not exist: {path}";
                onError(errorMessage);
                return;
            }

            string fileText = File.ReadAllText(path);
            TwitchConnectData twitchConnectData = JsonConvert.DeserializeObject<TwitchConnectData>(fileText);
            if (!twitchConnectData.IsValid())
            {
                string errorMessage = $"TwitchConfiguration.Load :: Some mandatory fields are empty: {path}";
                onError(errorMessage);
                return;
            }

            onSuccess(twitchConnectData);
        }
    }
}