#define RANDOM_TILES_OFF
using CodeStage.AntiCheat.ObscuredTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static LevelManager;

public class GamePlayManager : MonoBehaviour
{
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
    List<Vector2> tilesSpecialCoords = new List<Vector2>();
    List<int> tilesSpecial = new List<int>();
    List<Vector2[]> hints = new List<Vector2[]>();
    List<SpecailShapes> hintShapes = new List<SpecailShapes>();
    List<int> availabletiles = new List<int>();
    bool canDisplayhint = false;
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

    public void StartLevel(string levelFile, int levelNumber)
    {
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

        RenderLevel(levelInfo);
        for (int g = 0; g < GameGUI.PlayerGauges.Length; g++)
        {
            GameGUI.PlayerGauges[g].value = GameGUI.PlayerGauges[g].maxValue;
            GameGUI.PlayerGauges[g].transform.Find("TxtHP").GetComponent<TextMeshProUGUI>().text = GameGUI.PlayerGauges[g].maxValue.ToString("N0") + " / " + GameGUI.PlayerGauges[g].maxValue.ToString("N0");
        }

        numHit = new int[] { 0, 0, 0 };
        //timeForNewHint = DateTime.Now + new TimeSpan(0, 0, 7);
        StartHintingCountDown();
    }

    void RenderLevel(LevelInformation levelInfo)
    {
        this.levelInfo = levelInfo;
        GameGUI.RenderLevelBackground(levelInfo);
        GameGUI.InitializeEnemyRobots();        

        bool foundAMatch;

        do
        {
            foundAMatch = false;

            FillTiles(levelInfo);
            if(!MoreMovesArePossible())
            {
                continue;
            }

            hints.Clear();
            hintShapes.Clear();
            for (int x = 0; x < levelInfo.Width; x++)
            {
                for (int y = 0; y < levelInfo.Height; y++)
                {
                    TestForAMatchAround(x, y);
                    if (hints.Count > 0)
                    {
                        foundAMatch = true;
                        break;
                    }
                }
            }
        } while (foundAMatch);

        RenderTiles();
    }

    private void FillTiles(LevelInformation levelInfo)
    {
        tileSet = new string[levelInfo.Width, levelInfo.Height];

        availabletiles.Clear();
        for (int i = 0; i < levelInfo.AvailableTiles.Length; i++)
        {
            availabletiles.Add(int.Parse(levelInfo.AvailableTiles[i]));
        }

        int[,] tileIndices = new int[levelInfo.Width, levelInfo.Height];

#if RANDOM_TILES
        tileIndices[0, 5] = 9; // 10, 11 explode whole board
        tileIndices[1, 5] = 1;
        tileIndices[2, 5] = 3;
        tileIndices[3, 5] = 1;
        tileIndices[4, 5] = 2;
        tileIndices[5, 5] = 11;
        tileIndices[6, 5] = 2;
        tileIndices[7, 5] = 2;

        tileIndices[0, 4] = 1;
        tileIndices[1, 4] = 3;
        tileIndices[2, 4] = 0;
        tileIndices[3, 4] = 3;
        tileIndices[4, 4] = 3;
        tileIndices[5, 4] = 2;
        tileIndices[6, 4] = 0;
        tileIndices[7, 4] = 0;

        tileIndices[0, 3] = 12;
        tileIndices[1, 3] = 0;
        tileIndices[2, 3] = 3;
        tileIndices[3, 3] = 1;
        tileIndices[4, 3] = 0;
        tileIndices[5, 3] = 0;
        tileIndices[6, 3] = 1;
        tileIndices[7, 3] = 3;

        tileIndices[0, 2] = 0;
        tileIndices[1, 2] = 3;
        tileIndices[2, 2] = 1;
        tileIndices[3, 2] = 0;
        tileIndices[4, 2] = 2;
        tileIndices[5, 2] = 1;
        tileIndices[6, 2] = 0;
        tileIndices[7, 2] = 1;

        tileIndices[0, 1] = 2;
        tileIndices[1, 1] = 0;
        tileIndices[2, 1] = 2;
        tileIndices[3, 1] = 0;
        tileIndices[4, 1] = 3;
        tileIndices[5, 1] = 0;
        tileIndices[6, 1] = 1;
        tileIndices[7, 1] = 1;

        tileIndices[0, 0] = 0;
        tileIndices[1, 0] = 0;
        tileIndices[2, 0] = 3;
        tileIndices[3, 0] = 3;
        tileIndices[4, 0] = 1;
        tileIndices[5, 0] = 2;
        tileIndices[6, 0] = 1;
        tileIndices[7, 0] = 3;
#endif
        int index;
        for (int x = 0; x < levelInfo.Width; x++)
        {
            for (int y = 0; y < levelInfo.Height; y++)
            {
#if RANDOM_TILES
                index = tileIndices[x, y];
#else
                index = availabletiles[GetNextTile()];
#endif
                tileSet[x, y] = skinManager.Skins[skinManager.SelectedSkin].TileSet[index].Key;
            }
        }
    }

    private void RenderTiles()
    {
        GameGUI.RenderTiles(tileSet, levelInfo.Width, levelInfo.Height);
    }

    bool MoreMovesArePossible()
    {
        hints.Clear();
        hintShapes.Clear();

        bool specialGemsPresent = false;
        for (int i = 1; i < levelInfo.Width - 1; i++)
        {
            for (int j = 1; j < levelInfo.Height - 1; j++)
            {
                CheckForAMatchWitchSwapingTiles(i, j, i - 1, j);
                CheckForAMatchWitchSwapingTiles(i, j, i + 1, j);
                CheckForAMatchWitchSwapingTiles(i, j, i, j - 1);
                CheckForAMatchWitchSwapingTiles(i, j, i, j + 1);

                if (!specialGemsPresent)
                {
                    specialGemsPresent = IsSpecialGem(tileSet[i, j]);
                }
            }
        }

        if (hints.Count > 0 || specialGemsPresent)
        {
            //DisplayDebugHints();
            return true;
        }

        FindObjectOfType<SoundManager>().FadeOutLevelMusic();
        Debug.Log("No more moves are possible");
        return false;
    }

