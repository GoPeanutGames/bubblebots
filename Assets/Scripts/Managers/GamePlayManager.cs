using CodeStage.AntiCheat.ObscuredTypes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BubbleBots.Data;
using BubbleBots.Match3.Data;
using BubbleBots.Match3.Controllers;
using BubbleBots.Match3.Models;
using BubbleBots.Modes;
using BubbleBots.Gameplay.Models;

public class GamePlayManager : MonoBehaviour
{
    private enum GameplayState
    {
        RobotSelection,
        WaitForInput,
        SwapFailed,
        SwapFailedPlaying,
        Swap,
        SwapPlaying,
        CheckExplosionsAfterSwap,
        ExplosionsInProgress,
        RefillBoard,
        RefillBoardInProgress,
        CheckForMatches,
        LevelComplete,
        LevelFailed,
        WaveComplete,
        Shuffle
    }

    private GameplayState gameplayState = GameplayState.RobotSelection;

    public Vector2Int swapStart;
    public Vector2Int swapEnd;

    public GameplayData gameplayData;
    public BoardController boardController;
    public MatchPrecedence matchPrecedence;

    public bool inputLocked = true;

    public ServerGameplayController serverGameplayController;
    public GUIMenu MenuGUI;
    public GUIGame GameGUI;
    public SkinManager skinManager;
    public float DamageOfRobot2 = 0.05f;
    public int HintDuration = 7;

    int releaseTileX = -1;
    int releaseTileY = -1;
    int currentLevelIndex = 0;
    int currentEnemy = 0;
    int combo = 0;

    ObscuredLong score = 0;

    private PlayerRoster playerRoster;
    private Wave currentWave;
    private int currentWaveIndex;
    private bool levelComplete;

    public bool InputLocked()
    {
        return runningCoroutinesByStringName.Count > 0 || runningCoroutinesByEnumerator.Count > 0;
    }
    public void PrepareLevel(int levelNumber)
    {
        MenuGUI.SwitchToMultiplayer(levelNumber);
    }

    public void StartLevel(LevelData levelData)
    {
        playerRoster.ResetRoster();
        levelComplete = false;
        currentWaveIndex = 0;
        currentWave = new Wave()
        {
            bots = new List<BubbleBot>()
            {
                new BubbleBot() { hp = 50 },
                new BubbleBot() { hp = 50 },
                new BubbleBot() { hp = 50 }
            },
            completed = false
        };
        ModeManager.Instance.SetMode(Mode.FREE);
        AnalyticsManager.Instance.SendPlayEvent(currentLevelIndex);
        serverGameplayController.StartGameplaySession(currentLevelIndex);

        boardController = new BoardController();
        boardController.Initialize(levelData, matchPrecedence);
        boardController.PopulateBoardWithSeed(UnityEngine.Random.Range(0, 1337));

        RenderStartLevel();

        gameplayState = GameplayState.WaitForInput;
        inputLocked = false;

        GameGUI.SetRobotGauges(currentWave.bots);
        GameGUI.SetPlayerRobots(playerRoster);
    }

    private void RenderStartLevel()
    {
        GameGUI.RenderLevelBackground(boardController.GetBoardModel().width, boardController.GetBoardModel().height);
        GameGUI.InitializeEnemyRobots();
        GameGUI.RenderTiles(boardController.GetBoardModel());
    }

    public void ResetGameplay()
    {
        currentLevelIndex = 0;
        currentWaveIndex = 0;
        currentEnemy = 0;
        levelComplete = false;
        score = 0;
        UserManager.Instance.SetPlayerScore((int)score);
        GameGUI.UpdateScore((int)score);
    }

    public void StartGamePlay()
    {
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

        StartLevel(gameplayData.levels[currentLevelIndex]);
        AnalyticsManager.Instance.SendPlayEvent(currentLevelIndex);
        return;
    }

    public void SetDownTile(int x, int y)
    {
        if (InputLocked())
        {
            return;
        }

        if (!GameGUI.CanSwapTiles)
        {
            return;
        }

        releaseTileX = x;
        releaseTileY = y;
    }

