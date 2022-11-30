using CodeStage.AntiCheat.ObscuredTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static LevelManager;


using BubbleBots.Data;
using BubbleBots.Match3.Data;
using BubbleBots.Match3.Controllers;
using BubbleBots.Match3.Models;
using BubbleBots.Modes;

public class GamePlayManager : MonoBehaviour
{

    private enum GameplayState
    {
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
    }

    private GameplayState gameplayState;

    public Vector2Int swapStart;
    public Vector2Int swapEnd;

    public GameplayData gameplayData;
    public BoardController boardController;
    public MatchPrecedence matchPrecedence;

    public bool inputLocked = true;

    public ServerGameplayController serverGameplayController;
    public LevelManager levelManager;
    public GUIMenu MenuGUI;
    public GUIGame GameGUI;
    public SkinManager skinManager;
    public float DamageOfRobot1 = 0.05f;
    public float DamageOfRobot2 = 0.05f;
    public int HintDuration = 7;
    public int[] EnemyHPs = new int[] { 40, 40, 40 };
    int[] numHit = new int[] { 0, 0, 0 };

    enum SpecailShapes { Nothing, LongT, L, T, SmallSquare, Straight5, Straight4 }
    List<SlideInformation> tilesToSlide = new List<SlideInformation>();
    LevelInformation levelInfo;
    string[,] tileSet;
    List<Vector2> tilesToPut = new List<Vector2>();
    List<int> availabletiles = new List<int>();
    int releaseTileX = -1;
    int releaseTileY = -1;
    bool enemyDead = false;
    int numLevel = 1;
    int currentLevel = 0;
    int currentEnemy = 0;
    int killedEnemies = 0;
    int currentWave = 1;
    int maxEnemies = 3;
    int combo = 0;

    ObscuredLong score = 0;
    bool levelEnded = false;
    bool canAttack = false;

    float timeForNewHint = 0;

    public string[,] TileSet
    {
        get
        {
            return tileSet;
        }
    }

    public bool InputLocked()
    {
        return runningCoroutinesByStringName.Count > 0 || runningCoroutinesByEnumerator.Count > 0;
    }
    public void PrepareLevel(string levelFile, int levelNumber)
    {
        MenuGUI.SwitchToMultiplayer(levelFile, levelNumber);
    }

    public void StartLevel(LevelData levelData)
    {
        ModeManager.Instance.SetMode(Mode.FREE);
        AnalyticsManager.Instance.SendPlayEvent(currentLevel);
        serverGameplayController.StartGameplaySession(currentLevel);


        //LevelInformation levelInfo;
        //currentLevel = levelNumber;
        //currentEnemy = 0;
        //GameGUI.SetCurrentPlayer(0);
        //killedEnemies = 0;
        //currentWave = 1;
        //levelEnded = false;

        //try
        //{
        //    enemyDead = false;
        //    levelInfo = levelManager.LoadLevel(levelFile);
        //}
        //catch (Exception ex)
        //{
        //    Debug.LogError(ex.Message);
        //    return;
        //}

        //RenderLevel(levelInfo);
        //for (int g = 0; g < GameGUI.PlayerGauges.Length; g++)
        //{
        //    GameGUI.PlayerGauges[g].value = GameGUI.PlayerGauges[g].maxValue;
        //    GameGUI.PlayerGauges[g].transform.Find("TxtHP").GetComponent<TextMeshProUGUI>().text = GameGUI.PlayerGauges[g].maxValue.ToString("N0") + " / " + GameGUI.PlayerGauges[g].maxValue.ToString("N0");
        //}

        //numHit = new int[] { 0, 0, 0 };
        ////timeForNewHint = DateTime.Now + new TimeSpan(0, 0, 7);
        //StartHintingCountDown();


        boardController = new BoardController();
        boardController.Initialize(levelData, matchPrecedence);
        boardController.PopulateBoardWithSeed(1337);

        RenderStartLevel();

        gameplayState = GameplayState.WaitForInput;
        inputLocked = false;
    }