    private void CheckForAMatchWitchSwapingTiles(int x1, int y1, int x2, int y2)
    {
        if(x1 < 0 || y1 < 0 || x1 >= levelInfo.Width || y1 >= levelInfo.Height ||
           x2 < 0 || y2 < 0 || x2 >= levelInfo.Width || y2 >= levelInfo.Height ||
           tileSet[x1,y1] == tileSet[x2,y2])
        {
            return;
        }

        SwapKeys(x1, y1, x2, y2);
        TestForAMatchAround(x1, y1);
        TestForAMatchAround(x2, y2);
        SwapKeys(x1, y1, x2, y2);
    }

    private void TestForAMatchAround(int x, int y)
    {
        List<Vector2> hintList = new List<Vector2>();
        SpecailShapes shape = SpecailShapes.Nothing;

        hintList.Add(new Vector2(x, y));

        // search through right
        int _left = x;
        int _top = y;
        for (int _x = x + 1; _x < levelInfo.Width; _x++)
        {
            if (tileSet[x, y] != tileSet[_x, y])
            {
                break;
            }

            hintList.Add(new Vector2(_x, y));
        }

        // search through left
        for (int _x = x - 1; _x >= 0; _x--)
        {
            if (tileSet[x, y] != tileSet[_x, y])
            {
                break;
            }

            _left = _x;
            hintList.Add(new Vector2(_x, y));
        }

        if (hintList.Count >= 3)
        {
            // is it a long T
            if (hintList.Count == 5)
            {
                if (y - 1 > 0 && _left + 2 < levelInfo.Width && tileSet[_left + 2, y - 1] == tileSet[x, y])
                {
                    hintList.Add(new Vector2(_left + 2, y - 1));
                    shape = SpecailShapes.LongT;
                }
                else if (y + 1 < levelInfo.Height && _left + 2 < levelInfo.Width && tileSet[_left + 2, y + 1] == tileSet[x, y])
                {
                    hintList.Add(new Vector2(_left + 2, y + 1));
                    shape = SpecailShapes.LongT;
                }
                else
                {
                    shape = SpecailShapes.Straight5;
                }
            }
            else if (hintList.Count == 3)
            {
                // is it an L
                // ---
                // -
                // -
                if (y > 1 && _left < levelInfo.Width && tileSet[_left, y - 1] == tileSet[x, y] && tileSet[_left, y - 2] == tileSet[x, y])
                {
                    hintList.Add(new Vector2(_left, y - 1));
                    hintList.Add(new Vector2(_left, y - 2));
                    shape = SpecailShapes.L;
                }
                else
                // -
                // -
                // ---
                if (y < levelInfo.Height - 2 && _left < levelInfo.Width && tileSet[_left, y + 1] == tileSet[x, y] && tileSet[_left, y + 2] == tileSet[x, y])
                {
                    hintList.Add(new Vector2(_left, y + 1));
                    hintList.Add(new Vector2(_left, y + 2));
                    shape = SpecailShapes.L;
                }
                else
                // is it an L
                // ---
                //   -
                //   -
                if (y > 1 && _left + 2 < levelInfo.Width && tileSet[_left + 2, y - 1] == tileSet[x, y] && tileSet[_left + 2, y - 2] == tileSet[x, y])
                {
                    hintList.Add(new Vector2(_left + 2, y - 1));
                    hintList.Add(new Vector2(_left + 2, y - 2));
                    shape = SpecailShapes.L;
                }
                else
                //   -
                //   -
                // ---
                if (y < levelInfo.Height - 2 && _left + 2 < levelInfo.Width && tileSet[_left + 2, y + 1] == tileSet[x, y] && tileSet[_left + 2, y + 2] == tileSet[x, y])
                {
                    hintList.Add(new Vector2(_left + 2, y + 1));
                    hintList.Add(new Vector2(_left + 2, y + 2));
                    shape = SpecailShapes.L;
                }
                else
                // is it a T
                // ---
                //  -
                //  -
                if (y > 1 && _left + 1 < levelInfo.Width && tileSet[_left + 1, y - 1] == tileSet[x, y] && tileSet[_left + 1, y - 2] == tileSet[x, y])
                {
                    hintList.Add(new Vector2(_left + 1, y - 1));
                    hintList.Add(new Vector2(_left + 1, y - 2));
                    shape = SpecailShapes.T;
                }
                else
                //  -
                //  -
                // ---
                if (y < levelInfo.Height - 2 && _left + 1 < levelInfo.Width && tileSet[_left + 1, y + 1] == tileSet[x, y] && tileSet[_left + 1, y + 2] == tileSet[x, y])
                {
                    hintList.Add(new Vector2(_left + 1, y + 1));
                    hintList.Add(new Vector2(_left + 1, y + 2));
                    shape = SpecailShapes.T;
                }
            }

            if (hintList.Count == 4 && shape == SpecailShapes.Nothing)
            {
                shape = SpecailShapes.Straight4;
            }

            AddHint(hintList.ToArray(), shape);
        }
        else if (hintList.Count == 2)
        {
            if (y > 0 && tileSet[(int)hintList[0].x, (int)hintList[0].y] == tileSet[(int)hintList[0].x, (int)hintList[0].y - 1] && tileSet[(int)hintList[1].x, (int)hintList[1].y] == tileSet[(int)hintList[1].x, (int)hintList[1].y - 1])
            {
                WriteList(hintList);
                hintList.Add(new Vector2((int)hintList[0].x, (int)hintList[0].y - 1));
                hintList.Add(new Vector2((int)hintList[1].x, (int)hintList[1].y - 1));
                shape = SpecailShapes.SmallSquare;
                WriteList(hintList);
                AddHint(hintList.ToArray(), shape);
            }
            else if (y < levelInfo.Height - 1 && tileSet[(int)hintList[0].x, (int)hintList[0].y] == tileSet[(int)hintList[0].x, (int)hintList[0].y + 1] && tileSet[(int)hintList[1].x, (int)hintList[1].y] == tileSet[(int)hintList[1].x, (int)hintList[1].y + 1])
            {
                WriteList(hintList);
                hintList.Add(new Vector2((int)hintList[0].x, (int)hintList[0].y + 1));
                hintList.Add(new Vector2((int)hintList[1].x, (int)hintList[1].y + 1));
                shape = SpecailShapes.SmallSquare;
                WriteList(hintList);
                AddHint(hintList.ToArray(), shape);
            }
        }

        // search through bottom
        hintList.Clear();
        hintList.Add(new Vector2(x, y));

        for (int _y = y - 1; _y >= 0; _y--)
        {
            if (tileSet[x, y] != tileSet[x, _y])
            {
                break;
            }

            hintList.Add(new Vector2(x, _y));
        }

        // search through top
        for (int _y = y + 1; _y < levelInfo.Height; _y++)
        {
            if (tileSet[x, y] != tileSet[x, _y])
            {
                break;
            }

            _top = _y;
            hintList.Add(new Vector2(x, _y));
        }

        if (hintList.Count >= 3)
        {
            // is it a long T
            if (hintList.Count == 5)
            {
                if (x > 0 && _top - 2 >= 0 && tileSet[x - 1, _top - 2] == tileSet[x, y])
                {
                    hintList.Add(new Vector2(x - 1, _top - 2));
                    shape = SpecailShapes.LongT;
                }
                else if (x + 1 < levelInfo.Width && _top - 2 >= 0 && tileSet[x + 1, _top - 2] == tileSet[x, y])
                {
                    hintList.Add(new Vector2(x + 1, _top - 2));
                    shape = SpecailShapes.LongT;
                }
                else
                {
                    shape = SpecailShapes.Straight5;
                }
            }
            else if (hintList.Count == 3)
            {
                // is it an L
                // ---
                //   -
                //   -
                if (x > 1 && _top > 0 && tileSet[x - 1, _top] == tileSet[x, y] && tileSet[x - 2, _top] == tileSet[x, y])
                {
                    hintList.Add(new Vector2(x - 1, _top));
                    hintList.Add(new Vector2(x - 2, _top));
                    shape = SpecailShapes.L;
                }
                else
                // ---
                // -
                // -
                if (x < levelInfo.Width - 2 && _top > 0 && tileSet[x + 1, _top] == tileSet[x, y] && tileSet[x + 2, _top] == tileSet[x, y])
                {
                    hintList.Add(new Vector2(x + 1, _top));
                    hintList.Add(new Vector2(x + 2, _top));
                    shape = SpecailShapes.L;
                }
                else
                // is it an L
                //   -
                //   -
                // ---
                if (x > 1 && _top - 2 >= 0 && tileSet[x - 1, _top - 2] == tileSet[x, y] && tileSet[x - 2, _top - 2] == tileSet[x, y])
                {
                    hintList.Add(new Vector2(x - 1, _top - 2));
                    hintList.Add(new Vector2(x - 2, _top - 2));
                    shape = SpecailShapes.L;
                }
                else
                // -
                // -
                // ---
                if (x < levelInfo.Width - 2 && _top - 2 >= 0 && tileSet[x + 1, _top - 2] == tileSet[x, y] && tileSet[x + 2, _top - 2] == tileSet[x, y])
                {
                    hintList.Add(new Vector2(x + 1, _top - 2));
                    hintList.Add(new Vector2(x + 2, _top - 2));
                    shape = SpecailShapes.L;
                }
                else
                // is it a T
                //   -
                // ---
                //   -
                if (x > 1 && _top - 1 >= 0 && tileSet[x - 1, _top - 1] == tileSet[x, y] && tileSet[x - 2, _top - 1] == tileSet[x, y])
                {
                    hintList.Add(new Vector2(x - 1, _top - 1));
                    hintList.Add(new Vector2(x - 2, _top - 1));
                    shape = SpecailShapes.T;
                }
                else
                // -
                // ---
                // -
                if (x < levelInfo.Width - 2 && _top - 1 >= 0 && tileSet[x + 1, _top - 1] == tileSet[x, y] && tileSet[x + 2, _top - 1] == tileSet[x, y])
                {
                    hintList.Add(new Vector2(x + 1, _top - 1));
                    hintList.Add(new Vector2(x + 2, _top - 1));
                    shape = SpecailShapes.T;
                }
            }

            if (hintList.Count == 4 && shape == SpecailShapes.Nothing)
            {
                shape = SpecailShapes.Straight4;
            }

            AddHint(hintList.ToArray(), shape);
        }
        else if (hintList.Count == 2)
        {
            if (x > 0 && tileSet[(int)hintList[0].x, (int)hintList[0].y] == tileSet[(int)hintList[0].x - 1, (int)hintList[0].y] && tileSet[(int)hintList[1].x, (int)hintList[1].y] == tileSet[(int)hintList[1].x - 1, (int)hintList[1].y])
            {
                hintList.Add(new Vector2((int)hintList[0].x - 1, (int)hintList[0].y));
                hintList.Add(new Vector2((int)hintList[1].x - 1, (int)hintList[1].y));
                shape = SpecailShapes.SmallSquare;

                AddHint(hintList.ToArray(), shape);
            }
            else if (x < levelInfo.Width - 1 && tileSet[(int)hintList[0].x, (int)hintList[0].y] == tileSet[(int)hintList[0].x + 1, (int)hintList[0].y] && tileSet[(int)hintList[1].x, (int)hintList[1].y] == tileSet[(int)hintList[1].x + 1, (int)hintList[1].y])
            {
                hintList.Add(new Vector2((int)hintList[0].x + 1, (int)hintList[0].y));
                hintList.Add(new Vector2((int)hintList[1].x + 1, (int)hintList[1].y));
                shape = SpecailShapes.SmallSquare;

                AddHint(hintList.ToArray(), shape);
            }
        }
    }

