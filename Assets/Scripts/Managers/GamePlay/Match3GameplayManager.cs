using BubbleBots.Data;
using BubbleBots.Match3.Controllers;
using BubbleBots.Match3.Data;
using BubbleBots.Match3.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMatch3Events
{
    public delegate void OnGemsExploded(int count);
    public event OnGemsExploded onGemsExploded;

    public delegate void OnBoardEventsEnded();
    public event OnBoardEventsEnded onBoardEventsEnded;

    public delegate void OnBubbleExploded(int posX, int posY);
    public event OnBubbleExploded onBubbleExploded;

}

public class Match3GameplayManager : MonoBehaviour, IMatch3Events
{
    public enum GameplayState
    {
        RobotSelection,
        WaitForInput,
        SwapFailed,
        SwapFailedNotAllowed,
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

    private LevelData levelData;

    private Vector2Int swapStart;
    private Vector2Int swapEnd;

    public GameplayData gameplayData;
    public BoardController boardController;
    public MatchPrecedence matchPrecedence;

    public bool inputLocked = true;

    int releaseTileX = -1;
    int releaseTileY = -1;

    int combo = 0;

    public LevelDesign testLevel;

    public GUIMenu MenuGUI;
    public GUIGame GameGUI;


    private bool specialSpecialMatch = false; 
    private bool canSpawnBubbles = true;

    public event IMatch3Events.OnGemsExploded onGemsExploded;
    public event IMatch3Events.OnBoardEventsEnded onBoardEventsEnded;
    public event IMatch3Events.OnBubbleExploded onBubbleExploded;

    public Match3GameplayManager.GameplayState GetGameplayState()
    {
        return gameplayState;
    }

    public bool InputLocked()
    {
        return runningCoroutinesByStringName.Count > 0 || runningCoroutinesByEnumerator.Count > 0 || gameplayState != GameplayState.WaitForInput;
    }

    public void Initialize(LevelData _levelData, bool _canSpawnBubbles)
    {
        //todo fix this 
        GameGUI = FindObjectOfType<GUIGame>();

        levelData = _levelData;
        boardController = new BoardController();
        boardController.Initialize(levelData, matchPrecedence);
        boardController.PopulateBoardWithSeed(Random.Range(0, 1337));
        RenderStartLevel();
        gameplayState = GameplayState.WaitForInput;
        inputLocked = false;
        canSpawnBubbles = _canSpawnBubbles;
    }

    public void Initialize(LevelData _levelData)
    {
        //todo fix this 
        GameGUI = FindObjectOfType<GUIGame>();

        levelData = _levelData;
        boardController = new BoardController();
        boardController.Initialize(levelData, matchPrecedence);
        boardController.PopulateBoardWithSeed(Random.Range(0, 1337));
        RenderStartLevel();
        gameplayState = GameplayState.WaitForInput;
        inputLocked = false;
    }

    private void RenderStartLevel()
    {
        GameGUI.RenderLevelBackground(boardController.GetBoardModel().width, boardController.GetBoardModel().height);
        GameGUI.InitializeEnemyRobots();
        GameGUI.RenderTiles(boardController.GetBoardModel(), levelData);
    }

