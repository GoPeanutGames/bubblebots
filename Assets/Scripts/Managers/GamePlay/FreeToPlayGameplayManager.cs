using BubbleBots.Data;
using BubbleBots.Gameplay.Models;
using CodeStage.AntiCheat.ObscuredTypes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeToPlaySessionData
{
    ObscuredLong score = 0;

    private int scoreMultiplier = 10;// hardcoded score multiplier

    private int robotsKilled = 0;

    private int potentialBubbles = 0;

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

public class FreeToPlayGameplayManager : MonoBehaviour
{
    private enum FreeToPlayGameplayState
    {
        RobotSelection,
        ShowingLevelText,
        Match3Playing,
        LevelComplete,
        LevelCompleteMenu,
    }

    private FreeToPlayGameplayState gameplayState;

    public FreeToPlayGameplayData gameplayData;

    public ServerGameplayController serverGameplayController;
    public GUIMenu MenuGUI;
    public GUIGame GameGUI;
    public GUIRobotSelection RobotSelectionGUI;
    public float DamageOfRobot2 = 0.05f;

    int currentLevelIndex = 0;
    int currentEnemy = 0;

    private FreeToPlaySessionData sessionData;

    private PlayerRoster playerRoster;
    private Wave currentWave;
    private int currentWaveIndex;

    public Match3GameplayManager match3Manager;

    public void Start()
    {
        gameplayState = FreeToPlayGameplayState.RobotSelection;
        ShowRobotSelectionMenu();
    }

    private void ShowRobotSelectionMenu()
    {
        SoundManager.Instance?.FadeOutMusic(() =>
        {
            SoundManager.Instance.PlayRobotSelectMusicNew();
            SoundManager.Instance.FadeInMusic();
        });

        RobotSelectionGUI.gameObject.SetActive(true);
        MenuGUI.gameObject.SetActive(false);
        GameGUI.gameObject.SetActive(false);
    }

    public void StartSession()
    {
        sessionData = new FreeToPlaySessionData();
        playerRoster = new PlayerRoster()
        {
            bots = new List<BubbleBot>()
            {
                new BubbleBot { maxHp = 30, hp = 30, id = 1 },
                new BubbleBot { maxHp = 30, hp = 30, id = 2 },
                new BubbleBot { maxHp = 30, hp = 30, id = 3 },
            },
            currentBot = 0
        };
        currentLevelIndex = 0;

        match3Manager.Initialize(gameplayData.levels[currentLevelIndex]);

        match3Manager.onGemsExploded -= OnGemsExploded;
        match3Manager.onGemsExploded += OnGemsExploded;

        match3Manager.onBoardEventsEnded -= HitPlayer;
        match3Manager.onBoardEventsEnded += HitPlayer;

        match3Manager.onBubbleExploded -= OnBubbleExploded;
        match3Manager.onBubbleExploded += OnBubbleExploded;

        GameGUI.onEnemyChanged -= SetEnemy;
        GameGUI.onEnemyChanged += SetEnemy;

        MenuGUI.ResetScores();
        StartLevel();
    }

    public void StartLevel()
    {
        if (currentLevelIndex < gameplayData.stopHpRefreshAfterLevel)
        {
            playerRoster.ResetRoster();
        }

        match3Manager.Initialize(gameplayData.levels[Mathf.Min(gameplayData.levels.Count - 1, currentLevelIndex)]);

        currentWaveIndex = 0;
        currentEnemy = 0;
        currentWave = new Wave()
        {
            bots = new List<BubbleBot>()
            {
                new BubbleBot() { hp = 55 },
                new BubbleBot() { hp = 55 },
                new BubbleBot() { hp = 55 }
            },
            completed = false
        };

        GameGUI.gameObject.SetActive(true);
        MenuGUI.gameObject.SetActive(false);
        
        AnalyticsManager.Instance?.SendPlayEvent(currentLevelIndex);
        serverGameplayController?.StartGameplaySession(currentLevelIndex);

        GameGUI.SetRobotGauges(currentWave.bots);
        GameGUI.SetPlayerRobots(playerRoster);


        gameplayState = FreeToPlayGameplayState.ShowingLevelText;
    }

    public void StartNextLevel()
    {
        currentLevelIndex++;
        MenuGUI.HideWin();
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
        GameGUI.DamageToEnemyRobot(currentWave.bots[currentEnemy].hp);

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
        playerRoster.TakeDamage((int)DamageOfRobot2);

        if (playerRoster.AreAllBotsDead())
        {
            OnPlayerLost();
            GameGUI.KillPlayerRobot(playerRoster.currentBot);
        }
        else if (playerRoster.IsDead(playerRoster.currentBot))
        {
            GameGUI.KillPlayerRobot(playerRoster.currentBot);
            playerRoster.currentBot++;
        }
        else
        {
            GameGUI.DamagePlayerRobot(playerRoster.currentBot, (int)DamageOfRobot2);
        }
    }

    private void OnPlayerLost()
    {
        GameGUI.DisplayLose((int)GetScore());
        serverGameplayController?.EndGameplaySession((int)GetScore());
    }

    private bool KillEnemy()
    {
        sessionData.IncrementRobotsKilled(1);
        UserManager.RobotsKilled++;
        AnalyticsManager.Instance?.SendRobotKillEvent(UserManager.RobotsKilled);
        serverGameplayController?.UpdateGameplaySession((int)GetScore());

        GameGUI.KillEnemy();
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
            GameGUI.TargetEnemy(currentEnemy, false);
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
        GameGUI.UpdateScore((int)sessionData.GetScore());
    }

    private void Update()
    {
        switch (gameplayState)
        {
            case FreeToPlayGameplayState.ShowingLevelText:
                StartCoroutine(ShowLevelText(1f, 0.5f));
                break;
            case FreeToPlayGameplayState.Match3Playing:
                match3Manager.UpdateMatch3Logic();
                break;
            case FreeToPlayGameplayState.LevelComplete:
                StartCoroutine(EndLevelSequence());
                match3Manager.UpdateMatch3Logic();
                break;
        }
    }

    IEnumerator EndLevelSequence()
    {
        match3Manager.ExplodeAllSpecials();
        yield return new WaitUntil(() => match3Manager.GetGameplayState() == Match3GameplayManager.GameplayState.WaitForInput);
        gameplayState = FreeToPlayGameplayState.LevelCompleteMenu;

        UserManager.Instance.SetPlayerScore((int)GetScore());
        serverGameplayController.EndGameplaySession((int)GetScore());
        AnalyticsManager.Instance.SendLevelEvent((int)GetScore());

        UserManager.Instance?.AddBubbles(sessionData.GetPotentialBubbles());


        MenuGUI.DisplayWin();
    }

    IEnumerator ShowLevelText(float duration, float fadeDuration)
    {
        GameGUI.ShowLevelText(currentLevelIndex, duration, fadeDuration);
        yield return new WaitForSeconds(duration);

        currentEnemy = 0;
        GameGUI.TargetEnemy(currentEnemy);

        gameplayState = FreeToPlayGameplayState.Match3Playing;
    }

    private void OnBubbleExploded(int posX, int posY)
    {
        int reward = StubGetBubblesValue();
        sessionData.AddPotentialBubbles(reward);
        GameGUI.ExplodeBubble(posX, posY, reward);
        GameGUI.SetUnclaimedBubblesText(sessionData.GetPotentialBubbles());
    }

    private int StubGetBubblesValue()
    {
        int firstRoll = Random.Range(0, 100);

        if (firstRoll < 50)
        {
            return Random.Range(1, 21);
        } 
        else if (firstRoll < 80)
        {
            return Random.Range(21, 101);
        } 
        else if (firstRoll < 95)
        {
            return Random.Range(101, 300);
        }
        else if (firstRoll < 99)
        {
            return Random.Range(301, 450);
        } 
        else
        {
            return Random.Range(451, 501);
        }
    }
}