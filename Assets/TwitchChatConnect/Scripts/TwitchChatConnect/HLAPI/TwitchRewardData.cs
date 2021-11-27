using UnityEngine;

namespace TwitchChatConnect.HLAPI
{
    [CreateAssetMenu(fileName = "TwitchRewardData", menuName = "TwitchChatConnect/TwitchRewardData", order = 1)]
    public class TwitchRewardData : ScriptableObject
    {
        [field: SerializeField] public Sprite Icon { get; private set; }
        [field: SerializeField] public string TwitchId { get; private set; }
        [field: SerializeField] public string Name { get; private set; }


        public static TwitchRewardData Create(string twitchId, string name, Sprite icon)
        {
            TwitchRewardData data = CreateInstance<TwitchRewardData>();
            data.TwitchId = twitchId;
            data.Name = name;
            data.Icon = icon;
            return data;
        }
    }
}
