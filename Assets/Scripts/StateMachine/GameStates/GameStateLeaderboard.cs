using System.Collections.Generic;
using BubbleBots.Server.Player;
using UnityEngine;

public class GameStateLeaderboard : GameState
{
    private GameScreenLeaderboard _gameScreenLeaderboard;
    private enum SelectedTab {Free, Nether}

    private SelectedTab currentTab;
    
    public override string GetGameStateName()
    {
        return "game state leaderboard";
    }

    public override void Enable()
    {
        _gameScreenLeaderboard = Screens.Instance.PushScreen<GameScreenLeaderboard>();
        Screens.Instance.SetBackground(_gameScreenLeaderboard.BackgroundImage);
        GameEventsManager.Instance.AddGlobalListener(OnGameEvent);
        currentTab = SelectedTab.Nether;
        ShowFreeData();
    }

    private void GenerateEntries(List<ResponseLeaderboardDataEntry> entries)
    {
        _gameScreenLeaderboard.ClearEntries();
        foreach (ResponseLeaderboardDataEntry responseLeaderboardDataEntry in entries)
        {
            LeaderboardEntry entry = new()
            {
                nickname = string.IsNullOrEmpty(responseLeaderboardDataEntry.nickname) ? responseLeaderboardDataEntry.address : responseLeaderboardDataEntry.nickname,
                rank = responseLeaderboardDataEntry.rank.ToString(),
                score = responseLeaderboardDataEntry.score.ToString()
            };
            _gameScreenLeaderboard.AddEntry(entry);
        }
        
    }

    private void ShowFreeData()
    {
        if (currentTab == SelectedTab.Free)
        {
            return;
        }
        _gameScreenLeaderboard.ActivateFreeTab();
        currentTab = SelectedTab.Free;
        ServerManager.Instance.GetPlayerDataFromServer(PlayerAPI.Top100Free, (data) =>
        {
            Debug.Log(data);
            GetLeaderboardData leaderboardData = JsonUtility.FromJson<GetLeaderboardData>(data);
            GenerateEntries(leaderboardData.activities);
        });
    }

    private void ShowNetherData()
    {
        if (currentTab == SelectedTab.Nether)
        {
            return;
        }
        _gameScreenLeaderboard.ActivateNetherTab();
        currentTab = SelectedTab.Nether;
        ServerManager.Instance.GetPlayerDataFromServer(PlayerAPI.Top100Pro, (data) =>
        {
            GetLeaderboardData leaderboardData = JsonUtility.FromJson<GetLeaderboardData>(data);
            GenerateEntries(leaderboardData.activities);
        });
        
    }

    private void OnGameEvent(GameEventData data)
    {
        if (data.eventName == GameEvents.ButtonTap)
        {
            OnButtonTap(data);
        }
    }

    private void OnButtonTap(GameEventData data)
    {
        GameEventString customButtonData = data as GameEventString;
        switch (customButtonData.stringData)
        {

            case ButtonId.LeaderboardFree:
                ShowFreeData();
                break;
            case ButtonId.LeaderboardNether:
                ShowNetherData();
                break;
            case ButtonId.LeaderboardClose:
                break;
        }
    }

    public override void Disable()
    {
        Screens.Instance.PopScreen(_gameScreenLeaderboard);
        GameEventsManager.Instance.RemoveGlobalListener(OnGameEvent);
    }
}

public class LeaderboardEntry
{
    public string rank;
    public string nickname;
    public string score;
}