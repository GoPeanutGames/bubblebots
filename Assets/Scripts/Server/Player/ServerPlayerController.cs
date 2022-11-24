using BubbleBots.Server.Player;
using UnityEngine;

public class ServerPlayerController : MonoBehaviour
{
    private void OnPlayerCreatedOnServer(string data)
    {
        Debug.Log(data);
    }

    public void CreatePlayer(string address)
    {
        CreatePlayerData formData = new()
        {
            address = address,
        };
        string jsonFormData = JsonUtility.ToJson(formData);
        ServerManager.Instance.CreatePlayerOnServer(jsonFormData, OnPlayerCreatedOnServer);
    }
}
