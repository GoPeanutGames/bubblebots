using BubbleBots.Server.Player;
using UnityEngine;

public class ServerPlayerController : MonoBehaviour
{

    private void OnPlayerCreatedOnServer(string data)
    {
        //Debug.Log(data);
        //GameplaySessionResult result = JsonUtility.FromJson<GameplaySessionResult>(data);
        //currentGameplaySessionID = result.sessionId;
        //Debug.Log(currentGameplaySessionID);
    }

    public void CreatePlayer(string address)
    {
        CreatePlayerData formData = new()
        {

        };
        string jsonFormData = JsonUtility.ToJson(formData);
        ServerManager.Instance.CreatePlayerOnServer(jsonFormData, OnPlayerCreatedOnServer);
    }
}
