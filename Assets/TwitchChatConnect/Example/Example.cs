using UnityEngine;
using UnityEngine.UI;

public class Example : MonoBehaviour
{
    [SerializeField] private Transform panel;
    [SerializeField] private GameObject textPrefab;

    void Start()
    {
        TwitchChatClient.instance.onChatMessageReceived += ShowMessage;
    }

    void ShowMessage(TwitchChatMessage chatMessage)
    {
        string parameters = string.Join(" - ", chatMessage.parameters);
        string chatMessageText = $"Command: '{chatMessage.command}' - Sender: {chatMessage.sender} - Parameters: {parameters}";

        GameObject newText = Instantiate(textPrefab, panel);
        newText.GetComponent<Text>().text = chatMessageText;
    }
}