    private void WriteList(List<Vector2> list)
    {
        foreach (Vector2 vector in list)
        {
            Debug.Log(vector);
        }
    }

    private bool IsSpecialGem(string v)
    {
        return (v == "S1" || v == "S2" || v == "S3" || v == "S4" || v == "S5");
    }

    private void AddHint(Vector2[] components, SpecailShapes specailShapes)
    {
        bool allTrue;
        for (int i = 0; i < hints.Count; i++)
        {
            allTrue = true;
            for (int j = 0; j < components.Length; j++)
            {
                if (hints[i].Length > j && components[j] != hints[i][j])
                {
                    allTrue = false;
                    break;
                }
            }

            if(allTrue)
            {
                return;
            }
        }

        hints.Add(components);
        hintShapes.Add(specailShapes);
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

        if(!GameGUI.CanSwapTiles)
        {
            return;
        }


        releaseTileX = x;
        releaseTileY = y;
    }

    public void MoveOverTile(int x, int y)
    {
        if (InputLocked())
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
        if(x != releaseTileX && y != releaseTileY)
        {
            return;
        }

        combo = 0;
        canAttack = true;
        if (Mathf.Abs(x - releaseTileX) <= 1 && Mathf.Abs(y - releaseTileY) <= 1)
        {
            if(tileSet[x, y] == tileSet[releaseTileX, releaseTileY] && !IsSpecialGem(tileSet[x, y]))
            {
                StartTrackedCoroutine(SwapTilesBackAndForthOnGUI(x, y, releaseTileX, releaseTileY));
                return;
            }

            GameGUI.LockTiles("L2");

            if (IsSpecialGem(tileSet[x, y]))
            {
                StartTrackedCoroutine(SwapTilesOnceOnGUI(x, y, releaseTileX, releaseTileY));
                StartTrackedCoroutine(ProcessSpecialGem(x, y, releaseTileX, releaseTileY));
                return;
            }
            else if (IsSpecialGem(tileSet[releaseTileX, releaseTileY]))
            {
                StartTrackedCoroutine(SwapTilesOnceOnGUI(x, y, releaseTileX, releaseTileY));
                StartTrackedCoroutine(ProcessSpecialGem(releaseTileX, releaseTileY, x, y));
                return;
            }

            hints.Clear();
            hintShapes.Clear();
            CheckForAMatchWitchSwapingTiles(x, y, releaseTileX, releaseTileY);

            if(hints.Count > 0)
            {
                StartTrackedCoroutine(SwapTilesOnceOnGUI(x, y, releaseTileX, releaseTileY));
                SwapKeys(x, y, releaseTileX, releaseTileY);
                StartTrackedCoroutine(ExplodeTiles());
            }
            else
            {
                CheckForAMatchWitchSwapingTiles(releaseTileX, releaseTileY, x, y);

                if (hints.Count > 0)
                {
                    StartTrackedCoroutine(SwapTilesOnceOnGUI(x, y, releaseTileX, releaseTileY));
                    SwapKeys(x, y, releaseTileX, releaseTileY);
                    StartTrackedCoroutine(ExplodeTiles());
                }
                else
                {
                    StartTrackedCoroutine(SwapTilesBackAndForthOnGUI(x, y, releaseTileX, releaseTileY));
                }
            }
        }
    }

