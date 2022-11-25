using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using BubbleBots.Server;
using BubbleBots.Server.Gameplay;

public class ServerManager : MonoSingleton<ServerManager>
{
    private string sessionToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJhdXRoIjoiYWYtdXNlciIsImFnZW50IjoiIiwidG9rZW4iOiJmcmV5LXBhcmstc3RhdmUtaHVydGxlLXNvcGhpc20tbW9uYWNvLW1ha2VyLW1pbm9yaXR5LXRoYW5rZnVsLWdyb2Nlci11bmNpYWwtcG9uZ2VlIiwiaWF0IjoxNjYzNjk4NDkzfQ.wEOeF3Up1aJOtFUOLWB4AGKf-NBS609UoL4kIgrSGms";
    private readonly Dictionary<GameplaySessionAPI, string> SessionAPIMap = new()
    {
        { GameplaySessionAPI.Start, "/session/start"},
        { GameplaySessionAPI.Update, "/session/update"},
        { GameplaySessionAPI.End, "/session/end"}
    };
    private string createPlayerAPI = "/player";

    private string Encrypt(string jsonForm)
    {
        string password = EnvironmentManager.Instance.GetEncryptPass();
        EncryptedData jsonEncryptedForm = new()
        {
            data = SimpleAESEncryption.Encrypt2(jsonForm, password)
        };
        return JsonUtility.ToJson(jsonEncryptedForm);
    }

    private UnityWebRequest SetupWebRequest(string api, string formData)
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

    private void SendWebRequest(UnityWebRequest webRequest, Action<string> onComplete)
    {
        AsyncOperation operation = webRequest.SendWebRequest();
        operation.completed += (result) =>
        {
            onComplete?.Invoke(webRequest.downloadHandler.text);
            webRequest.Dispose();
        };
    }

    public void SendGameplayDataToServer(GameplaySessionAPI api, string formData, Action<string> onComplete)
    {
        UnityWebRequest webRequest = SetupWebRequest(SessionAPIMap[api], formData);
        SendWebRequest(webRequest, onComplete);
    }

    public void CreatePlayerOnServer(string formData, Action<string> onComplete)
    {
        UnityWebRequest webRequest = SetupWebRequest(createPlayerAPI, formData);
        SendWebRequest(webRequest, onComplete);
    }
}
