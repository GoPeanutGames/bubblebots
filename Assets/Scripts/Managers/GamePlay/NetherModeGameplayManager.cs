using BubbleBots.Data;
using BubbleBots.Gameplay.Models;
using BubbleBots.Modes;
using CodeStage.AntiCheat.ObscuredTypes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NethermodeSessionData
{
    ObscuredLong score = 0;

    private int scoreMultiplier = 10;// hardcoded score multiplier

    private int robotsKilled = 0;

    private int potentialBubbles = 0;

    private int totalBubbles = 0;

    public void AddTotalBubbles(int val)
    {
        totalBubbles += val;
    }

    public int GetTotalBubbles()
    {
        return totalBubbles;
    }

    public void IncrementScore(int toAdd)
    {
        score += toAdd * scoreMultiplier;
    }

    public long GetScore()
    {
        return score;
    }

    public void IncrementRobotsKilled(int killed)
    {
        robotsKilled += killed;
    }

    public int GetRobotsKilled()
    {
        return robotsKilled;
    }

    public void AddPotentialBubbles(int value)
    {
        potentialBubbles += value;
    }

    public int GetPotentialBubbles()
    {
        return potentialBubbles;
    }
    public void ResetPotentialBubbles()
    {
        potentialBubbles = 0;
    }
}

public class NetherModeGameplayManager : MonoBehaviour
{
    private enum NethermodeGameplayState
    {
        ShowingLevelText,
        Match3Playing,
        LevelComplete,
        EndLevelSequence,
        LevelCompleteMenu,
        GameEndMenu,
        NetherModeComplete
    }

    private NethermodeGameplayState gameplayState;

    public NetherModeGameplayData gameplayData;

    public ServerGameplayController serverGameplayController;
    //public GUIGame GameGUI;
    public float enemyDamage = 0.05f;

    int currentLevelIndex = 0;
    int currentEnemy = 0;

    private NethermodeSessionData sessionData;

    private PlayerRoster playerRoster;
    private Wave currentWave;
    private int currentWaveIndex;

    public Match3GameplayManager match3Manager;

    public void Start()
    {
        gameplayState = NethermodeGameplayState.ShowingLevelText;
        SoundManager.Instance?.FadeOutMusic(() =>
        {
            SoundManager.Instance.PlayNetherModeMusic();
            SoundManager.Instance.FadeInMusic();
        });

    }
    public void StartSession(List<BubbleBotData> bots)
    {
        ModeManager.Instance.SetMode(Mode.PRO);
        sessionData = new NethermodeSessionData();
        playerRoster = new PlayerRoster()
        {
            bots = new List<BubbleBot>()
            {
                new BubbleBot { maxHp = 12, hp = 12, id = bots[0].id, bubbleBotData = bots[0] },
                new BubbleBot { maxHp = 12, hp = 12, id = bots[1].id, bubbleBotData = bots[1] },
                new BubbleBot { maxHp = 12, hp = 12, id = bots[2].id, bubbleBotData = bots[2] },
            },
            currentBot = 0
        };
        currentLevelIndex = 0;

        match3Manager.Initialize(gameplayData.levels[currentLevelIndex], true);

        match3Manager.onGemsExploded -= OnGemsExploded;
        match3Manager.onGemsExploded += OnGemsExploded;

        match3Manager.onBoardEventsEnded -= HitPlayer;
        match3Manager.onBoardEventsEnded += HitPlayer;

        match3Manager.onBubbleExploded -= OnBubbleExploded;
        match3Manager.onBubbleExploded += OnBubbleExploded;

        GameEventsManager.Instance.AddGlobalListener(OnGameEvent);

        GameEventsManager.Instance.PostEvent(new GameEventData() { eventName = GameEvents.FreeModeSessionStarted });
        UserManager.RobotsKilled = 0;
        //MenuGUI.ResetScores();
        StartLevel();
    }

    private void OnGameEvent(GameEventData data)
    {
        if (data.eventName == GameEvents.FreeModeEnemyChanged)
        {
            SetEnemy((data as GameEventInt).intData);
        }
    }

    public void SetPlayerRoster()
    {

    }