    public void ExplodeAllSpecials()
    {
        inputLocked = true;
        //to do // implement
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

    IEnumerator SwapTilesNotAllowedOnGUI(int x1, int y1, int x2, int y2)
    {
        GameGUI.SwapTilesFail(x1, y1, x2, y2, false);
        yield return new WaitForSeconds(GameGUI.SwapDuration);
        yield return new WaitForSeconds(GameGUI.SwapDuration);
        GameGUI.CanSwapTiles = true;
        ZeroReleasedTiles();
        gameplayState = GameplayState.WaitForInput;
    }

    IEnumerator SwapTilesOnceOnGUI(int x1, int y1, int x2, int y2)
    {
        GameGUI.SwapTiles(x1, y1, x2, y2, true);
        yield return new WaitForSeconds(GameGUI.SwapDuration);


        if (boardController.GetBoardModel()[x1][y1].gem.IsSpecial() &&
            boardController.GetBoardModel()[x2][y2].gem.IsSpecial())
        {
            GameGUI.ColorBombEffect(x1, y1);
            GameGUI.ColorBombEffect(x2, y2);
            specialSpecialMatch = true;
            yield return new WaitForSeconds(1.1f);
        }


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
    IEnumerator ShuffleBoard()
    {
        gameplayState = GameplayState.Shuffle;
        for (int i = 0; i < boardController.GetBoardModel().width; ++i)
            for (int j = 0; j < boardController.GetBoardModel().height; ++j)
            {
                GameGUI.ChangeColorScale(i, j, boardController.GetBoardModel()[i][j].gem.GetId(), levelData);
            }

        yield return new WaitForSeconds(0.5f);
        gameplayState = GameplayState.CheckForMatches;
    }

    public void UpdateMatch3Logic()
    {
        if (gameplayState == GameplayState.WaitForInput)
        {
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
        else if (gameplayState == GameplayState.SwapFailedNotAllowed)
        {
            StartTrackedCoroutine(SwapTilesNotAllowedOnGUI(swapStart.x, swapStart.y, swapEnd.x, swapEnd.y));
            gameplayState = GameplayState.SwapFailedPlaying;
        }
        else if (gameplayState == GameplayState.SwapFailedPlaying)
        {
            //wait for animation to end and go to GameplayState.WaitForInput
        }
        else if (gameplayState == GameplayState.Swap)
        {
            StartTrackedCoroutine(SwapTilesOnceOnGUI(swapStart.x, swapStart.y, swapEnd.x, swapEnd.y));
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
            List<GemMove> gemMoves = boardController.RefillBoard(canSpawnBubbles);
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
                onBoardEventsEnded?.Invoke();
                gameplayState = GameplayState.WaitForInput;
                ZeroReleasedTiles();
            }
        }
    }

    /// track coroutines started on this object as they contain board processing algorithms
    /// input should be blocked as board processing algorithms are running

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

            if (!boardController.IsSwapAllowed(x, y, releaseTileX, releaseTileY))
            {
                swapStart = new Vector2Int(x, y);
                swapEnd = new Vector2Int(releaseTileX, releaseTileY);
                gameplayState = GameplayState.SwapFailedNotAllowed;
            }
            else if (!boardController.CanSwap(x, y, releaseTileX, releaseTileY))
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
                GameGUI.AppearAt(gemMoves[i].To.x, gemMoves[i].To.y, boardController.GetBoardModel()[gemMoves[i].To.x][gemMoves[i].To.y].gem.GetId(), boardController.GetBoardModel().width, boardController.GetBoardModel().height, GameGUI.SwapDuration / 2, levelData);
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
        if (specialSpecialMatch)
        {
            GameGUI.SwapDuration = GameGUI.SpecialSwapDuration;
        }


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
                SoundManager.Instance?.PlayLightningSfx();
                GameGUI.LineDestroyEffect(lineBlastExplodeEvent.lineBlastStartPosition.x, lineBlastExplodeEvent.lineBlastStartPosition.y, false);
                yield return new WaitForSeconds(0.2f);
            }
            ColumnBlastEvent columnBlasEvent = swapResult.explodeEvents[i] as ColumnBlastEvent;
            if (columnBlasEvent != null)
            {
                SoundManager.Instance?.PlayLightningSfx();
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
                SoundManager.Instance?.PlayColorSfx();
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
                SoundManager.Instance?.PlayHammerSfx();
            }

            BombBlastEvent bombBlastEvent = swapResult.explodeEvents[i] as BombBlastEvent;
            if (bombBlastEvent != null)
            {
                SoundManager.Instance?.PlayBombSfx();
            }

            combo += 1;
            SoundManager.Instance?.PlayComboSfx(combo);
            for (int j = 0; j < swapResult.explodeEvents[i].toExplode.Count; ++j)
            {
                GameGUI.ExplodeTile(swapResult.explodeEvents[i].toExplode[j].x, swapResult.explodeEvents[i].toExplode[j].y, false);
                if (boardController.GetBoardModel()[swapResult.explodeEvents[i].toExplode[j].x][swapResult.explodeEvents[i].toExplode[j].y].gem.IsBubble())
                {
                    BubbleExploded(swapResult.explodeEvents[i].toExplode[j].x, swapResult.explodeEvents[i].toExplode[j].y);
                }
            }
            onGemsExploded?.Invoke(swapResult.explodeEvents[i].toExplode.Count);
            //IncrementScore(swapResult.explodeEvents[i].toExplode.Count);
        }

        specialSpecialMatch = false;
        GameGUI.SwapDuration = GameGUI.DefaultSwapDuration;

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
                    swapResult.explodeEvents[i].toCreate[j].GemData.gemId,
                    boardController.GetBoardModel().width,
                    boardController.GetBoardModel().height, GameGUI.SwapDuration / 2, levelData);
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
                GameGUI.ColorChangeEffect(colorChangeEvent.targetColor, colorChangeEvent.toChange, levelData);
                yield return new WaitForSeconds(1f);
            }
        }

     
        gameplayState = GameplayState.RefillBoard;
    }

    private void BubbleExploded(int posX, int posy)
    {
        onBubbleExploded(posX, posy);
        //GameGUI.ExplodeBubble(posX, posy);
    }

    public void SetCanSpawnBubbles(bool canSpawn)
    {
        canSpawnBubbles = canSpawn;
    }
}