    private void ProcessSpecialMatching()
    {
        int chosenHint = FindHighestHint();
        if (chosenHint < 0 || chosenHint >= hints.Count)
        {
            return;
        }

        bool vertical = (int)hints[chosenHint][0].x == (int)hints[chosenHint][1].x;
        SpecailShapes shape = hintShapes[chosenHint];
        Vector2 leftMost = Vector2.zero;
        Vector2 topMost = Vector2.zero;

        if(shape != SpecailShapes.Nothing)
        {
            topMost = hints[chosenHint][0];
            for (int i = 1; i < hints[chosenHint].Length; i++)
            {
                if (hints[chosenHint][i].y > topMost.y)
                {
                    topMost = hints[chosenHint][i];
                }
            }

            leftMost = hints[chosenHint][0];
            for (int i = 1; i < hints[chosenHint].Length; i++)
            {
                if (hints[chosenHint][i].x < leftMost.x)
                {
                    leftMost = hints[chosenHint][i];
                }
            }

            switch(shape)
            {
                case SpecailShapes.Straight5:
                    // place a special tile here
                    if (vertical)
                    {
                        // place vertical
                        tilesSpecialCoords.Clear();
                        tilesSpecialCoords.Add(new Vector2((int)topMost.x, (int)topMost.y - 2));
                        tilesSpecial.Clear();
                        tilesSpecial.Add(10);
                    }
                    else
                    {
                        // place horizontal
                        tilesSpecialCoords.Clear();
                        tilesSpecialCoords.Add(new Vector2((int)leftMost.x + 2, (int)leftMost.y));
                        tilesSpecial.Clear();
                        tilesSpecial.Add(10);
                    }

                    break;
                case SpecailShapes.Straight4:
                    // place a special tile here
                    if (vertical)
                    {
                        // place vertical
                        tilesSpecialCoords.Clear();
                        tilesSpecialCoords.Add(new Vector2((int)topMost.x, (int)topMost.y - 2));
                        tilesSpecial.Clear();
                        tilesSpecial.Add(9);
                    }
                    else
                    {
                        // place horizontal
                        tilesSpecialCoords.Clear();
                        tilesSpecialCoords.Add(new Vector2((int)leftMost.x + 2, (int)leftMost.y));
                        tilesSpecial.Clear();
                        tilesSpecial.Add(9);
                    }

                    break;
                case SpecailShapes.LongT:
                    if (vertical)
                    {
                        // place vertical
                        tilesSpecialCoords.Clear();
                        tilesSpecialCoords.Add(new Vector2((int)topMost.x, (int)topMost.y - 2));
                        tilesSpecial.Clear();
                        tilesSpecial.Add(13);
                    }
                    else
                    {
                        // place horizontal
                        tilesSpecialCoords.Clear();
                        tilesSpecialCoords.Add(new Vector2((int)leftMost.x + 2, (int)leftMost.y));
                        tilesSpecial.Clear();
                        tilesSpecial.Add(13);
                    }

                    break;
                case SpecailShapes.L:
                    tilesSpecialCoords.Clear();
                    tilesSpecialCoords.Add(DetectCenterL(hints[chosenHint]));
                    tilesSpecial.Clear();
                    tilesSpecial.Add(11);

                    break;
                case SpecailShapes.T:
                    tilesSpecialCoords.Clear();
                    tilesSpecialCoords.Add(DetectCenterT(hints[chosenHint]));
                    tilesSpecial.Clear();
                    tilesSpecial.Add(11);

                    break;
                case SpecailShapes.SmallSquare:
                    tilesSpecialCoords.Clear();
                    tilesSpecialCoords.Add(new Vector2((int)topMost.x, (int)topMost.y - 1));
                    tilesSpecial.Clear();
                    tilesSpecial.Add(12);

                    break;
            }
        }
    }

