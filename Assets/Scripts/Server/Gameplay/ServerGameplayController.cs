using BubbleBots.Server.Gameplay;
using System;
using UnityEngine;

public class ServerGameplayController : MonoBehaviour
{
    private string currentGameplaySessionID;
    private int currentLevel;

    private bool sessionStarted = false;

    private void OnGameplaySessionStart(string data)
    {
        Debug.Log(data);
        GameplaySessionResult result = JsonUtility.FromJson<GameplaySessionResult>(data);
        currentGameplaySessionID = result.sessionId;
        sessionStarted = true;
        Debug.Log(currentGameplaySessionID);
    }

    private void OnGameplaySessionUpdate(string data)
    {
        Debug.Log(data);
        GameplaySessionResult result = JsonUtility.FromJson<GameplaySessionResult>(data);
        if(result.sessionId == currentGameplaySessionID)
        {
            Debug.Log("OK");
        }
    }

    private void OnGameplaySessionEnd(string data)
    {
        Debug.Log(data);
        GameplaySessionResult result = JsonUtility.FromJson<GameplaySessionResult>(data);
        if (result.sessionId == currentGameplaySessionID)
        {
            Debug.Log("OK - ENDING");
        }
        currentGameplaySessionID = "";
    }

    public void StartGameplaySession(int level)
    {
        string address = WalletManager.Instance.GetWalletAddress();
        Debug.Log(address);
        if (string.IsNullOrEmpty(address))
        {
            return;
        }
        currentLevel = level;
        GameplaySessionStartData formData = new()
        {
            address = WalletManager.Instance.GetWalletAddress(),
            timezone = TimeZoneInfo.Local.DisplayName,
            mode = ModeManager.Instance.Mode.ToString(),
            startTime = DateTime.Now.ToString(),
        };
        string jsonFormData = JsonUtility.ToJson(formData);
        ServerManager.Instance.SendGameplayDataToServer(GameplaySessionAPI.Start, jsonFormData, OnGameplaySessionStart);
    }

    public void UpdateGameplaySession()
    {
        GameplaySessionUpdateData formData = new()
        {
            sessionId = currentGameplaySessionID,
            score = (int)LeaderboardManager.Instance.Score,
            level = currentLevel,
            kills = (int)LeaderboardManager.Instance.RobotsKilled
        };
        string jsonFormData = JsonUtility.ToJson(formData);
        ServerManager.Instance.SendGameplayDataToServer(GameplaySessionAPI.Update, jsonFormData, OnGameplaySessionUpdate);
    }

    public void EndGameplaySession()
    {
        GameplaySessionEndData formData = new()
        {
            sessionId = currentGameplaySessionID,
            score = (int)LeaderboardManager.Instance.Score,
            endTime = DateTime.Now.ToString(),
        };
        string jsonFormData = JsonUtility.ToJson(formData);
        ServerManager.Instance.SendGameplayDataToServer(GameplaySessionAPI.End, jsonFormData, OnGameplaySessionEnd);
    }

    private void OnApplicationQuit()
    {
        if (sessionStarted)
        {
            EndGameplaySession();
        }
    }
}