    private void RenderStartLevel()
    {
        //this.levelInfo = levelInfo;

        List<String> availableTiles = new List<string>();
        for (int i = 0; i < gameplayData.levels[currentLevel].gemSet.Count; ++i)
        {
            availableTiles.Add(gameplayData.levels[currentLevel].gemSet[i].ToString());
        }
        LevelInformation levelInforFromLevelData = new LevelInformation(
            1,
            boardController.GetBoardModel().width,
            boardController.GetBoardModel().height,
            availableTiles.ToArray(),
            gameplayData.levels[currentLevel].waves
            );
        GameGUI.RenderLevelBackground(levelInforFromLevelData);
        //GameGUI.InitializeEnemyRobots();
        GameGUI.RenderTiles(boardController.GetBoardModel());
    }

    public void StartLevel(string levelFile, int levelNumber)
    {
        StartLevel(gameplayData.levels[currentLevel]);

        return;
        AnalyticsManager.Instance.SendPlayEvent(levelNumber);
        //LeaderboardManager.Instance.ResetKilledRobots();
        LevelInformation levelInfo;
        currentLevel = levelNumber;
        currentEnemy = 0;
        GameGUI.SetCurrentPlayer(0);
        killedEnemies = 0;
        currentWave = 1;
        levelEnded = false;

        try
        {
            enemyDead = false;
            levelInfo = levelManager.LoadLevel(levelFile);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
            return;
        }

//        RenderLevel(levelInfo);
        for (int g = 0; g < GameGUI.PlayerGauges.Length; g++)
        {
            GameGUI.PlayerGauges[g].value = GameGUI.PlayerGauges[g].maxValue;
            GameGUI.PlayerGauges[g].transform.Find("TxtHP").GetComponent<TextMeshProUGUI>().text = GameGUI.PlayerGauges[g].maxValue.ToString("N0") + " / " + GameGUI.PlayerGauges[g].maxValue.ToString("N0");
        }

        numHit = new int[] { 0, 0, 0 };
        //timeForNewHint = DateTime.Now + new TimeSpan(0, 0, 7);
        StartHintingCountDown();
    }

   

    private bool IsSpecialGem(string v)
    {
        return (v == "S1" || v == "S2" || v == "S3" || v == "S4" || v == "S5");
    }
    private void SwapKeys(int x1, int y1, int x2, int y2)
    {
        string temp = tileSet[x1, y1];
        tileSet[x1, y1] = tileSet[x2, y2];
        tileSet[x2, y2] = temp;
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

    private void HitEnemy()
    {
        if (levelEnded)
        {
            return;
        }

        GameGUI.DamageToEnemyRobot(tilesToPut.Count);
        numHit[currentEnemy] += tilesToPut.Count;

        if (numHit[currentEnemy] >= EnemyHPs[currentEnemy])
        {
            KillEnemy();

            if (++killedEnemies >= maxEnemies)
            {
                if (++currentWave > levelInfo.Waves)
                {
                    levelEnded = true;
                    FindObjectOfType<SoundManager>().FadeOutStartMusic();
                    StartTrackedCoroutine(FinishLevel());
                    //LeaderboardManager.Instance.Score = score;
                }
                else
                {
                    killedEnemies = 0;
                    numHit = new int[] { 0, 0, 0 };

                    GameGUI.StartNextWave();
                }
            }
        }
        else
        {
            HitPlayer();
            //if (!MoreMovesArePossible())
            {
                //StartTrackedCoroutine(RefreshBoard());
            }
        }
    }

    private void HitPlayer()
    {
        if (!canAttack)
        {
            return;
        }

        canAttack = false;
        GameGUI.DamageToPlayerRobot(DamageOfRobot2);
    }

    private void KillEnemy()
    {
        if (enemyDead)
        {
            return;
        }

        GameGUI.KillEnemy();
        //currentEnemy = (currentEnemy + 1) % maxEnemies;
        for (int i = maxEnemies - 1; i >= 0; i--)
        {
            if (numHit[i] < EnemyHPs[currentEnemy])
            {
                currentEnemy = i;
                break;
            }
        }

        GameGUI.TargetEnemy(currentEnemy, false);
    }

    IEnumerator FinishLevel()
    {
        serverGameplayController.EndGameplaySession((int)score);
        AnalyticsManager.Instance.SendLevelEvent();
        yield return new WaitForSeconds(GameGUI.SwapDuration);

        enemyDead = true;
        ResetHintTime();
        GameGUI.LockTiles("L3");
        numLevel += 1;

        MenuGUI.gameObject.SetActive(true);
        MenuGUI.DisplayWin();
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
        StartHintingCountDown();
    }

    public void StartHintingCountDown()
    {
        timeForNewHint = Time.time + HintDuration;
    }

    private void Start()
    {
        GameGUI.SetRobotGauges(EnemyHPs);
        GameGUI.SetPlayerGauges();
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
        return currentLevel;
    }

    public int GetNumLevel()
    {
        return numLevel;
    }

    public long IncrementScore(int score)
    {
        serverGameplayController.UpdateGameplaySession((int)this.score);

        this.score += score;

        return this.score;
    }

    private void Update()
    {

        if (gameplayState == GameplayState.WaitForInput)
        {


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
            //SwapResult swapResult = boardController.SwapGems(swapStart.x, swapStart.y, swapEnd.x, swapEnd.y);
            //StartTrackedCoroutine(ProcessSwapResult(swapResult));
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
                gameplayState = GameplayState.WaitForInput;
            }
        }
    }