    private Vector2 DetectCenterT(Vector2[] hint)
    {
        int lowestX = int.MaxValue;
        int lowestY = int.MaxValue;
        int highestX = -1;
        int highestY = -1;

        for (int i = 0; i < hint.Length; i++)
        {
            if (hint[i].x < lowestX)
            {
                lowestX = (int)hint[i].x;
            }

            if (hint[i].x > highestX)
            {
                highestX = (int)hint[i].x;
            }

            if (hint[i].y < lowestY)
            {
                lowestY = (int)hint[i].y;
            }

            if (hint[i].y > highestY)
            {
                highestY = (int)hint[i].y;
            }
        }

        bool leftTop = false;
        bool rightTop = false;
        bool leftBottom = false;
        bool rightBottom = false;

        // test for right bottom
        for (int i = 0; i < hint.Length; i++)
        {
            if (hint[i].x == lowestX && hint[i].y == lowestY)
            {
                leftTop = true;
            } else if (hint[i].x == highestX && hint[i].y == lowestY)
            {
                rightTop = true;
            } else if (hint[i].x == lowestX && hint[i].y == highestY)
            {
                leftBottom = true;
            }
            else if (hint[i].x == highestX && hint[i].y == highestY)
            {
                rightBottom = true;
            }
        }

        if(leftTop && rightTop)
        {
            return new Vector2(lowestX + 1, lowestY);
        } else if (leftTop && leftBottom)
        {
            return new Vector2(lowestX, lowestY + 1);
        } else if (leftBottom && rightBottom)
        {
            return new Vector2(lowestX + 1, highestY);
        } else
        {
            return new Vector2(highestX, lowestY + 1);
        }
    }

    private Vector2 DetectCenterL(Vector2[] hint)
    {
        int lowestX = int.MaxValue;
        int lowestY = int.MaxValue;
        int highestX = -1;
        int highestY = -1;

        for (int i = 0; i < hint.Length; i++)
        {
            if (hint[i].x < lowestX)
            {
                lowestX = (int)hint[i].x;
            }

            if (hint[i].x > highestX)
            {
                highestX = (int)hint[i].x;
            }

            if (hint[i].y < lowestY)
            {
                lowestY = (int)hint[i].y;
            }

            if (hint[i].y > highestY)
            {
                highestY = (int)hint[i].y;
            }
        }

        bool topLeft = false;
        bool topRight = false;
        bool bottomLeft = false;
        bool bottomRight = false;

        // test for right bottom
        for (int i = 0; i < hint.Length; i++)
        {
            if (hint[i].x == lowestX && hint[i].y == lowestY)
            {
                bottomLeft = true;
            }

            if (hint[i].x == highestX && hint[i].y == lowestY)
            {
                bottomRight = true;
            }

            if (hint[i].x == lowestX && hint[i].y == highestY)
            {
                topLeft = true;
            }

            if (hint[i].x == highestX && hint[i].y == highestY)
            {
                topRight = true;
            }
        }

        if (topLeft && topRight && bottomLeft)
        {
            //Debug.Log("L coords: " + lowestX + ", " + highestY);
            return new Vector2(lowestX, highestY);
        }

        if (topLeft && topRight && bottomRight)
        {
            //Debug.Log("L coords: " + highestX + ", " + highestY);
            return new Vector2(highestX, highestY);
        }

        if (topRight && bottomRight && bottomLeft)
        {
            //Debug.Log("L coords: " + highestX + ", " + lowestY);
            return new Vector2(highestX, lowestY);
        }

        //Debug.Log("L coords: " + lowestX + ", " + lowestY);
        return new Vector2(lowestX, lowestY);
    }

