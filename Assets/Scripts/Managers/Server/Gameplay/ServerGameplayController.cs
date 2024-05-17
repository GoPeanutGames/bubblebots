using System;
using BubbleBots.Server.Gameplay;
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
        GameplaySessionUpdateDataResponse r = JsonUtility.FromJson<GameplaySessionUpdateDataResponse>(data);
        Debug.Log("[RECEIVE]received score update: " + r.score);
        currentGameplaySessionID = "";
    }

    public void StartGameplaySession(int level)
    {
        if (UserManager.PlayerType == PlayerType.Guest)
        {
            return;
        }
        string address = UserManager.Instance.GetPlayerWalletAddress();
        string signature = UserManager.Instance.GetPlayerSignature();
        currentLevel = level;
        GameplaySessionStartData formData = new()
        {
            signature = signature,
            address = address,
            mode = ModeManager.Instance.Mode.ToString(),
            level = level,
            timezone = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).TotalHours.ToString(),
            startTime = DateTime.Now.ToString("O"),
        };
        string jsonFormData = JsonUtility.ToJson(formData);
        ServerManager.Instance.SendGameplayDataToServer(GameplaySessionAPI.Start, jsonFormData, OnGameplaySessionStart);
    }

    public void UpdateGameplaySession(int score, bool bubbleBurst = false, System.Action<int, int> callback = null)
    {
        Debug.Log("[SEND_UNATH]sending score update: " + score);
        if (UserManager.PlayerType == PlayerType.Guest)
        {
            return;
        }

        Debug.Log("[SEND]sending score update: " + score);
        previousScore = score;
        GameplaySessionUpdateData formData = new()
        {
            sessionId = currentGameplaySessionID,
            score = score,
            level = currentLevel,
            mode = ModeManager.Instance.Mode.ToString(),
            specialBurst = bubbleBurst,
            kills = UserManager.RobotsKilled,
            status = Enum.GetName(typeof(GameStatus), GameStatus.PLAYING) // should always be this
        };
        string jsonFormData = JsonUtility.ToJson(formData);
        ServerManager.Instance.SendGameplayDataToServer(GameplaySessionAPI.Update, jsonFormData, (response) => {
            GameplaySessionUpdateDataResponse r = JsonUtility.FromJson<GameplaySessionUpdateDataResponse>(response);
            if (callback != null)
            {
                callback(r.bubbles, r.specialBurst);
            }
            GameEventsManager.Instance.PostEvent(new GameEventUpdateSession() { eventName = GameEvents.UpdateSessionResponse, bubbles = r.bubbles });
            Debug.Log("[RECEIVE]received score update: " + r.score);
        });
    }

    public void EndGameplaySession(int score, GameStatus gameStatus)
    {
        Debug.Log("[SEND_UNATH]sending score update on END: " + score);
        if (UserManager.PlayerType == PlayerType.Guest)
        {
            return;
        }
        Debug.Log("[SEND]sending score update on END: " + score);
        previousScore = score;
        GameplaySessionEndData formData = new()
        {
            sessionId = currentGameplaySessionID,
            score = score,
            endTime = DateTime.Now.ToString("O"),
            status = Enum.GetName(typeof(GameStatus), gameStatus)
        };
        string jsonFormData = JsonUtility.ToJson(formData);
        ServerManager.Instance.SendGameplayDataToServer(GameplaySessionAPI.End, jsonFormData, OnGameplaySessionEnd);
    }

    private void OnApplicationQuit()
    {
        if (sessionStarted)
        {
            EndGameplaySession(previousScore, GameStatus.LOSE);
        }
    }
}
