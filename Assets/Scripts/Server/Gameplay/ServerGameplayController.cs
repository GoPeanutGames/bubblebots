using BubbleBots.Server.Gameplay;
using System;
using UnityEngine;

public class ServerGameplayController : MonoSingleton<ServerGameplayController>
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
        if (UserManager.PlayerType == PlayerType.Guest)
        {
            return;
        }
        string address = UserManager.Instance.GetPlayerWalletAddress();
        currentLevel = level;
        GameplaySessionStartData formData = new()
        {
            address = address,
            timezone = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).TotalHours.ToString(),
            mode = ModeManager.Instance.Mode.ToString(),
            startTime = DateTime.Now.ToString("O"),
        };
        string jsonFormData = JsonUtility.ToJson(formData);
        ServerManager.Instance.SendGameplayDataToServer(GameplaySessionAPI.Start, jsonFormData, OnGameplaySessionStart);
    }

    public void UpdateGameplaySession(int score)
    {
        if (UserManager.PlayerType == PlayerType.Guest)
        {
            return;
        }
        previousScore = score;
        GameplaySessionUpdateData formData = new()
        {
            sessionId = currentGameplaySessionID,
            score = score,
            level = currentLevel,
            kills = UserManager.RobotsKilled
        };
        string jsonFormData = JsonUtility.ToJson(formData);
        ServerManager.Instance.SendGameplayDataToServer(GameplaySessionAPI.Update, jsonFormData, _ => { });
    }

    public void EndGameplaySession(int score)
    {
        if (UserManager.PlayerType == PlayerType.Guest)
        {
            return;
        }
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