    private int FindHighestHint()
    {
        int chosenHint = 0;
        bool shapeFound;
        SpecailShapes[] shapePrecedence = new SpecailShapes[] { SpecailShapes.LongT, SpecailShapes.Straight5, SpecailShapes.L, SpecailShapes.T, SpecailShapes.Straight4, SpecailShapes.SmallSquare };
        for (int s = 0; s < shapePrecedence.Length; s++)
        {
            shapeFound = false;
            for (int i = 0; i < hintShapes.Count; i++)
            {
                if (hintShapes[i] == shapePrecedence[s])
                {
                    shapeFound = true;
                    chosenHint = i;

                    break;
                }
            }

            if (shapeFound)
            {
                break;
            }
        }

        return chosenHint;
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
                    LeaderboardManager.Instance.Score = score;
                } else
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
            if (!MoreMovesArePossible())
            {
                StartTrackedCoroutine(RefreshBoard());
            }
        }
    }

    IEnumerator RefreshBoard()
    {
        yield return new WaitForSeconds(GameGUI.SwapDuration);

        MenuGUI.DisplayNoMoreMoves();

        yield return new WaitForSeconds(3);

        RenderLevel(levelInfo);
        MenuGUI.HideNoMoreMoves();
        GameGUI.CanSwapTiles = true;
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
        yield return new WaitForSeconds(GameGUI.SwapDuration);

        enemyDead = true;
        ResetHintTime();
        GameGUI.LockTiles("L3");
        numLevel += 1;

        MenuGUI.gameObject.SetActive(true);
        MenuGUI.DisplayWin();
    }

    IEnumerator ExplodeTiles()
    {
        //DisplayDebugHints();
        yield return new WaitForSeconds(GameGUI.SwapDuration);
        tilesToSlide.Clear();
        tilesToPut.Clear();

        bool vertical;

        if(hints.Count == 0)
        {
            ReleaseTiles("R1");
            yield break;
        }

        // sort hints
        Vector2 temp;
        for (int i = 0; i < hints.Count; i++)
        {
            vertical = hints[i][0].y == hints[i][1].y;
            for (int vi = 0; vi < hints[i].Length; vi++)
            {
                for (int vj = vi + 1; vj < hints[i].Length; vj++)
                {
                    if (vertical)
                    {
                        if (hints[i][vj].x < hints[i][vi].x)
                        {
                            // swap
                            temp = hints[i][vj];
                            hints[i][vj] = hints[i][vi];
                            hints[i][vi] = temp;
                        }
                    }
                    else
                    {
                        if (hints[i][vj].y < hints[i][vi].y)
                        {
                            // swap
                            temp = hints[i][vj];
                            hints[i][vj] = hints[i][vi];
                            hints[i][vi] = temp;
                        }
                    }
                }
            }
        }

        // eliminate duplicate hints
        bool deleted;
        bool allTheSame;
        do
        {
            deleted = false;
            for (int i = 0; i < hints.Count; i++)
            {
                for (int j = i + 1; j < hints.Count; j++)
                {
                    if (hints[i].Length != hints[j].Length)
                    {
                        continue;
                    }

                    allTheSame = true;
                    for (int v = 0; v < hints[i].Length; v++)
                    {
                        if (hints[i][v].x != hints[j][v].x || hints[i][v].y != hints[j][v].y)
                        {
                            allTheSame = false;
                            break;
                        }
                    }

                    if(allTheSame)
                    {
                        deleted = true;
                        hints.RemoveAt(i);

                        break;
                    }
                }

                if(deleted)
                {
                    break;
                }
            }

        } while (deleted);

        combo += 1;
        FindObjectOfType<SoundManager>().PlayComboSound(combo);

        ProcessTheHints();
        ProcessSpecialMatching();
        ProcessScrolling();
        HitEnemy();

        // final step: test if new blocks appeared
        ProcessNewlyAppearedBlocks(tilesToPut);
    }

    private void ProcessTheHints()
    {
        for (int selected = 0; selected < hints.Count; selected++)
        {
            for (int indx = 0; indx < hints[selected].Length; indx++)
            {
                if (IsSpecialGem(tileSet[(int)hints[selected][indx].x, (int)hints[selected][indx].y]))
                {
                    BlastWholeBoard();
                    break;
                }

                try
                {
                    GameGUI.ExplodeTile((int)hints[selected][indx].x, (int)hints[selected][indx].y, false);
                    tileSet[(int)hints[selected][indx].x, (int)hints[selected][indx].y] = "X";
                } catch
                {
                    Debug.LogError("hint could not be processed (selected: " + selected + ", length: " + (int)hints.Count + ", indx: " + indx + ")");
                }
            }
        }
    }

    private void ProcessScrolling()
    {
        int newIndex;
        tilesToSlide.Clear();
        tilesToPut.Clear();
        for (int x = 0; x < levelInfo.Width; x++)
        {
            for (int y = 0; y < levelInfo.Height; y++)
            {
                if (tileSet[x, y] == "X")
                {
                    // find the nearest tile towards the top
                    int foundAt = -1;
                    for (int sy = y + 1; sy < levelInfo.Height; sy++)
                    {
                        if (tileSet[x, sy] != "X")
                        {
                            foundAt = sy;
                            break;
                        }
                    }

                    if (foundAt == -1)
                    {
                        // fill from here until the top
                        for (int py = y; py < levelInfo.Height; py++)
                        {
                            newIndex = GetNextTile();
                            for (int i = 0; i < tilesSpecialCoords.Count; i++)
                            {
                                if (tilesSpecialCoords[i].x == x && tilesSpecialCoords[i].y == py)
                                {
                                    newIndex = tilesSpecial[i];
                                    break;
                                }
                            }

                            tileSet[x, py] = skinManager.Skins[skinManager.SelectedSkin].TileSet[newIndex].Key;
                            GameGUI.AppearAt(x, py, tileSet[x, py]);
                            

                            AddTilesToPut(new Vector2(x, py));
                        }

                        break;
                    }
                    else
                    {
                        // scroll the tile from x, foundAt to x, y
                        bool foundInSpecialMatches = false;
                        for (int i = 0; i < tilesSpecialCoords.Count; i++)
                        {
                            if (tilesSpecialCoords[i].x == x && tilesSpecialCoords[i].y == y)
                            {
                                tileSet[x, y] = skinManager.Skins[skinManager.SelectedSkin].TileSet[tilesSpecial[i]].Key;
                                foundInSpecialMatches = true;
                                GameGUI.AppearAt(x, y, tileSet[x, y]);
                                
                                break;
                            }
                        }

                        if (!foundInSpecialMatches)
                        {
                            GameGUI.ScrollTileDown(x, foundAt, foundAt - y);
                            tileSet[x, y] = tileSet[x, foundAt];
                            tileSet[x, foundAt] = "X";

                            tilesToSlide.Add(new SlideInformation(new Vector2(x, y), new Vector2(x, foundAt)));
                        }
                    }
                }
            }
        }
    }

    private void AddTilesToPut(Vector2 v)
    {
        for (int i = 0; i < tilesToPut.Count; i++)
        {
            if (tilesToPut[i].x == v.x && tilesToPut[i].y == v.y)
            {
                return;
            }
        }

        tilesToPut.Add(v);
    }

    public void ProcessNewlyAppearedBlocks(List<Vector2> tilesToPut)
    {
        if (levelEnded)
        {
            return;
        }

        Vector2 to;
        // TODO: Process special matching
        List<Vector2[]> tempHints = new List<Vector2[]>();
        List<SpecailShapes> tempHintShapes = new List<SpecailShapes>();
        for (int i = 0; i < tilesToSlide.Count; i++)
        {
            hints.Clear();
            hintShapes.Clear();
            to = tilesToSlide[i].To;
            // search for a special gem
            TestForAMatchAround((int)to.x, (int)to.y);
            for (int h = 0; h < hints.Count; h++)
            {
                tempHints.Add(hints[h]);
                tempHintShapes.Add(hintShapes[h]);
            }
        }

        for (int i = 0; i < tilesToPut.Count; i++)
        {
            hints.Clear();
            hintShapes.Clear();
            TestForAMatchAround((int)tilesToPut[i].x, (int)tilesToPut[i].y);
            for (int h = 0; h < hints.Count; h++)
            {
                tempHints.Add(hints[h]);
                tempHintShapes.Add(hintShapes[h]);
            }
        }

        if (tempHints.Count > 0)
        {
            hints.Clear();
            hintShapes.Clear();
            for (int i = 0; i < tempHints.Count; i++)
            {
                hints.Add(tempHints[i]);
                hintShapes.Add(tempHintShapes[i]);
            }

            StartTrackedCoroutine(MoveOverAfter(0.5f));
            StartHintingCountDown();
        }
        else
        {
            ReleaseTiles("R2");
        }
    }

    IEnumerator MoveOverAfter(float delay)
    {
        yield return new WaitForSeconds(delay);

        StartTrackedCoroutine(ExplodeTiles());
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
    }

    IEnumerator SwapTilesOnceOnGUI(int x1, int y1, int x2, int y2)
    {
        GameGUI.SwapTiles(x1, y1, x2, y2, true);
        yield return new WaitForSeconds(GameGUI.SwapDuration);

        //ReleaseTiles();
        releaseTileX = -1;
        releaseTileY = -1;
    }

    public void ZeroReleasedTiles()
    {
        releaseTileX = -1;
        releaseTileY = -1;
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

    public void DisplayDebugHints()
    {
        Debug.Log(hints.Count + " more moves are possible:");
        string s = "";
        for (int i = 0; i < hints.Count; i++)
        {
            for (int j = 0; j < hints[i].Length; j++)
            {
                s += hints[i][j].x + "," + hints[i][j].y + "-" + tileSet[(int)hints[i][j].x, (int)hints[i][j].y] + " :: ";
            }

            s += System.Environment.NewLine;
        }

        Debug.Log("They are: " + s);
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

    int GetNextTile()
    {
        return UnityEngine.Random.Range(0, availabletiles.Count);
    }

    IEnumerator ProcessSpecialGem(int x, int y, int releaseX, int releaseY)
    {
        yield return new WaitForSeconds(GameGUI.SwapDuration);

        StartTrackedCoroutine(SubProcessSpecialGem(x, y, releaseX, releaseY));
    }

    IEnumerator SubProcessSpecialGem(int x, int y, int releaseX, int releaseY)
    {
        bool horizontal = x != releaseX;
        //Debug.Log("D0: " + tileSet[x, y] + " (" + x + "," + y + ") :: (" + releaseX + "," + releaseY + ")");
        switch (tileSet[x, y])
        {
            case "S1":
                FindObjectOfType<SoundManager>().PlayLightningSound();
                GameGUI.LineDestroyEffect(x, y, !horizontal);
                yield return new WaitForSeconds(0.4f);

                ProcessLineBlast(x, y, horizontal, tileSet[releaseX, releaseY] == "S1");
                break;
            case "S2":
                GameGUI.ColorBlastEffect(x, y);
                FindObjectOfType<SoundManager>().PlayColorSound();
                yield return new WaitForSeconds(0.51f);

                /*if (tileSet[releaseX, releaseY] == "S2")
                {
                    BlastWholeBoard();
                }
                else
                {*/
                ProcessColorBlast(x, y, tileSet[releaseX, releaseY]);
                //}
                break;
            case "S3":
                FindObjectOfType<SoundManager>().PlayBombSound();
                if (tileSet[releaseX, releaseY] == "S3")
                {
                    BlastWholeBoard();
                }
                else
                {
                    ProcessGridBlast(releaseX, releaseY);
                }

                break;
            case "S4":
                FindObjectOfType<SoundManager>().PlayHammerSound();
                ProcessPlusBlast(releaseX, releaseY);

                break;
            case "S5":
                string code = tileSet[releaseX, releaseY];
                List<Vector2> changedTiles = ProcessRandomColorChange(x, y, releaseX, releaseY);
                GameGUI.ColorChangeEffect(code, changedTiles);
                StartTrackedCoroutine(ReevaluateBoard());

                yield break;
        }

        tilesSpecialCoords.Clear();
        tilesSpecial.Clear();

        ProcessScrolling();
        HitEnemy();
        //ReleaseTiles();

        if (!levelEnded)
        {
            ProcessNewlyAppearedBlocks(tilesToPut);
        }
    }

    private void BlastWholeBoard()
    {
        for (int x = 0; x < levelInfo.Width; x++)
        {
            for (int y = 0; y < levelInfo.Height; y++)
            {
                GameGUI.ExplodeTile(x, y, false);
                tileSet[x, y] = "X";
            }
        }
    }

    private void ProcessPlusBlast(int x, int y)
    {
        //
        if (x > 0)
        {
            GameGUI.ExplodeTile(x - 1, y, false);
            tileSet[x - 1, y] = "X";
        }

        //
        if (y > 0)
        {
            GameGUI.ExplodeTile(x, y - 1, false);
            tileSet[x, y - 1] = "X";
        }

        GameGUI.ExplodeTile(x, y, false);
        tileSet[x, y] = "X";

        if (y < levelInfo.Height - 1)
        {
            GameGUI.ExplodeTile(x, y + 1, false);
            tileSet[x, y + 1] = "X";
        }

        //
        if (x < levelInfo.Width - 1)
        {
            GameGUI.ExplodeTile(x + 1, y, false);
            tileSet[x + 1, y] = "X";
        }
    }

    private void ProcessGridBlast(int x, int y)
    {
        //
        if (x > 0)
        {
            if (y > 0)
            {
                if (IsSpecialGem(tileSet[x - 1, y - 1]))
                {
                    ProcessSpecialGem(x, y, x - 1, y - 1);
                }

                GameGUI.ExplodeTile(x - 1, y - 1, false);
                tileSet[x - 1, y - 1] = "X";
            }

            if (IsSpecialGem(tileSet[x - 1, y]))
            {
                ProcessSpecialGem(x, y, x - 1, y);
            }

            GameGUI.ExplodeTile(x - 1, y, false);
            tileSet[x - 1, y] = "X";

            if (y < levelInfo.Height - 1)
            {
                if (IsSpecialGem(tileSet[x - 1, y + 1]))
                {
                    ProcessSpecialGem(x, y, x - 1, y + 1);
                }

                GameGUI.ExplodeTile(x - 1, y + 1, false);
                tileSet[x - 1, y + 1] = "X";
            }
        }

        //
        if (y > 0)
        {
            if (IsSpecialGem(tileSet[x, y - 1]))
            {
                ProcessSpecialGem(x, y, x, y - 1);
            }

            GameGUI.ExplodeTile(x, y - 1, false);
            tileSet[x, y - 1] = "X";
        }

        GameGUI.ExplodeTile(x, y, false);
        tileSet[x, y] = "X";

        if (y < levelInfo.Height - 1)
        {
            if (IsSpecialGem(tileSet[x, y + 1]))
            {
                ProcessSpecialGem(x, y, x, y + 1);
            }

            GameGUI.ExplodeTile(x, y + 1, false);
            tileSet[x, y + 1] = "X";
        }

        //
        if (x < levelInfo.Width - 1)
        {
            if (y > 0)
            {
                if (IsSpecialGem(tileSet[x + 1, y - 1]))
                {
                    ProcessSpecialGem(x, y, x + 1, y - 1);
                }

                GameGUI.ExplodeTile(x + 1, y - 1, false);
                tileSet[x + 1, y - 1] = "X";
            }

            if (IsSpecialGem(tileSet[x + 1, y]))
            {
                ProcessSpecialGem(x, y, x + 1, y);
            }

            GameGUI.ExplodeTile(x + 1, y, false);
            tileSet[x + 1, y] = "X";

            if (y < levelInfo.Height - 1)
            {
                if (IsSpecialGem(tileSet[x + 1, y + 1]))
                {
                    ProcessSpecialGem(x, y, x + 1, y + 1);
                }

                GameGUI.ExplodeTile(x + 1, y + 1, false);
                tileSet[x + 1, y + 1] = "X";
            }
        }
    }

    IEnumerator ReevaluateBoard()
    {
        yield return new WaitForSeconds(1);

        hints.Clear();
        hintShapes.Clear();
        //Debug.Log("S0");

        for (int x = 0; x < levelInfo.Width; x++)
        {
            for (int y = 0; y < levelInfo.Height; y++)
            {
                TestForAMatchAround(x, y);
            }
        }

        //Debug.Log("S1 (" + hints.Count + ")");
        tilesSpecialCoords.Clear();
        tilesSpecial.Clear();

        ProcessTheHints();
        ProcessScrolling();
        HitEnemy();
        //ReleaseTiles();

        if (!levelEnded)
        {
            ProcessNewlyAppearedBlocks(tilesToPut);
        }
    }

    private List<Vector2> ProcessRandomColorChange(int x, int y, int releaseX, int releaseY)
    {
        string code = tileSet[releaseX, releaseY];
        List<Vector2> result = new List<Vector2>();

        // change numElements different elements from different colors to the ones which are in the long T6
        int numElements = 9; //UnityEngine.Random.Range(5, 7);
        int _x;
        int _y;
        bool overlapPresent;
        int numAttempts;
        for (int i = 0; i < numElements; i++)
        {
            overlapPresent = false;
            numAttempts = 0;
            do
            {
                _x = UnityEngine.Random.Range(0, levelInfo.Width);
                _y = UnityEngine.Random.Range(0, levelInfo.Height);
                if (numAttempts++ >= 100)
                {
                    Debug.LogWarning("ProcessRandomColorChange attempted for 100 times!");
                    break;
                }

                if (IsSpecialGem(tileSet[_x, _y]))
                {
                    overlapPresent = true;
                }
                else
                {
                    for (int h = 0; h < hints.Count; h++)
                    {
                        for (int f = 0; f < hints[h].Length; f++)
                        {
                            if (hints[h][f].x == _x && hints[h][f].y == _y)
                            {
                                overlapPresent = true;
                                break;
                            }
                        }

                        if (overlapPresent)
                        {
                            break;
                        }
                    }
                }

                // not overlaped, good, but is the target gem in a different color?
                if (!overlapPresent)
                {
                    if (tileSet[_x, _y] == tileSet[x, y])
                    {
                        overlapPresent = true;
                    }
                }
            } while (overlapPresent);

            // it is safe to convert the gem to the given color
            tileSet[_x, _y] = code;
            result.Add(new Vector2(_x, _y));
            AddTilesToPut(new Vector2(_x, _y));
        }

        result.Add(new Vector2(x, y));
        tileSet[x, y] = code;
        AddTilesToPut(new Vector2(x, y));

        return result;
    }

    private void ProcessColorBlast(int x, int y, string v)
    {
        for (int _x = 0; _x < levelInfo.Width; _x++)
        {
            for (int _y = 0; _y < levelInfo.Height; _y++)
            {
                if(x == _x && y == _y)
                {
                    continue;
                }

                if (tileSet[_x, _y] == v)
                {
                    GameGUI.ExplodeTile(_x, _y, false);
                    tileSet[_x, _y] = "X";
                }
            }
        }

        GameGUI.ExplodeTile(x, y, false);
        tileSet[x, y] = "X";
    }

    private void ProcessLineBlast(int x, int y, bool horizontal, bool bothDir)
    {
        if (horizontal || bothDir)
        {
            // explode horizontal
            for (int indx = 0; indx < levelInfo.Width; indx++)
            {
                if (indx != x && IsSpecialGem(tileSet[indx, y]))
                {
                    StartTrackedCoroutine(ProcessSpecialGem(indx, y, x, y));
                }

                tileSet[indx, y] = "X";
                GameGUI.ExplodeTile(indx, y, false);
            }
        }

        if (!horizontal || bothDir)
        {
            // explode vertical
            for (int indy = 0; indy < levelInfo.Height; indy++)
            {
                if(bothDir && indy == y)
                {
                    continue;
                }

                if (indy != y && IsSpecialGem(tileSet[x, indy]))
                {
                    StartTrackedCoroutine(ProcessSpecialGem(x, indy, x, y));
                }

                tileSet[x, indy] = "X";
                GameGUI.ExplodeTile(x, indy, false);
            }
        }
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
        this.score += score;

        return this.score;
    }

    private void Update()
    {
        if(canDisplayhint && timeForNewHint != 0 && timeForNewHint <= Time.time)
        {
            //Debug.Log("Testing the hint (next test is in " + timeForNewHint + ")");
            //timeForNewHint = DateTime.Now + new TimeSpan(0, 0, HintDuration + 1);
            StartHintingCountDown();

            hints.Clear();
            hintShapes.Clear();
            //Debug.Log("S0");

            int firstVertical = 0;
            for (int x = 0; x < levelInfo.Width - 1; x++)
            {
                for (int y = 0; y < levelInfo.Height - 1; y++)
                {
                    // test right
                    SwapKeys(x, y, x + 1, y);
                    TestForAMatchAround(x, y);
                    TestForAMatchAround(x + 1, y);
                    SwapKeys(x, y, x + 1, y);

                    firstVertical = hints.Count;
                    // test down
                    SwapKeys(x, y, x, y + 1);
                    TestForAMatchAround(x, y);
                    TestForAMatchAround(x, y + 1);
                    SwapKeys(x, y, x, y + 1);
                }
            }

            if (hints.Count > 0)
            {
                int selectedHint = 0;
                int highestCount = 0;
                for (int i = 0; i < hints.Count; i++)
                {
                    if (hints[i].Length > highestCount)
                    {
                        highestCount = hints[i].Length;
                        selectedHint = i;
                    }
                }

                GameGUI.DisplayHintAt((int)hints[selectedHint][0].x, (int)hints[selectedHint][0].y);
            }

            hints.Clear();
            hintShapes.Clear();
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

}
