using System.Collections;
using System.Collections.Generic;
using BubbleBots.Data;
using BubbleBots.Gameplay.Models;
using BubbleBots.Modes;
using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine;

public class FreeToPlaySessionData
{
    ObscuredLong score = 0;

    private int scoreMultiplier = 10;// hardcoded score multiplier

    private int robotsKilled = 0;

    private int potentialBubbles = 0;

    private int totalBubbles = 0;

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

    public void AddTotalBubbles(int val)
    {
        totalBubbles += val;
    }

    public int GetTotalBubbles()
    {
        return totalBubbles;
    }

    public void ResetPotentialBubbles()
    {
        potentialBubbles = 0;
    }
}

public class FreeToPlayGameplayManager : MonoBehaviour
{
    private enum FreeToPlayGameplayState
    {
        ShowingLevelText,
        Match3Playing,
        LevelComplete,
        EndLevelSequence,
        LevelCompleteMenu,
        GameEndMenu
    }

    private FreeToPlayGameplayState gameplayState;

    public FreeToPlayGameplayData gameplayData;

    public ServerGameplayController serverGameplayController;
    //public GUIGame GameGUI;
    public float enemyDamage = 0.05f;

    int currentLevelIndex = 0;
    int currentEnemy = 0;

    private FreeToPlaySessionData sessionData;

    private PlayerRoster playerRoster;
    private Wave currentWave;
    private int currentWaveIndex;

    public Match3GameplayManager match3Manager;

    public void Start()
    {
        gameplayState = FreeToPlayGameplayState.ShowingLevelText;
        SoundManager.Instance?.FadeOutMusic(() =>
        {
            SoundManager.Instance.PlayLevelMusicNew();
            SoundManager.Instance.FadeInMusic();
        });

    }
    public void StartSession(List<BubbleBotData> bots)
    {
        ModeManager.Instance.SetMode(Mode.FREE);
        if (EnvironmentManager.Instance.IsRhym())
        {
            ModeManager.Instance.SetMode(Mode.RHYM);
        }
        sessionData = new FreeToPlaySessionData();
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

        match3Manager.Initialize(gameplayData.levels[currentLevelIndex], false);

        match3Manager.onGemsExploded -= OnGemsExploded;
        match3Manager.onGemsExploded += OnGemsExploded;

        match3Manager.onBoardEventsEnded -= HitPlayer;
        match3Manager.onBoardEventsEnded += HitPlayer;

        match3Manager.onBubbleExploded -= OnBubbleExploded;
        match3Manager.onBubbleExploded += OnBubbleExploded;

        GameEventsManager.Instance.AddGlobalListener(OnGameEvent);

        GameEventsManager.Instance.PostEvent( new GameEventData() { eventName = GameEvents.FreeModeSessionStarted });
        UserManager.RobotsKilled = 0;
        

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
        if (currentLevelIndex < gameplayData.stopHpRefreshAfterLevel)
        {
            playerRoster.ResetRoster();
        }

        match3Manager.Initialize(gameplayData.levels[Mathf.Min(gameplayData.levels.Count - 1, currentLevelIndex)]);

        List<BubbleBotData> enemiesToChooseFrom = new List<BubbleBotData>(gameplayData.enemyRobots);
        BubbleBotData first = enemiesToChooseFrom[Random.Range(0, enemiesToChooseFrom.Count)];
        enemiesToChooseFrom.Remove(first);
        BubbleBotData second = enemiesToChooseFrom[Random.Range(0, enemiesToChooseFrom.Count)];
        enemiesToChooseFrom.Remove(second);
        BubbleBotData third = enemiesToChooseFrom[Random.Range(0, enemiesToChooseFrom.Count)];
        enemiesToChooseFrom.Remove(third);
        
        currentWaveIndex = 0;
        currentEnemy = 0;
        currentWave = new Wave()
        {
            bots = new List<BubbleBot>()
            {
                new BubbleBot() { hp = 40, maxHp = 40, bubbleBotData = first },
                new BubbleBot() { hp = 40, maxHp = 40, bubbleBotData = second },
                new BubbleBot() { hp = 40, maxHp = 40, bubbleBotData = third }
            },
            completed = false
        };

        AnalyticsManager.Instance?.SendPlayEvent(currentLevelIndex + 1);
        serverGameplayController?.StartGameplaySession(currentLevelIndex + 1);


        GameEventsManager.Instance.PostEvent(new GameEventLevelStart() { eventName = GameEvents.FreeModeLevelStart, enemies = currentWave.bots, playerRoster = this.playerRoster });

        gameplayState = FreeToPlayGameplayState.ShowingLevelText;
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
        gameplayState = FreeToPlayGameplayState.LevelComplete;
    }

