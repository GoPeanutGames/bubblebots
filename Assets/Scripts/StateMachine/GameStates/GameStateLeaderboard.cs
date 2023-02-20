using System.Collections.Generic;
using BubbleBots.Server.Player;
using UnityEngine;

public class GameStateLeaderboard : GameState
{
    private GameScreenLeaderboard _gameScreenLeaderboard;
    private GameScreenHomeHeader _gameScreenHomeHeader;
    private enum SelectedTab {Free, Nether}

    private SelectedTab currentTab;
    private bool fetchingData = false;
    
    public override string GetGameStateName()
    {
        return "game state leaderboard";
    }

    public override void Enable()
    {
        _gameScreenLeaderboard = Screens.Instance.PushScreen<GameScreenLeaderboard>();
        _gameScreenLeaderboard.StartOpen();
        GameEventsManager.Instance.AddGlobalListener(OnGameEvent);
        currentTab = SelectedTab.Nether;
        ShowFreeData();
    }

    private void GenerateEntries(List<ResponseLeaderboardDataEntry> entries)
    {
        foreach (ResponseLeaderboardDataEntry responseLeaderboardDataEntry in entries)
        {
            LeaderboardEntry entry = new()
            {
                nickname = string.IsNullOrEmpty(responseLeaderboardDataEntry.nickname) ? responseLeaderboardDataEntry.address.Substring(0,10) + "..." : responseLeaderboardDataEntry.nickname,
                rank = responseLeaderboardDataEntry.rank.ToString(),
                score = responseLeaderboardDataEntry.score.ToString()
            };
            _gameScreenLeaderboard.AddEntry(entry);
        }
        
    }

    private void ShowFreeData()
    {
        if (currentTab == SelectedTab.Free || fetchingData)
        {
            return;
        }
        fetchingData = true;
        _gameScreenLeaderboard.ClearEntries();
        _gameScreenLeaderboard.ActivateFreeTab();
        currentTab = SelectedTab.Free;
        ServerManager.Instance.GetPlayerDataFromServer(PlayerAPI.Top100Free, (data) =>
        {
            GetLeaderboardData leaderboardData = JsonUtility.FromJson<GetLeaderboardData>(data);
            GenerateEntries(leaderboardData.activities);
            fetchingData = false;
        });
    }

    private void ShowNetherData()
    {
        if (currentTab == SelectedTab.Nether || fetchingData)
        {
            return;
        }
        fetchingData = true;
        _gameScreenLeaderboard.ClearEntries();
        _gameScreenLeaderboard.ActivateNetherTab();
        currentTab = SelectedTab.Nether;
        ServerManager.Instance.GetPlayerDataFromServer(PlayerAPI.Top100Pro, (data) =>
        {
            GetLeaderboardData leaderboardData = JsonUtility.FromJson<GetLeaderboardData>(data);
            GenerateEntries(leaderboardData.activities);
            fetchingData = false;
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
                stateMachine.PushState(new GameStateHome());
                break;
        }
    }

    public override void Disable()
    {
        _gameScreenLeaderboard.StartClose();
        GameEventsManager.Instance.RemoveGlobalListener(OnGameEvent);
    }
}

public class LeaderboardEntry
{
    public string rank;
    public string nickname;
    public string score;
}