    private void OnWaveFinished()
    {
        //SoundManager.Instance.FadeOutMusic();
        currentWaveIndex++;
        currentWave.completed = true;

        if (currentWaveIndex >= gameplayData.levels[currentLevelIndex].waves)
        {
            OnLevelFinished();
        }
    }
    private void OnLevelFinished()
    {

        levelComplete = true;
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
        if (levelComplete || currentWave.completed)
        {
            return;
        }
        playerRoster.TakeDamage((int)DamageOfRobot2);
        if (playerRoster.AreAllBotsDead())
        {
            OnPlayerLost();
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
        GameGUI.DisplayLose();
        serverGameplayController.EndGameplaySession((int)GetScore());
    }

    private bool KillEnemy()
    {
        UserManager.RobotsKilled++;
        AnalyticsManager.Instance.SendRobotKillEvent(UserManager.RobotsKilled);
        serverGameplayController.UpdateGameplaySession((int)score);

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

    IEnumerator SwapTilesBackAndForthOnGUI(int x1, int y1, int x2, int y2)
    {
        GameGUI.SwapTiles(x1, y1, x2, y2, false);
        GameGUI.LockTiles("S0");
        yield return new WaitForSeconds(GameGUI.SwapDuration);
        GameGUI.SwapTiles(x2, y2, x1, y1, false);
        GameGUI.LockTiles("S1");
        yield return new WaitForSeconds(GameGUI.SwapDuration);

        //ReleaseTiles();
        //Debug.Log("Release: RXX");
        GameGUI.CanSwapTiles = true;

        ZeroReleasedTiles();
        gameplayState = GameplayState.WaitForInput;
    }

    IEnumerator SwapTilesOnceOnGUI(int x1, int y1, int x2, int y2)
    {
        GameGUI.SwapTiles(x1, y1, x2, y2, true);
        yield return new WaitForSeconds(GameGUI.SwapDuration);

        //ReleaseTiles();
        releaseTileX = -1;
        releaseTileY = -1;
        gameplayState = GameplayState.CheckExplosionsAfterSwap;
    }

    public void ZeroReleasedTiles()
    {
        releaseTileX = -1;
        releaseTileY = -1;
        GameGUI.CanSwapTiles = true;
        inputLocked = false;
    }

    public void ReleaseTiles(string releaseSource)
    {
        //Debug.Log("Release: " + releaseSource);
        StartTrackedCoroutine(ReleaseTilesNow());
    }

    IEnumerator ReleaseTilesNow()
    {
        yield return new WaitForSeconds(GameGUI.SwapDuration/* * 1.25f*/);

        GameGUI.CanSwapTiles = true;

        ZeroReleasedTiles();
    }
    public void SetEnemy(int currentEmeny)
    {
        this.currentEnemy = currentEmeny;
    }
    public long GetScore()
    {
        return score;
    }

    public int GetCurrentLevel()
    {
        return currentLevelIndex;
    }

    public long IncrementScore(int toAdd)
    {
        score += toAdd * 10; // hardcoded score multiplier
        serverGameplayController.UpdateGameplaySession((int)score);
        GameGUI.UpdateScore((int)score);
        return score;
    }


    private void StartNextWave()
    {
        currentWave.bots = new List<BubbleBot>()
        {
            new BubbleBot() { hp = 50 },
            new BubbleBot() { hp = 50 },
            new BubbleBot() { hp = 50 }
        };
        currentWave.completed = false;
        gameplayState = GameplayState.WaveComplete;
        GameGUI.StartNextWave();
    }

    private void PrepareNextLevel()
    {
        currentLevelIndex++;
        currentWave.completed = false;
        levelComplete = false;
        MenuGUI.gameObject.SetActive(true);
        MenuGUI.DisplayWin();
    }

    IEnumerator ShuffleBoard()
    {
        gameplayState = GameplayState.Shuffle;
        for (int i = 0; i < boardController.GetBoardModel().width; ++i)
            for (int j = 0; j < boardController.GetBoardModel().height; ++j)
            {
                GameGUI.ChangeColorScale(i, j, GetKeyFromId(boardController.GetBoardModel()[i][j].gem.GetId()));
            }

        yield return new WaitForSeconds(0.5f);
        gameplayState = GameplayState.CheckForMatches;
    }

    private void Update()
    {
        if (gameplayState == GameplayState.WaitForInput)
        {
            if (levelComplete)
            {
                UserManager.Instance.SetPlayerScore((int)score);
                serverGameplayController.EndGameplaySession((int)score);
                AnalyticsManager.Instance.SendLevelEvent();
                PrepareNextLevel();
                return;
            }

            if (currentWave.completed)
            {
                StartNextWave();
                return;
            }

            if (!boardController.HasPossibleMove())
            {
                boardController.GetBoardModel().Shuffle();
                StartTrackedCoroutine(ShuffleBoard());
            }
        }
        else if (gameplayState == GameplayState.SwapFailed)
        {
            StartTrackedCoroutine(SwapTilesBackAndForthOnGUI(swapStart.x, swapStart.y, swapEnd.x, swapEnd.y));
            gameplayState = GameplayState.SwapFailedPlaying;
        }
        else if (gameplayState == GameplayState.SwapFailedPlaying)
        {
            //wait for animation to end and go to GameplayState.WaitForInput
        }
        else if (gameplayState == GameplayState.Swap)
        {
            StartCoroutine(SwapTilesOnceOnGUI(swapStart.x, swapStart.y, swapEnd.x, swapEnd.y));
            gameplayState = GameplayState.SwapPlaying;
        }
        else if (gameplayState == GameplayState.SwapPlaying)
        {
            //wait for animation to end  and go to GameplayState.CheckExplosionsAfterSwap
        }
        else if (gameplayState == GameplayState.CheckExplosionsAfterSwap)
        {
            gameplayState = GameplayState.ExplosionsInProgress;

            NewSwapResult swapResult = boardController.NewSwapGems(swapStart.x, swapStart.y, swapEnd.x, swapEnd.y);
            StartTrackedCoroutine(ProcessSwapResult(swapResult));


        }
        else if (gameplayState == GameplayState.ExplosionsInProgress)
        {
            //wait for animations to end and go to GameplayState.RefillBoard
        }
        else if (gameplayState == GameplayState.RefillBoard)
        {
            List<GemMove> gemMoves = boardController.RefillBoard();
            StartTrackedCoroutine(RefillBoard(gemMoves));
            gameplayState = GameplayState.RefillBoardInProgress;
        }
        else if (gameplayState == GameplayState.RefillBoardInProgress)
        {
            //wait for animations to end and go to GameplayState.CheckForMatches
        }
        else if (gameplayState == GameplayState.CheckForMatches)
        {
            NewSwapResult swapResult = boardController.CheckForMatches();
            if (swapResult.explodeEvents != null && swapResult.explodeEvents.Count > 0)
            {
                StartTrackedCoroutine(ProcessSwapResult(swapResult));
                gameplayState = GameplayState.ExplosionsInProgress;
            }
            else
            {
                HitPlayer();
                gameplayState = GameplayState.WaitForInput;
                ZeroReleasedTiles();
            }
        }
    }

    /// track coroutines started on this object as they contain board processing algorithms
    /// input should be blocked as board processing algorithms are running
    /// patchwork, entire board logic and processing should be rewritten as it is very convoluted and does not have a clear order of execution

    private List<string> runningCoroutinesByStringName = new List<string>();
    private List<IEnumerator> runningCoroutinesByEnumerator = new List<IEnumerator>();
    public Coroutine StartTrackedCoroutine(string methodName)
    {
        return StartCoroutine(GenericRoutine(methodName, null));
    }
    public Coroutine StartTrackedCoroutine(IEnumerator coroutine)
    {
        return StartCoroutine(GenericRoutine(coroutine));
    }
    public Coroutine StartTrackedCoroutine(string methodName, object parameter)
    {
        return StartCoroutine(GenericRoutine(methodName, parameter));
    }
    public bool IsTrackedCoroutineRunning(string methodName)
    {
        return runningCoroutinesByStringName.Contains(methodName);
    }
    public bool IsTrackedCoroutineRunning(IEnumerator coroutine)
    {
        return runningCoroutinesByEnumerator.Contains(coroutine);
    }
    public void StopTrackedCoroutine(string methodName)
    {
        if (!runningCoroutinesByStringName.Contains(methodName))
        {
            return;
        }
        StopCoroutine(methodName);
        runningCoroutinesByStringName.Remove(methodName);
    }
    public void StopTrackedCoroutine(IEnumerator coroutine)
    {
        if (!runningCoroutinesByEnumerator.Contains(coroutine))
        {
            return;
        }
        StopCoroutine(coroutine);
        runningCoroutinesByEnumerator.Remove(coroutine);
    }
    private IEnumerator GenericRoutine(string methodName, object parameter)
    {
        runningCoroutinesByStringName.Add(methodName);
        if (parameter == null)
        {
            yield return StartCoroutine(methodName);
        }
        else
        {
            yield return StartCoroutine(methodName, parameter);
        }
        runningCoroutinesByStringName.Remove(methodName);
    }
    private IEnumerator GenericRoutine(IEnumerator coroutine)
    {
        runningCoroutinesByEnumerator.Add(coroutine);
        yield return StartCoroutine(coroutine);
        runningCoroutinesByEnumerator.Remove(coroutine);
    }


    //refactored code
    public void TouchTile(int x, int y)
    {
        if (InputLocked())
        {
            return;
        }
        releaseTileX = x;
        releaseTileY = y;
    }

    public void SwapTile(int x, int y)
    {
        if (InputLocked() || inputLocked)
        {
            return;
        }

        if (releaseTileX == -1 || releaseTileY == -1)
        {
            return;
        }

        if (!GameGUI.CanSwapTiles || (x == releaseTileX && y == releaseTileY))
        {
            //Debug.Log("L1-Q0 (" + (GameGUI.CanSwapTiles) + " | " + x + " | " + y + " | " + releaseTileX + " | " + releaseTileY + ")");
            return;
        }

        // eliminate diagonal
        if (x != releaseTileX && y != releaseTileY)
        {
            return;
        }

        combo = 0;

        if (Mathf.Abs(x - releaseTileX) <= 1 && Mathf.Abs(y - releaseTileY) <= 1)
        {
            inputLocked = true;

            if (!boardController.CanSwap(x, y, releaseTileX, releaseTileY))
            {
                swapStart = new Vector2Int(x, y);
                swapEnd = new Vector2Int(releaseTileX, releaseTileY);
                gameplayState = GameplayState.SwapFailed;

            }
            else
            {
                swapStart = new Vector2Int(x, y);
                swapEnd = new Vector2Int(releaseTileX, releaseTileY);
                gameplayState = GameplayState.Swap;
            }
        }


    }

    IEnumerator RefillBoard(List<GemMove> gemMoves)
    {
        for (int i = 0; i < gemMoves.Count; ++i)
        {
            if (gemMoves[i].From.y >= boardController.GetBoardModel().height)
            {
                //GameGUI.AppearAt(gemMoves[i].To.x, gemMoves[i].To.y, boardController.GetBoardModel()[gemMoves[i].To.x][gemMoves[i].To.y].gem.GetId().ToString(), boardController.GetBoardModel().width, boardController.GetBoardModel().height, GameGUI.SwapDuration);
            }
            else
            {
                GameGUI.ScrollTileDown(gemMoves[i].From.x, gemMoves[i].From.y, gemMoves[i].From.y - gemMoves[i].To.y, GameGUI.SwapDuration);
            }
        }
        yield return new WaitForSeconds(GameGUI.SwapDuration);
        for (int i = 0; i < gemMoves.Count; ++i)
        {
            if (gemMoves[i].From.y >= boardController.GetBoardModel().height)
            {
                GameGUI.AppearAt(gemMoves[i].To.x, gemMoves[i].To.y, GetKeyFromId(boardController.GetBoardModel()[gemMoves[i].To.x][gemMoves[i].To.y].gem.GetId()), boardController.GetBoardModel().width, boardController.GetBoardModel().height, GameGUI.SwapDuration / 2);
            }
            else
            {
                //GameGUI.ScrollTileDown(gemMoves[i].From.x, gemMoves[i].From.y, gemMoves[i].From.y - gemMoves[i].To.y, GameGUI.SwapDuration);
            }
        }
        yield return new WaitForSeconds(GameGUI.SwapDuration);
        gameplayState = GameplayState.CheckForMatches;
        GameGUI.CanSwapTiles = true;
    }

    IEnumerator ProcessSwapResult(NewSwapResult swapResult)
    {
        for (int i = 0; swapResult.explodeEvents != null && i < swapResult.explodeEvents.Count; ++i)
        {
            if (swapResult.explodeEvents[i].toExplode == null ||
                swapResult.explodeEvents[i].toExplode.Count == 0)
            {
                continue;
            }

            //play vfx
            LineBlastExplodeEvent lineBlastExplodeEvent = swapResult.explodeEvents[i] as LineBlastExplodeEvent;
            if (lineBlastExplodeEvent != null)
            {
                SoundManager.Instance.PlayLightningSfx();
                GameGUI.LineDestroyEffect(lineBlastExplodeEvent.lineBlastStartPosition.x, lineBlastExplodeEvent.lineBlastStartPosition.y, false);
                yield return new WaitForSeconds(0.2f);
            }
            ColumnBlastEvent columnBlasEvent = swapResult.explodeEvents[i] as ColumnBlastEvent;
            if (columnBlasEvent != null)
            {
                SoundManager.Instance.PlayLightningSfx();
                GameGUI.LineDestroyEffect(columnBlasEvent.columnBlastStartPosition.x, columnBlasEvent.columnBlastStartPosition.y, true);
                yield return new WaitForSeconds(0.2f);
            }
            ColorBlastEvent colorBlastEvent = swapResult.explodeEvents[i] as ColorBlastEvent;
            if (colorBlastEvent != null)
            {
                //GameGUI.ColorBlastEffect(colorBlastEvent.colorBlastPosition.x, colorBlastEvent.colorBlastPosition.y);
                for (int j = 0; j < swapResult.explodeEvents[i].toExplode.Count; ++j)
                {
                    GameGUI.ColorBombEffect(swapResult.explodeEvents[i].toExplode[j].x, swapResult.explodeEvents[i].toExplode[j].y);
                }
                SoundManager.Instance.PlayColorSfx();
                yield return new WaitForSeconds(1.1f);
            }
            //BoardBlastEvent boardBlastEvent = swapResult.explodeEvents[i] as BoardBlastEvent;
            //if (boardBlastEvent != null)
            //{
            //    for (int j = 0; j < 6; ++j)
            //    {
            //        GameGUI.ColorBlastEffect(UnityEngine.Random.Range(0, boardController.GetBoardModel().width), UnityEngine.Random.Range(0, boardController.GetBoardModel().height));
            //        SoundManager.Instance.PlayColorSfx();
            //        yield return new WaitForSeconds(0.1f);
            //    }
            //    //yield return new WaitForSeconds(0.4f);
            //}

            HammerBlastEvent hammerBlastEvent = swapResult.explodeEvents[i] as HammerBlastEvent;
            if (hammerBlastEvent != null)
            {
                SoundManager.Instance.PlayHammerSfx();
            }

            BombBlastEvent bombBlastEvent = swapResult.explodeEvents[i] as BombBlastEvent;
            if (bombBlastEvent != null)
            {
                SoundManager.Instance.PlayBombSfx();
            }

            combo += 1;
            SoundManager.Instance.PlayComboSfx(combo);
            for (int j = 0; j < swapResult.explodeEvents[i].toExplode.Count; ++j)
            {
                GameGUI.ExplodeTile(swapResult.explodeEvents[i].toExplode[j].x, swapResult.explodeEvents[i].toExplode[j].y, false);
            }
            HitEnemy(swapResult.explodeEvents[i].toExplode.Count);
            IncrementScore(swapResult.explodeEvents[i].toExplode.Count);

        }
        yield return new WaitForSeconds(GameGUI.SwapDuration / 2);
        bool createdGems = false;
        for (int i = 0; swapResult.explodeEvents != null && i < swapResult.explodeEvents.Count; ++i)
        {
            if (swapResult.explodeEvents[i].toCreate == null ||
                swapResult.explodeEvents[i].toCreate.Count == 0)
            {
                continue;
            }

            for (int j = 0; j < swapResult.explodeEvents[i].toCreate.Count; ++j)
            {
                if (swapResult.explodeEvents[i].toCreate[j] == null)
                {
                    continue;
                }
                GameGUI.AppearAt(swapResult.explodeEvents[i].toCreate[j].At.x,
                    swapResult.explodeEvents[i].toCreate[j].At.y,
                    skinManager.Skins[skinManager.SelectedSkin].TileSet[swapResult.explodeEvents[i].toCreate[j].Id].Key,
                    boardController.GetBoardModel().width,
                    boardController.GetBoardModel().height, GameGUI.SwapDuration / 2);
                createdGems = true;
            }
        }
        if (createdGems)
        {
            yield return new WaitForSeconds(GameGUI.SwapDuration / 2);
        }
        for (int i = 0; i < swapResult.explodeEvents.Count; ++i)
        {
            ColorChangeEvent colorChangeEvent = swapResult.explodeEvents[i] as ColorChangeEvent;
            if (colorChangeEvent != null)
            {
                GameGUI.ColorChangeEffect(GetKeyFromId(colorChangeEvent.targetColor), colorChangeEvent.toChange);
                yield return new WaitForSeconds(1f);
            }
        }
        gameplayState = GameplayState.RefillBoard;
    }

    private string GetKeyFromId(int id) // strange naming convention
    {
        if (id == 9)
        {
            return "S1";
        }
        else if (id == 12)
        {
            return "S4";
        }
        else if (id == 13)
        {
            return "S5";
        }
        else if (id == 10)
        {
            return "S2";
        }
        else if (id == 11)
        {
            return "S3";
        }

        return id.ToString();
    }

}