    public void StartLevel()
    {
        match3Manager.Initialize(gameplayData.levels[Mathf.Min(gameplayData.levels.Count - 1, currentLevelIndex)], true);

        currentWaveIndex = 0;
        currentEnemy = 0;
        currentWave = new Wave()
        {
            bots = new List<BubbleBot>()
            {
                new BubbleBot() { hp = 60 },
                new BubbleBot() { hp = 60 },
                new BubbleBot() { hp = 60 }
            },
            completed = false
        };

        //GameGUI.gameObject.SetActive(true);
        //MenuGUI.gameObject.SetActive(false);

        AnalyticsManager.Instance?.SendPlayEvent(currentLevelIndex + 1);
        serverGameplayController?.StartGameplaySession(currentLevelIndex + 1);


        GameEventsManager.Instance.PostEvent(new GameEventLevelStart() { eventName = GameEvents.FreeModeLevelStart, enemies = currentWave.bots, playerRoster = this.playerRoster });

        FindObjectOfType<GUIGame>().SetRobotGauges(currentWave.bots);
        FindObjectOfType<GUIGame>().SetPlayerRobots(playerRoster);
        //GameGUI.SetPlayerRobots(playerRoster);


        gameplayState = NethermodeGameplayState.ShowingLevelText;
    }

    public void StartNextLevel()
    {
        currentLevelIndex++;
        //MenuGUI.HideWin();
        StartLevel();
    }

    public void OnGemsExploded(int count)
    {
        HitEnemy(count);
        IncrementScore(count);
    }
    private void OnWaveFinished()
    {
        currentWaveIndex++;
        currentWave.completed = true;

        if (currentWaveIndex >= gameplayData.levels[Mathf.Min(gameplayData.levels.Count - 1, currentLevelIndex)].waves)
        {
            OnLevelFinished();
        }
    }

    private void OnLevelFinished()
    {
        gameplayState = NethermodeGameplayState.LevelComplete;
        if (currentLevelIndex >= gameplayData.levels.Count - 1)
        {
            gameplayState = NethermodeGameplayState.NetherModeComplete;
            OnNetherModeComplete();
        }
    }

    private void HitEnemy(int damage)
    {
        if (currentWave.completed)
        {
            return;
        }

        currentWave.bots[currentEnemy].hp -= damage;

        GameEventsManager.Instance.PostEvent(new GameEventEnemyRobotDamage() { eventName = GameEvents.FreeModeEnemyRobotDamage, enemyRobotNewHp = currentWave.bots[currentEnemy].hp });
        //GameGUI.DamageToEnemyRobot(currentWave.bots[currentEnemy].hp);
        FindObjectOfType<GUIGame>().DamageToEnemyRobot(currentWave.bots[currentEnemy].hp);


        if (currentWave.bots[currentEnemy].hp <= 0)
        {
            bool waveFinished = KillEnemy();

            if (waveFinished)
            {
                OnWaveFinished();
            }
        }
    }

    private void HitPlayer()
    {
        if (gameplayState == NethermodeGameplayState.LevelComplete || currentWave.completed)
        {
            return;
        }
        playerRoster.TakeDamage((int)enemyDamage);

        if (playerRoster.AreAllBotsDead())
        {
            OnPlayerLost();
            GameEventsManager.Instance.PostEvent(new GameEventPlayerRobotKilled() { eventName = GameEvents.FreeModePlayerRobotKilled, id = playerRoster.currentBot });
            //GameGUI.KillPlayerRobot(playerRoster.currentBot);
            FindObjectOfType<GUIGame>().DamageToEnemyRobot(currentWave.bots[currentEnemy].hp);
        }
        else if (playerRoster.IsDead(playerRoster.currentBot))
        {
            GameEventsManager.Instance.PostEvent(new GameEventPlayerRobotKilled() { eventName = GameEvents.FreeModePlayerRobotKilled, id = playerRoster.currentBot });
            //GameGUI.KillPlayerRobot(playerRoster.currentBot);
            FindObjectOfType<GUIGame>().KillPlayerRobot(playerRoster.currentBot);
            playerRoster.currentBot++;
        }
        else
        {
            GameEventsManager.Instance.PostEvent(new GameEventPlayerRobotDamage() { eventName = GameEvents.FreeModePlayerRobotDamage, id = playerRoster.currentBot, damage = (int)enemyDamage });
            //GameGUI.DamagePlayerRobot(playerRoster.currentBot, (int)enemyDamage);
            FindObjectOfType<GUIGame>().DamagePlayerRobot(playerRoster.currentBot, (int)enemyDamage);
        }
    }

