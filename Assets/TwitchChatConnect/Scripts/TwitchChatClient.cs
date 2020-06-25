using UnityEngine;
using System.Net.Sockets;
using System.IO;

public class TwitchChatClient : MonoBehaviour
{
    [Header("config.json file with 'username', 'userToken' and 'channelName'")]
    [SerializeField] private string configurationPath = "";
    [Header("Command prefix, by default is '!' (only 1 character)")]
    [SerializeField] private string commandPrefix = "!";
    [Header("Automatic initialize, otherwise it is necessary to call 'Init'")]
    [SerializeField] private bool automaticInit = true;

    private TcpClient twitchClient;
    private StreamReader reader;
    private StreamWriter writer;

    private TwitchConnectData data;

    public delegate void OnChatMessageReceived(TwitchChatMessage chatMessage);
    public OnChatMessageReceived onChatMessageReceived;

    private bool hasInitialized = false;

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

    void Start()
    {
        if (!automaticInit) return;
        Init();
    }

    void Update()
    {
        if (twitchClient == null || !twitchClient.Connected) return;
        ReadChat();
    }

    public void Init()
    {
        if (hasInitialized) return;
        hasInitialized = true;

        // Checks
        if (configurationPath == "") configurationPath = Application.persistentDataPath + "/config.json";
        if (commandPrefix == "" || commandPrefix == null) commandPrefix = "!";
        if (commandPrefix.Length > 1)
        {
            Debug.LogError($"TwitchChatClient.Init :: Command prefix length should contain only 1 character. Command prefix: {commandPrefix}");
            return;
        }

        data = TwitchConfiguration.Load(configurationPath);
        if (data == null) return;
        Login();
    }

    private void Login()
    {
        twitchClient = new TcpClient("irc.chat.twitch.tv", 6667);
        reader = new StreamReader(twitchClient.GetStream());
        writer = new StreamWriter(twitchClient.GetStream());

        writer.WriteLine("PASS " + data.userToken);
        writer.WriteLine("NICK " + data.username);
        writer.WriteLine("USER " + data.username + " 8 * :" + data.username);
        writer.WriteLine("JOIN #" + data.channelName);
        writer.Flush();
    }

    private void ReadChat()
    {
        if (twitchClient.Available <= 0) return;
        var message = reader.ReadLine();

        if (!message.Contains("PRIVMSG")) return;

        var splitPoint = message.IndexOf(commandPrefix, 1);
        var username = message.Substring(0, splitPoint);
        splitPoint = message.IndexOf(":", 1);
        message = message.Substring(splitPoint + 1);
        string[] messages = message.Split(' ');

        if (messages.Length == 0 || messages[0][0] != commandPrefix[0]) return;

        username = username.Substring(1);

        TwitchChatMessage chatMessage = new TwitchChatMessage(username, messages);
        onChatMessageReceived?.Invoke(chatMessage);
    }

    public string ReadLine()
    {
        if (twitchClient.Available == 0) return "";
        return reader.ReadLine();
    }

    public void SendTwitchChatMessage(string message)
    {
        writer.WriteLine("PRIVMSG #" + data.channelName + " :/me " + message);
        writer.Flush();
    }

    public void SendCommand(string command, string parameters)
    {
        writer.WriteLine("PRIVMSG #" + data.channelName + " :" + command + " " + parameters);
        writer.Flush();
    }
}
