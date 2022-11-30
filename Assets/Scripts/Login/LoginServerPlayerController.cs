using BubbleBots.Server.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoginServerPlayerController : MonoBehaviour
{
    private string tempAddress;

    private void SetDataToUser(GetPlayerDataResult res)
    {
        UserManager.Instance.SetPlayerUserName(res.nickname, false);
        UserManager.Instance.SetPlayerRank(res.rank);
        UserManager.Instance.SetWalletAddress(tempAddress);
    }

    private void OnPlayerCreatedOnServer(string data)
    {
        GetPlayerDataResult result = JsonUtility.FromJson<GetPlayerDataResult>(data);
        SetDataToUser(result);
        SceneManager.LoadScene(EnvironmentManager.Instance.GetSceneName());
    }

    private void OnFailPlayerGet(string data)
    {
        CreatePlayerData formData = new()
        {
            address = tempAddress,
        };
        string jsonFormData = JsonUtility.ToJson(formData);
        ServerManager.Instance.SendPlayerDataToServer(PlayerAPI.Create, jsonFormData, OnPlayerCreatedOnServer);
    }

    private void OnSuccessPlayerGet(string data)
    {
        GetPlayerDataResult result = JsonUtility.FromJson<GetPlayerDataResult>(data);
        SetDataToUser(result);
        SceneManager.LoadScene(EnvironmentManager.Instance.GetSceneName());
    }

    public void GetOrCreatePlayer(string address)
    {
        tempAddress = address;
        ServerManager.Instance.GetPlayerDataFromServer(PlayerAPI.Get, OnSuccessPlayerGet, address, OnFailPlayerGet);
    }
}
