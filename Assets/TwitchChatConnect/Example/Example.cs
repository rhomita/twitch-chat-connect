using TwitchChatConnect.Client;
using TwitchChatConnect.Data;
using UnityEngine;
using UnityEngine.UI;

public class Example : MonoBehaviour
{
    [SerializeField] private Transform panel;
    [SerializeField] private GameObject textPrefab;

    void Start()
    {
        TwitchChatClient.instance.Init(() =>
        {
            TwitchChatClient.instance.onChatMessageReceived += ShowMessage;
        }, message =>
        {
            // Error when initializing.
            Debug.LogError(message);
        });
    }

    void ShowMessage(TwitchChatMessage chatMessage)
    {
        string parameters = string.Join(" - ", chatMessage.Parameters);
        string chatMessageText = $"Command: '{chatMessage.Command}' - Sender: {chatMessage.Sender} - Parameters: {parameters}";

        TwitchChatClient.instance.SendChatMessage($"Hello {chatMessage.Sender}! I received your message.");
        TwitchChatClient.instance.SendChatMessage($"Hello {chatMessage.Sender}! This message will be sent in 5 seconds.", 5);
        
        GameObject newText = Instantiate(textPrefab, panel);
        newText.GetComponent<Text>().text = chatMessageText;
    }
}