    private void OnPlayerLost()
    {
        gameplayState = NethermodeGameplayState.GameEndMenu;
        //GameEventsManager.Instance.PostEvent(new GameEventFreeModeLose() { eventName = GameEvents.FreeModeLose, score = (int)GetScore() });
        GameEventsManager.Instance.PostEvent(new GameEventFreeModeLose()
        {
            eventName = GameEvents.FreeModeLose,
            score = (int)GetScore(),
            numBubblesWon = sessionData.GetTotalBubbles(),
            lastLevelPotentialBubbles = sessionData.GetPotentialBubbles()
        });
        //GameGUI.DisplayLose((int)GetScore());
        FindObjectOfType<GUIGame>().DisplayLose((int)GetScore());
        serverGameplayController?.EndGameplaySession((int)GetScore(), BubbleBots.Server.Gameplay.GameStatus.LOSE);
    }

    private bool KillEnemy()
    {
        sessionData.IncrementRobotsKilled(1);
        UserManager.RobotsKilled++;
        AnalyticsManager.Instance?.SendRobotKillEvent(UserManager.RobotsKilled);
        serverGameplayController?.UpdateGameplaySession((int)GetScore());

        GameEventsManager.Instance.PostEvent(new GameEventEnemyRobotKilled() { eventName = GameEvents.FreeModeEnemyRobotKilled, id = currentEnemy });
        FindObjectOfType<GUIGame>().KillEnemy();
        //GameGUI.KillEnemy();

        bool allEnemiesKilled = true;

        currentEnemy = (currentEnemy + 1) % currentWave.bots.Count;
        if (currentWave.bots[currentEnemy].hp <= 0)
        {
            for (int i = currentWave.bots.Count - 1; i >= 0; i--)
            {
                if (currentWave.bots[i].hp > 0)
                {
                    currentEnemy = i;
                    allEnemiesKilled = false;
                    break;
                }
            }
        }
        else
        {
            allEnemiesKilled = false;
        }

        if (!allEnemiesKilled)
        {
            GameEventsManager.Instance.PostEvent(new GameEventEnemyRobotTargeted() { eventName = GameEvents.FreeModeEnemyRobotTargeted, id = currentEnemy });
            FindObjectOfType<GUIGame>().TargetEnemy(currentEnemy, false);
        }
        return allEnemiesKilled;
    }


    public void SetEnemy(int currentEmeny)
    {
        currentEnemy = currentEmeny;
    }

    public long GetScore()
    {
        return sessionData.GetScore();
    }

    public int GetCurrentLevel()
    {
        return currentLevelIndex;
    }

    public void IncrementScore(int toAdd)
    {
        sessionData.IncrementScore(toAdd);
        serverGameplayController?.UpdateGameplaySession((int)sessionData.GetScore());
        GameEventsManager.Instance.PostEvent(new GameEventScoreUpdate() { eventName = GameEvents.FreeModeScoreUpdate, score = (int)sessionData.GetScore() });
        //GameGUI.UpdateScore((int)sessionData.GetScore());
        FindObjectOfType<GUIGame>().UpdateScore((int)sessionData.GetScore());
    }

    public bool CanShowQuitPopup()
    {
        return gameplayState == NethermodeGameplayState.Match3Playing &&
            match3Manager.GetGameplayState() == Match3GameplayManager.GameplayState.WaitForInput;
    }

    private void Update()
    {
        switch (gameplayState)
        {
            case NethermodeGameplayState.ShowingLevelText:
                StartCoroutine(ShowLevelText(2f, 0.5f));
                break;
            case NethermodeGameplayState.Match3Playing:
                match3Manager.UpdateMatch3Logic();
                break;
            case NethermodeGameplayState.LevelComplete:
                StartCoroutine(EndLevelSequence());
                match3Manager.UpdateMatch3Logic();
                break;
            case NethermodeGameplayState.EndLevelSequence:
                match3Manager.UpdateMatch3Logic();
                break;
            case NethermodeGameplayState.NetherModeComplete:
                break;
        }
    }

    public bool IsFinished()
    {
        return gameplayState == NethermodeGameplayState.NetherModeComplete;
    }

