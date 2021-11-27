using System.Collections.Generic;
using UnityEngine;

namespace TwitchChatConnect.HLAPI
{
    public class TwitchRewardCollection : MonoBehaviour
    {
        public static TwitchRewardCollection instance { get; private set; }

        [SerializeField] private List<TwitchRewardData> _rewards;

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

        public bool TryFind(string id, out TwitchRewardData data)
        {
            data = Find(id);
            return data != null;
        }

        public TwitchRewardData Find(string id)
        {
            foreach (TwitchRewardData reward in _rewards)
                if (reward.TwitchId == id)
                    return reward;
            return default;
        }
    }
}