    private void HitEnemy(int damage)
    {
        if (currentWave.completed)
        {
            return;
        }

        currentWave.bots[currentEnemy].hp -= damage;

        GameEventsManager.Instance.PostEvent(new GameEventEnemyRobotDamage() {eventName = GameEvents.FreeModeEnemyRobotDamage, index = currentEnemy, enemyRobotNewHp = currentWave.bots[currentEnemy].hp });

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
        if (gameplayState == FreeToPlayGameplayState.LevelComplete || currentWave.completed)
        {
            return;
        }
        playerRoster.TakeDamage((int)enemyDamage);

        if (playerRoster.AreAllBotsDead())
        {
            OnPlayerLost();
            GameEventsManager.Instance.PostEvent(new GameEventPlayerRobotKilled() { eventName = GameEvents.FreeModePlayerRobotKilled, id = playerRoster.currentBot, enemyIndex = currentEnemy});
            //GameGUI.KillPlayerRobot(playerRoster.currentBot);
            // FindObjectOfType<GUIGame>().DamageToEnemyRobot(currentWave.bots[currentEnemy].hp);
        }
        else if (playerRoster.IsDead(playerRoster.currentBot))
        {
            GameEventsManager.Instance.PostEvent(new GameEventPlayerRobotKilled() { eventName = GameEvents.FreeModePlayerRobotKilled, id = playerRoster.currentBot, enemyIndex = currentEnemy});
            //GameGUI.KillPlayerRobot(playerRoster.currentBot);
            // FindObjectOfType<GUIGame>().KillPlayerRobot(playerRoster.currentBot);
            playerRoster.currentBot++;
        }
        else
        {
            GameEventsManager.Instance.PostEvent(new GameEventPlayerRobotDamage() { eventName = GameEvents.FreeModePlayerRobotDamage, enemyIndex = currentEnemy, id = playerRoster.currentBot, damage = (int)enemyDamage });
            //GameGUI.DamagePlayerRobot(playerRoster.currentBot, (int)enemyDamage);
            // FindObjectOfType<GUIGame>().DamagePlayerRobot(playerRoster.currentBot, (int)enemyDamage);
        }
    }

    private void OnPlayerLost()
    {
        gameplayState = FreeToPlayGameplayState.GameEndMenu;
        GameEventsManager.Instance.PostEvent(new GameEventFreeModeLose() { eventName = GameEvents.FreeModeLose, score = (int)GetScore(), numBubblesWon = sessionData.GetTotalBubbles(),
            lastLevelPotentialBubbles = sessionData.GetPotentialBubbles() });
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
            // FindObjectOfType<GUIGame>().TargetEnemy(currentEnemy, false);
        }
        return allEnemiesKilled;
    }
    public void TargetEnemy(int index)
    {
        currentEnemy = index;
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
        return gameplayState == FreeToPlayGameplayState.Match3Playing &&
            match3Manager.GetGameplayState() == Match3GameplayManager.GameplayState.WaitForInput;
    }

    private void Update()
    {
        switch (gameplayState)
        {
            case FreeToPlayGameplayState.ShowingLevelText:
                StartCoroutine(ShowLevelText(2f, 0.5f));
                break;
            case FreeToPlayGameplayState.Match3Playing:
                match3Manager.UpdateMatch3Logic();
                break;
            case FreeToPlayGameplayState.LevelComplete:
                StartCoroutine(EndLevelSequence());
                match3Manager.UpdateMatch3Logic();
                break;
            case FreeToPlayGameplayState.EndLevelSequence:
                match3Manager.UpdateMatch3Logic();
                break;
        }


        if (Input.GetKeyDown(KeyCode.Space))
        {
            match3Manager.ExplodeAllSpecials();
        }
    }

    IEnumerator EndLevelSequence()
    {
        gameplayState = FreeToPlayGameplayState.EndLevelSequence;
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
        gameplayState = FreeToPlayGameplayState.LevelCompleteMenu;
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
        // FindObjectOfType<GUIGame>().TargetEnemy(currentEnemy);
        //GameGUI.TargetEnemy(currentEnemy);

        gameplayState = FreeToPlayGameplayState.Match3Playing;
    }

    private void OnBubbleExploded(int _posX, int _posY)
    {
        serverGameplayController?.UpdateGameplaySession((int)sessionData.GetScore(), true, (val) => { FindObjectOfType<GUIGame>().ExplodeBubble(_posX, _posY, val - sessionData.GetPotentialBubbles());});
        GameEventsManager.Instance.PostEvent(new GameEventBubbleExploded() { eventName = GameEvents.BubbleExploded, posX = _posX, posY = _posY });
        //FindObjectOfType<GUIGame>().ExplodeBubble(_posX, _posY, 0);
    }

    public void OnNewBubblesCount(int newValue)
    {
        int diff = newValue - sessionData.GetPotentialBubbles();
        sessionData.AddPotentialBubbles(diff);
        FindObjectOfType<GUIGame>().SetUnclaimedBubblesText(sessionData.GetPotentialBubbles());
    }

    public void SetCanSpawnBubbles(bool canSpawn)
    {
        match3Manager.SetCanSpawnBubbles(canSpawn);
        if (EnvironmentManager.Instance.IsRhym())
        {
            match3Manager.SetCanSpawnBubbles(false);
        }
    }

}