    private void OnNetherModeComplete()
    {
        serverGameplayController?.EndGameplaySession((int)GetScore(), BubbleBots.Server.Gameplay.GameStatus.WON);
        GameEventsManager.Instance.PostEvent(new GameEventNetherModeComplete() { eventName = GameEvents.NetherModeComplete, numBubblesWon = sessionData.GetTotalBubbles()});
    }

    IEnumerator EndLevelSequence()
    {
        gameplayState = NethermodeGameplayState.EndLevelSequence;
        yield return new WaitUntil(() => match3Manager.GetGameplayState() == Match3GameplayManager.GameplayState.WaitForInput);
        while (match3Manager.HasSpecials())
        {
            match3Manager.ExplodeAllSpecials();
            yield return new WaitUntil(() => match3Manager.GetGameplayState() == Match3GameplayManager.GameplayState.WaitForInput);
            if (match3Manager.HasSpecials())
            {
                yield return new WaitForSeconds(.1f);
            }
        }
        gameplayState = NethermodeGameplayState.LevelCompleteMenu;
        UserManager.Instance.SetPlayerScore((int)GetScore());
        serverGameplayController?.EndGameplaySession((int)GetScore(), BubbleBots.Server.Gameplay.GameStatus.WON);
        AnalyticsManager.Instance.SendLevelEvent((int)GetScore());

        UserManager.Instance?.AddBubbles(sessionData.GetPotentialBubbles());

        sessionData.AddTotalBubbles(sessionData.GetPotentialBubbles());

        GameEventsManager.Instance.PostEvent(new GameEventLevelComplete() { eventName = GameEvents.FreeModeLevelComplete, numBubblesWon = sessionData.GetTotalBubbles(), lastLevelPotentialBubbles = sessionData.GetPotentialBubbles() });

        sessionData.ResetPotentialBubbles();
        FindObjectOfType<GUIGame>().SetUnclaimedBubblesText(sessionData.GetPotentialBubbles());



        //MenuGUI.DisplayWin();
    }

    IEnumerator ShowLevelText(float _duration, float _fadeDuration)
    {
        GameEventsManager.Instance.PostEvent(new GameEventShowLevelText() { eventName = GameEvents.ShowLevetText, duration = _duration, fadeDuration = _fadeDuration });
        //GameGUI.ShowLevelText(currentLevelIndex, duration, fadeDuration);
        FindObjectOfType<GUIGame>().ShowLevelText(currentLevelIndex, _duration, _fadeDuration);
        yield return new WaitForSeconds(_duration);

        currentEnemy = 0;
        GameEventsManager.Instance.PostEvent(new GameEventEnemyRobotTargeted() { eventName = GameEvents.FreeModeEnemyRobotTargeted, id = currentEnemy });
        FindObjectOfType<GUIGame>().TargetEnemy(currentEnemy);
        //GameGUI.TargetEnemy(currentEnemy);

        gameplayState = NethermodeGameplayState.Match3Playing;
    }
    public void TargetEnemy(int index)
    {
        currentEnemy = index;
    }

    private void OnBubbleExploded(int _posX, int _posY)
    {
        serverGameplayController?.UpdateGameplaySession((int)sessionData.GetScore(), true, (val) => { FindObjectOfType<GUIGame>().ExplodeBubble(_posX, _posY, val - sessionData.GetPotentialBubbles()); });
        GameEventsManager.Instance.PostEvent(new GameEventBubbleExploded() { eventName = GameEvents.BubbleExploded, posX = _posX, posY = _posY });
        //FindObjectOfType<GUIGame>().ExplodeBubble(_posX, _posY, 0);
        //GameEventsManager.Instance.PostEvent(new GameEventUpdateUnclaimedBubbles() { eventName = GameEvents.BubblesUnclaimedUpdate, balance = sessionData.GetPotentialBubbles() });
        //FindObjectOfType<GUIGame>().SetUnclaimedBubblesText(sessionData.GetPotentialBubbles());
        //GameGUI.SetUnclaimedBubblesText(sessionData.GetPotentialBubbles());
    }

    public void OnNewBubblesCount(int newValue)
    {
        int diff = newValue - sessionData.GetPotentialBubbles();
        sessionData.AddPotentialBubbles(diff);
        FindObjectOfType<GUIGame>().SetUnclaimedBubblesText(sessionData.GetPotentialBubbles());
    }
}
