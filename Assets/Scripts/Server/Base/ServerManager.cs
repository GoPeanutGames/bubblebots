using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using BubbleBots.Server;
using BubbleBots.Server.Gameplay;
using BubbleBots.Server.Player;
using BubbleBots.Server.Signature;

public class ServerManager : MonoSingleton<ServerManager>
{
    private string sessionToken =
        "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJhdXRoIjoiYWYtdXNlciIsImFnZW50IjoiIiwidG9rZW4iOiJmcmV5LXBhcmstc3RhdmUtaHVydGxlLXNvcGhpc20tbW9uYWNvLW1ha2VyLW1pbm9yaXR5LXRoYW5rZnVsLWdyb2Nlci11bmNpYWwtcG9uZ2VlIiwiaWF0IjoxNjYzNjk4NDkzfQ.wEOeF3Up1aJOtFUOLWB4AGKf-NBS609UoL4kIgrSGms";

    private readonly Dictionary<GameplaySessionAPI, string> SessionAPIMap = new()
    {
        { GameplaySessionAPI.Start, "/session/start" },
        { GameplaySessionAPI.Update, "/session/update" },
        { GameplaySessionAPI.End, "/session/end" }
    };

    private readonly Dictionary<PlayerAPI, string> playerAPIMap = new()
    {
        { PlayerAPI.Create, "/player" },
        { PlayerAPI.UpdateNickname, "/player/nickname" },
        { PlayerAPI.Get, "/player/me/" },
        { PlayerAPI.Top100, "/player/score" }
    };

    private readonly Dictionary<SignatureLoginAPI, string> signatureAPIMap = new()
    {
        { SignatureLoginAPI.Get, "/auth/login-schema/" }
    };

    private string Encrypt(string jsonForm)
    {
        string password = EnvironmentManager.Instance.GetEncryptPass();
        EncryptedData jsonEncryptedForm = new()
        {
            data = SimpleAESEncryption.Encrypt2(jsonForm, password)
        };
        return JsonUtility.ToJson(jsonEncryptedForm);
    }

    private UnityWebRequest SetupPostWebRequest(string api, string formData)
    {
        string encryptedFormData = Encrypt(formData);
        string serverUrl = EnvironmentManager.Instance.GetServerUrl();
        UnityWebRequest webRequest = UnityWebRequest.Post(serverUrl + api, encryptedFormData);
        UploadHandler customUploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(encryptedFormData));
        customUploadHandler.contentType = "application/json";
        webRequest.uploadHandler = customUploadHandler;
        webRequest.SetRequestHeader("Authorization", "Bearer " + sessionToken);
        webRequest.SetRequestHeader("Access-Control-Allow-Origin", "*");
        webRequest.SetRequestHeader("Content-Type", "application/json");
        webRequest.SetRequestHeader("Accept", "*/*");
        return webRequest;
    }

    private UnityWebRequest SetupGetWebRequest(string api)
    {
        string serverUrl = EnvironmentManager.Instance.GetServerUrl();
        UnityWebRequest webRequest = UnityWebRequest.Get(serverUrl + api);

        webRequest.SetRequestHeader("Content-Type", "application/json");
        webRequest.SetRequestHeader("Access-Control-Allow-Origin", "*");
        webRequest.SetRequestHeader("Accept", "*/*");
        return webRequest;
    }

    private void SendWebRequest(UnityWebRequest webRequest, Action<string> onComplete, Action<string> onFail = null)
    {
        AsyncOperation operation = webRequest.SendWebRequest();
        operation.completed += (result) =>
        {
            string data = webRequest.downloadHandler.text;
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                onComplete?.Invoke(data);
            }
            else
            {
                onFail?.Invoke(data);
            }

            webRequest.Dispose();
        };
    }

    public void SendGameplayDataToServer(GameplaySessionAPI api, string formData, Action<string> onComplete,
        Action<string> onFail = null)
    {
        UnityWebRequest webRequest = SetupPostWebRequest(SessionAPIMap[api], formData);
        SendWebRequest(webRequest, onComplete, onFail);
    }

    public void SendPlayerDataToServer(PlayerAPI api, string formData, Action<string> onComplete,
        Action<string> onFail = null)
    {
        UnityWebRequest webRequest = SetupPostWebRequest(playerAPIMap[api], formData);
        SendWebRequest(webRequest, onComplete, onFail);
    }

    public void GetPlayerDataFromServer(PlayerAPI api, Action<string> onComplete, string address = "",
        Action<string> onFail = null)
    {
        UnityWebRequest webRequest = SetupGetWebRequest(playerAPIMap[api] + address);
        SendWebRequest(webRequest, onComplete, onFail);
    }

    public void GetLoginSignatureDataFromServer(SignatureLoginAPI api, Action<string> onComplete, string address = "", Action<string> onFail = null)
    {
        UnityWebRequest webRequest = SetupGetWebRequest(signatureAPIMap[api] + address);
        SendWebRequest(webRequest, onComplete, onFail);
    }
}