    public void ResetHintTime()
    {
        timeForNewHint = 0;
    }

    public float GetTimeForNewHint()
    {
        return timeForNewHint;
    }

    public bool GetEnemyDead()
    {
        return enemyDead;
    }

    public void EndLevel()
    {
        levelEnded = true;
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
        canAttack = true;

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
                GameGUI.AppearAt(gemMoves[i].To.x, gemMoves[i].To.y, boardController.GetBoardModel()[gemMoves[i].To.x][gemMoves[i].To.y].gem.GetId().ToString());
            }
            else
            {
                GameGUI.ScrollTileDown(gemMoves[i].From.x, gemMoves[i].From.y, gemMoves[i].From.y - gemMoves[i].To.y);
            }
        }
        yield return new WaitForSeconds(2 * GameGUI.SwapDuration);
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
                FindObjectOfType<SoundManager>().PlayLightningSound();
                GameGUI.LineDestroyEffect(lineBlastExplodeEvent.lineBlastStartPosition.x, lineBlastExplodeEvent.lineBlastStartPosition.y, false);
                yield return new WaitForSeconds(0.4f);
            }
            ColumnBlastEvent columnBlasEvent = swapResult.explodeEvents[i] as ColumnBlastEvent;
            if (columnBlasEvent != null)
            {
                FindObjectOfType<SoundManager>().PlayLightningSound();
                GameGUI.LineDestroyEffect(columnBlasEvent.columnBlastStartPosition.x, columnBlasEvent.columnBlastStartPosition.y, true);
                yield return new WaitForSeconds(0.4f);
            }
            ColorBlastEvent colorBlastEvent = swapResult.explodeEvents[i] as ColorBlastEvent;
            if (colorBlastEvent != null)
            {
                //GameGUI.ColorBlastEffect(colorBlastEvent.colorBlastPosition.x, colorBlastEvent.colorBlastPosition.y);
                for (int j = 0; j < swapResult.explodeEvents[i].toExplode.Count; ++j)
                {
                    GameGUI.ColorBombEffect(swapResult.explodeEvents[i].toExplode[j].x, swapResult.explodeEvents[i].toExplode[j].y);
                }
                FindObjectOfType<SoundManager>().PlayColorSound();
                yield return new WaitForSeconds(1.1f);
            }
            BoardBlastEvent boardBlastEvent = swapResult.explodeEvents[i] as BoardBlastEvent;
            if (boardBlastEvent != null)
            {
                for (int j = 0; j < 6; ++j)
                {
                    GameGUI.ColorBlastEffect(UnityEngine.Random.Range(0, boardController.GetBoardModel().width), UnityEngine.Random.Range(0, boardController.GetBoardModel().height));
                    FindObjectOfType<SoundManager>().PlayColorSound();
                    yield return new WaitForSeconds(0.1f);
                }
                //yield return new WaitForSeconds(0.4f);
            }

            combo += 1;
            FindObjectOfType<SoundManager>().PlayComboSound(combo);
            for (int j = 0; j < swapResult.explodeEvents[i].toExplode.Count; ++j)
            {
                GameGUI.ExplodeTile(swapResult.explodeEvents[i].toExplode[j].x, swapResult.explodeEvents[i].toExplode[j].y, false);
            }


        }
        yield return new WaitForSeconds(GameGUI.SwapDuration);
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
                    skinManager.Skins[skinManager.SelectedSkin].TileSet[swapResult.explodeEvents[i].toCreate[j].Id].Key);
                createdGems = true;
            }
        }
        if (createdGems)
        {
            yield return new WaitForSeconds(GameGUI.SwapDuration);
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