[System.Serializable]
public class TwitchConnectData
{
    public string username { get; set; }
    public string userToken { get; set; }
    public string channelName { get; set; }

    public bool IsValid()
    {
        return (username != "" && username != null)
            && (userToken != "" && userToken != null)
            && (channelName != "" && channelName != null);
    }
}
