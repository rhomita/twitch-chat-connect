using System.Collections;
using System.Collections.Generic;
using TwitchChatConnect.Data;
using UnityEngine;
using UnityEngine.UI;

namespace TwitchChatConnect.Example.MiniGame
{
    public class GameUI : MonoBehaviour
    {
        [SerializeField] private Text namePrefab;
        [SerializeField] private Transform grid;

        public Dictionary<TwitchUser, UserInfo> Users;

        #region Singleton
        public static GameUI instance { get; private set; }
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
        
        void Start()
        {
            Users = new Dictionary<TwitchUser, UserInfo>();
        }
        
        public void UpdateUser(TwitchUser user)
        {
            if (!Users.ContainsKey(user))
            {
                Text text = Instantiate(namePrefab, grid);
                UserInfo userInfo = new UserInfo(text, user.DisplayName);
                Users.Add(user, userInfo);
            }

            Users[user].Update();
        }

        public void Reset()
        {
            foreach (Transform child in grid)
            {
                Destroy(child.gameObject);
            }
            Users = new Dictionary<TwitchUser, UserInfo>();
        }
    }

    public class UserInfo
    {
        private string name;
        private int count;
        private Text text;

        public UserInfo(Text text, string name)
        {
            this.count = 0;
            this.text = text;
            this.name = name;
        }
        
        public void Update()
        {
            count += 1;
            text.text = GetText();
        }

        public string GetText()
        {
            return $"{name}: {count}";
        }
    }
}