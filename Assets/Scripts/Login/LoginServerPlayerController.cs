using BubbleBots.Server.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoginServerPlayerController : MonoBehaviour
{
    private string tempAddress;
    private string tempSignature;

    private void SetDataToUser(GetPlayerDataResult res)
    {
        UserManager.Instance.SetPlayerUserName(res.nickname, false);
        UserManager.Instance.SetPlayerRank(res.rank);
        UserManager.Instance.SetWalletAddress(tempAddress);
        UserManager.Instance.SetSignature(tempSignature);
    }

    private void OnPlayerCreatedOnServer(string data)
    {
        GetPlayerDataResult result = JsonUtility.FromJson<GetPlayerDataResult>(data);
        SetDataToUser(result);
        SceneManager.LoadScene(EnvironmentManager.Instance.GetSceneName());
    }

    private void OnFailPlayerGet(string data)
    {
        Debug.Log(data);
        CreatePlayerData formData = new()
        {
            address = tempAddress,
            signature = tempSignature
        };
        Debug.Log(tempAddress);
        Debug.Log(tempSignature);
        string jsonFormData = JsonUtility.ToJson(formData);
        ServerManager.Instance.SendPlayerDataToServer(PlayerAPI.Create, jsonFormData, OnPlayerCreatedOnServer);
    }

    private void OnSuccessPlayerGet(string data)
    {
        Debug.Log(data);
        GetPlayerDataResult result = JsonUtility.FromJson<GetPlayerDataResult>(data);
        SetDataToUser(result);
        SceneManager.LoadScene(EnvironmentManager.Instance.GetSceneName());
    }

    public void GetOrCreatePlayer(string address, string signature)
    {
        tempAddress = address;
        tempSignature = signature;
        ServerManager.Instance.GetPlayerDataFromServer(PlayerAPI.Get, OnSuccessPlayerGet, address, OnFailPlayerGet);
    }
}
