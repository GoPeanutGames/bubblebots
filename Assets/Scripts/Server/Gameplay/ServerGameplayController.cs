using BubbleBots.Server.Gameplay;
using System;
using UnityEngine;

public class ServerGameplayController : MonoBehaviour
{
    private string currentGameplaySessionID;
    private int currentLevel;

    private bool sessionStarted = false;
    private int previousScore;

    private void OnGameplaySessionStart(string data)
    {
        GameplaySessionResult result = JsonUtility.FromJson<GameplaySessionResult>(data);
        currentGameplaySessionID = result.sessionId;
        sessionStarted = true;
    }

    private void OnGameplaySessionEnd(string data)
    {
        currentGameplaySessionID = "";
    }

    public void StartGameplaySession(int level)
    {
        string address = WalletManager.Instance.GetWalletAddress();
        if (string.IsNullOrEmpty(address))
        {
            return;
        }
        currentLevel = level;
        GameplaySessionStartData formData = new()
        {
            address = WalletManager.Instance.GetWalletAddress(),
            timezone = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).TotalHours.ToString(),
            mode = ModeManager.Instance.Mode.ToString(),
            startTime = DateTime.Now.ToString("O"),
        };
        string jsonFormData = JsonUtility.ToJson(formData);
        ServerManager.Instance.SendGameplayDataToServer(GameplaySessionAPI.Start, jsonFormData, OnGameplaySessionStart);
    }

    public void UpdateGameplaySession(int score)
    {
        previousScore = score;
        GameplaySessionUpdateData formData = new()
        {
            sessionId = currentGameplaySessionID,
            score = score,
            level = currentLevel,
            kills = (int)LeaderboardManager.Instance.RobotsKilled
        };
        string jsonFormData = JsonUtility.ToJson(formData);
        ServerManager.Instance.SendGameplayDataToServer(GameplaySessionAPI.Update, jsonFormData, _ => { });
    }

    public void EndGameplaySession(int score)
    {
        previousScore = score;
        GameplaySessionEndData formData = new()
        {
            sessionId = currentGameplaySessionID,
            score = score,
            endTime = DateTime.Now.ToString("O"),
        };
        string jsonFormData = JsonUtility.ToJson(formData);
        ServerManager.Instance.SendGameplayDataToServer(GameplaySessionAPI.End, jsonFormData, OnGameplaySessionEnd);
    }

    private void OnApplicationQuit()
    {
        if (sessionStarted)
        {
            EndGameplaySession(previousScore);
        }
    }
}
