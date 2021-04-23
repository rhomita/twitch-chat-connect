using TwitchChatConnect.Client;
using TwitchChatConnect.Config;
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
                Debug.Log("Connected!");
                TwitchChatClient.instance.onChatMessageReceived += ShowMessage;
                TwitchChatClient.instance.onChatCommandReceived += ShowCommand;
                TwitchChatClient.instance.onChatRewardReceived += ShowReward;
                
            },
            message =>
            {
                // Error when initializing.
                Debug.LogError(message);
            });
    }

    void ShowCommand(TwitchChatCommand chatCommand)
    {
        TwitchConnectData a = ScriptableObject.CreateInstance<TwitchConnectData>();
        string parameters = string.Join(" - ", chatCommand.Parameters);
        string message =
            $"Command: '{chatCommand.Command}' - Username: {chatCommand.User.DisplayName} - Bits: {chatCommand.Bits} - Sub: {chatCommand.User.IsSub} - Parameters: {parameters}";

        TwitchChatClient.instance.SendChatMessage($"Hello {chatCommand.User.DisplayName}! I received your message.");
        TwitchChatClient.instance.SendChatMessage(
            $"Hello {chatCommand.User.DisplayName}! This message will be sent in 5 seconds.", 5);

        AddText(message);
    }

    void ShowReward(TwitchChatReward chatReward)
    {
        string message = $"Reward unlocked by {chatReward.User.DisplayName} - Reward ID: {chatReward.CustomRewardId} - Message: {chatReward.Message}";
        AddText(message);
    }
    
    void ShowMessage(TwitchChatMessage chatMessage)
    {
        string message = $"Message by {chatMessage.User.DisplayName} - Bits: {chatMessage.Bits} - Sub: {chatMessage.User.IsSub} - Message: {chatMessage.Message}";
        AddText(message);
    }

    private void AddText( string message)
    {
        GameObject newText = Instantiate(textPrefab, panel);
        newText.GetComponent<Text>().text = message;   
    }
}