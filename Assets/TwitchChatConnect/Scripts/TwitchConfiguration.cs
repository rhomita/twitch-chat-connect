using Newtonsoft.Json;
using System.IO;
using UnityEngine;

public static class TwitchConfiguration 
{
    public static TwitchConnectData Load(string path)
    {
        if (File.Exists(path))
        {
            string fileText = File.ReadAllText(path);
            TwitchConnectData twitchConnectData = JsonConvert.DeserializeObject<TwitchConnectData>(fileText);
            if (!twitchConnectData.IsValid())
            {
                Debug.Log(twitchConnectData.username);
                Debug.LogError($"TwitchConfiguration.Load :: Some mandatory fields are empty: {path}");
                return null;
            }

            
            return twitchConnectData;
        }
        Debug.LogError($"TwitchConfiguration.Load :: Error loading the data, path does not exist: {path}");
        return null;
    }
}
