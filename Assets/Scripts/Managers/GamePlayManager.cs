#define RANDOM_TILES_OFF
using System;
using System.Collections;
using System.Collections.Generic;
//using UnityEditor.Rendering.LookDev;
using UnityEngine;
using static LevelManager;
//using static UnityEditor.Searcher.SearcherWindow.Alignment;

public class GamePlayManager : MonoBehaviour
{
    public LevelManager levelManager;
    public GUIMenu MenuGUI;
    public GUIGame GameGUI;
    public SkinManager skinManager;
    public float DamageOfRobot1 = 0.05f;
    public float DamageOfRobot2 = 0.05f;
    public Animator Robot1Anim;
    public Animator Robot2Anim;
    public GameObject HitEffect1;
    public GameObject HitEffect2;
    public int[] EnemyHPs = new int[] { 40, 40, 40 };
    int[] numHit = new int[] { 0, 0, 0 };

    List<SlideInformation> tilesToSlide = new List<SlideInformation>();
    LevelInformation levelInfo;
    string[,] tileSet;
    List<Vector2> tilesToPut = new List<Vector2>();
    List<Vector2> tilesSpecialCoords = new List<Vector2>();
    List<int> tilesSpecial = new List<int>();
    List<Vector2[]> hints = new List<Vector2[]>();
    List<int> availabletiles = new List<int>();
    int releaseTileX = -1;
    int releaseTileY = -1;
    bool enemyDead = false;
    int numLevel = 1;
    int currentLevel = 0;
    int currentEnemy = 0;
    int killedEnemies = 0;
    int maxEnemies = 3;
    long score = 0;

    // special matching related stuff
    bool smLongT = false;
    bool sm4Square = false;
    bool sm5InLine = false;
    bool sm4InLine = false;
    bool smBigT = false;
    bool smBigL = false;

    public string[,] TileSet
    {
        get
        {
            return tileSet;
        }
    }

    public void PrepareLevel(string levelFile, int levelNumber)
    {
        MenuGUI.SwitchToMultiplayer(levelFile, levelNumber);
    }

    public void StartLevel(string levelFile, int levelNumber)
    {
        LevelInformation levelInfo;
        currentLevel = levelNumber;
        currentEnemy = 0;
        killedEnemies = 0;

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
        GameGUI.TargetEnemy(0);
        numHit = new int[] { 0, 0, 0 };
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
        tileIndices[0, 4] = 3;
        tileIndices[1, 4] = 3;
        tileIndices[2, 4] = 0;
        tileIndices[3, 4] = 3;
        tileIndices[4, 4] = 3;
        tileIndices[5, 4] = 2;
        tileIndices[6, 4] = 0;
        tileIndices[7, 4] = 0;

        tileIndices[0, 3] = 3;
        tileIndices[1, 3] = 0;
        tileIndices[2, 3] = 3;
        tileIndices[3, 3] = 1;
        tileIndices[4, 3] = 0;
        tileIndices[5, 3] = 0;
        tileIndices[6, 3] = 2;
        tileIndices[7, 3] = 1;

        tileIndices[0, 2] = 0;
        tileIndices[1, 2] = 3;
        tileIndices[2, 2] = 1;
        tileIndices[3, 2] = 0;
        tileIndices[4, 2] = 2;
        tileIndices[5, 2] = 3;
        tileIndices[6, 2] = 1;
        tileIndices[7, 2] = 3;

        tileIndices[0, 1] = 2;
        tileIndices[1, 1] = 0;
        tileIndices[2, 1] = 2;
        tileIndices[3, 1] = 0;
        tileIndices[4, 1] = 3;
        tileIndices[5, 1] = 0;
        tileIndices[6, 1] = 0;
        tileIndices[7, 1] = 1;

        tileIndices[0, 0] = 1;
        tileIndices[1, 0] = 0;
        tileIndices[2, 0] = 3;
        tileIndices[3, 0] = 3;
        tileIndices[4, 0] = 1;
        tileIndices[5, 0] = 2;
        tileIndices[7, 0] = 1;
        tileIndices[6, 0] = 3;

        tileIndices[0, 5] = 2;
        tileIndices[1, 5] = 1;
        tileIndices[2, 5] = 3;
        tileIndices[3, 5] = 1;
        tileIndices[4, 5] = 2;
        tileIndices[5, 5] = 1;
        tileIndices[6, 5] = 2;
        tileIndices[7, 5] = 2;
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

        for (int i = 1; i < levelInfo.Width - 1; i++)
        {
            for (int j = 1; j < levelInfo.Height - 1; j++)
            {
                CheckForAMatchWitchSwapingTiles(i, j, i - 1, j);
                CheckForAMatchWitchSwapingTiles(i, j, i + 1, j);
                CheckForAMatchWitchSwapingTiles(i, j, i, j - 1);
                CheckForAMatchWitchSwapingTiles(i, j, i, j + 1);
            }
        }

        if (hints.Count > 0)
        {
            //DisplayDebugHints();
            return true;
        }

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

        // search through left
        hintList.Add(new Vector2(x, y));

        for (int _x = x - 1; _x >= 0; _x--)
        {
            if (tileSet[x, y] != tileSet[_x, y])
            {
                break;
            }

            hintList.Add(new Vector2(_x, y));
        }

        // search through right
        for (int _x = x + 1; _x < levelInfo.Width; _x++)
        {
            if (tileSet[x, y] != tileSet[_x, y])
            {
                break;
            }

            hintList.Add(new Vector2(_x, y));
        }

        if (hintList.Count >= 3)
        {
            AddHint(hintList.ToArray());
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

            hintList.Add(new Vector2(x, _y));
        }

        if (hintList.Count >= 3)
        {
            AddHint(hintList.ToArray());
        }
    }

    private bool IsSpecialGem(string v)
    {
        return (v == "S1" || v == "S2" || v == "S3" || v == "S4" || v == "S5");
    }

    private void AddHint(Vector2[] components)
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
    }

    private void SwapKeys(int x1, int y1, int x2, int y2)
    {
        string temp = tileSet[x1, y1];
        tileSet[x1, y1] = tileSet[x2, y2];
        tileSet[x2, y2] = temp;
    }

    public void SetDownTile(int x, int y)
    {
        if(!GameGUI.CanSwapTiles)
        {
            return;
        }

        releaseTileX = x;
        releaseTileY = y;
    }

    public void MoveOverTile(int x, int y)
    {
        if (!GameGUI.CanSwapTiles || (x == releaseTileX && y == releaseTileY) || releaseTileX == -1 || releaseTileY == -1)
        {
            return;
        }

        if (Mathf.Abs(x - releaseTileX) <= 1 && Mathf.Abs(y - releaseTileY) <= 1 && tileSet[x, y] != tileSet[releaseTileX, releaseTileY])
        {
            GameGUI.CanSwapTiles = false;

            if (IsSpecialGem(tileSet[x, y]))
            {
                StartCoroutine(SwapTilesOnceOnGUI(x, y, releaseTileX, releaseTileY));
                StartCoroutine(ProcessSpecialGem(x, y, releaseTileX, releaseTileY));

                return;
            }
            else if (IsSpecialGem(tileSet[releaseTileX, releaseTileY]))
            {
                StartCoroutine(SwapTilesOnceOnGUI(x, y, releaseTileX, releaseTileY));
                StartCoroutine(ProcessSpecialGem(releaseTileX, releaseTileY, x, y));

                return;
            }

            hints.Clear();
            CheckForAMatchWitchSwapingTiles(x, y, releaseTileX, releaseTileY);

            if(hints.Count > 0)
            {
                PrepareToProcessSpeacialMatching();
                StartCoroutine(SwapTilesOnceOnGUI(x, y, releaseTileX, releaseTileY));
                SwapKeys(x, y, releaseTileX, releaseTileY);
                StartCoroutine(ExplodeTiles());
            } else
            {
                CheckForAMatchWitchSwapingTiles(releaseTileX, releaseTileY, x, y);

                if (hints.Count > 0)
                {
                    PrepareToProcessSpeacialMatching();
                    StartCoroutine(SwapTilesOnceOnGUI(x, y, releaseTileX, releaseTileY));
                    SwapKeys(x, y, releaseTileX, releaseTileY);
                    StartCoroutine(ExplodeTiles());
                }
                else
                {
                    StartCoroutine(SwapTilesBackAndForthOnGUI(x, y, releaseTileX, releaseTileY));
                }
            }
        }
    }

    private void ProcessSpecialMatching(bool vertical, int x, int y)
    {
        Vector2 leftMost = Vector2.zero;
        Vector2 topMost = Vector2.zero;

        sm5InLine = false;
        sm4InLine = false;
        smBigT = false;
        smLongT = false;
        sm4Square = false;
        smBigL = false;

        if (hints.Count > 0)
        {
            if (hints[0].Length >= 5)
            {
                Debug.Log("5 in line!");
                sm5InLine = true;
            }
            else if (hints[0].Length == 4)
            {
                Debug.Log("4 in line!");
                sm4InLine = true;
            }
            else if (hints.Count > 1)
            {
                // is it a T or an L?
            }
        }

        // a separate inspection is necessary for 4 squares
        bool collidesWithAHint;
        for (int _x = 0; _x < levelInfo.Width - 1; _x++)
        {
            for (int _y = 0; _y < levelInfo.Height - 1; _y++)
            {
                if (tileSet[_x, _y] == tileSet[_x + 1, _y] && tileSet[_x, _y] == tileSet[_x + 1, _y + 1] && tileSet[_x, _y] == tileSet[_x, _y + 1])
                {
                    // test whether any part of this square collides with another hint
                    collidesWithAHint = false;
                    for (int i = 0; i < hints.Count; i++)
                    {
                        for (int l = 0; l < hints[i].Length; l++)
                        {
                            if (hints[i][l].x == _x || hints[i][l].x == _x + 1 || hints[i][l].y == _y || hints[i][l].y == _y + 1)
                            {
                                collidesWithAHint = true;
                                break;
                            }
                        }
                    }

                    if (!collidesWithAHint)
                    {
                        sm4Square = true;
                        leftMost.x = _x;
                        leftMost.y = _y;

                        /*tilesSpecialCoords.Clear();
                        tilesSpecialCoords.Add(new Vector2((int)leftMost.x, (int)leftMost.y));
                        tilesSpecial.Clear();
                        tilesSpecial.Add(11);*/

                        break;
                    }
                }
            }

            if(sm4Square)
            {
                break;
            }
        }

        if (vertical)
        {
            while(y < levelInfo.Height - 1)
            {
                y += 1;

                if (tileSet[x, y - 1] != tileSet[x, y])
                {
                    y -= 1;
                    break;
                }
            }

            topMost = new Vector2(x, y);

            // is it a long T or a straight line?
            if (sm5InLine && topMost.x < levelInfo.Width - 1)
            {
                try
                {
                    if (tileSet[(int)topMost.x + 1, (int)topMost.y + 2] == tileSet[(int)topMost.x, (int)topMost.y])
                    {
                        smBigT = true;
                        sm5InLine = false;
                    }
                }
                catch
                {
                    Debug.LogError("Error 1301 == x: " + (topMost.x + 1) + ", y: " + (topMost.y + 2) + ", tsl: " + levelInfo.Width + ", tsh: " + levelInfo.Height);
                }
            } else if (sm5InLine && topMost.x > 0)
            {
                if (tileSet[(int)topMost.x - 1, (int)topMost.y + 2] == tileSet[(int)topMost.x, (int)topMost.y])
                {
                    smBigT = true;
                    sm5InLine = false;
                }
            }
        }
        else
        {
            while (x > 0)
            {
                x -= 1;

                if (tileSet[x, y] != tileSet[x + 1, y])
                {
                    x += 1;
                    break;
                }
            }

            leftMost = new Vector2(x, y);

            // is it a long T or a straight line?
            if (sm5InLine && leftMost.y < levelInfo.Height - 1)
            {
                if(tileSet[(int)leftMost.x + 2, (int)leftMost.y + 1] == tileSet[(int)leftMost.x, (int)leftMost.y])
                {
                    smBigT = true;
                    sm5InLine = false;
                }
            } else if (sm5InLine && leftMost.y > 0)
            {
                if (tileSet[(int)leftMost.x + 2, (int)leftMost.y - 1] == tileSet[(int)leftMost.x, (int)leftMost.y])
                {
                    smBigT = true;
                    sm5InLine = false;
                }
            }
        }

        if (sm5InLine)
        {
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

            sm5InLine = false;
        }
        else if (sm4InLine)
        {
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

            sm4InLine = false;
        }
        else if (smBigT)
        {
            Debug.Log("BigT: vertical: " + vertical + ", topMost: " + topMost + ", leftMost: " + leftMost);

            // place a special tile here
            if (vertical)
            {
                // place vertical
                tilesSpecialCoords.Clear();
                tilesSpecialCoords.Add(new Vector2((int)topMost.x + 2, (int)topMost.y));
                tilesSpecial.Clear();
                tilesSpecial.Add(13);
            }
            else
            {
                // place horizontal
                tilesSpecialCoords.Clear();
                tilesSpecialCoords.Add(new Vector2((int)leftMost.x, (int)leftMost.y - 2));
                tilesSpecial.Clear();
                tilesSpecial.Add(13);
            }

            smBigT = false;
        }
    }

    private void PrepareToProcessSpeacialMatching()
    {

    }

    private void HitEnemy()
    {
        GameGUI.DamageToEnemyRobot(tilesToPut.Count);
        var hitEffect = Instantiate(HitEffect1);
        hitEffect.transform.position = HitEffect1.transform.position;
        hitEffect.SetActive(true);
        Destroy(hitEffect.gameObject, 2);

        numHit[currentEnemy] += tilesToPut.Count;
        if (numHit[currentEnemy] % 2 == 0)
        {
            HitPlayer();
        }

        if (numHit[currentEnemy] >= EnemyHPs[currentEnemy])
        {
            KillEnemy();
            //numHit[currentEnemy] = 0;

            if (++killedEnemies >= maxEnemies)
            {
                StartCoroutine(FinishLevel());
                LeaderboardManager.Instance.Score = score;
            }
        }
        else
        {
            if (!MoreMovesArePossible())
            {
                StartCoroutine(RefreshBoard());
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
        GameGUI.DamageToPlayerRobot(DamageOfRobot2);
        //Robot2Anim.CrossFade("XBotHit", 0.1f);
        var hitEffect = Instantiate(HitEffect2);
        hitEffect.transform.position = HitEffect2.transform.position;
        hitEffect.SetActive(true);
        Destroy(hitEffect.gameObject, 2);
    }

    private void KillEnemy()
    {
        if (enemyDead)
        {
            return;
        }

        GameGUI.KillEnemy();
        currentEnemy = (currentEnemy + 1) % maxEnemies;
        GameGUI.TargetEnemy(currentEnemy);
    }

    IEnumerator FinishLevel()
    {
        yield return new WaitForSeconds(GameGUI.SwapDuration);

        enemyDead = true;
        //Robot1Anim.CrossFade("YBotDie", 0.1f);
        GameGUI.CanSwapTiles = false;
        numLevel += 1;

        MenuGUI.gameObject.SetActive(true);
        MenuGUI.UnlockLevel(currentLevel + 1);
        MenuGUI.DisplayWin();

        LeaderboardManager.Instance.SaveScore(score);
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

        ProcessTheHints();
        ProcessSpecialMatching((int)hints[0][0].x == (int)hints[0][1].x, (int)hints[0][0].x, (int)hints[0][0].y);
        ProcessScrolling();
        HitEnemy();

        // final step: test if new blocks appeared
        ProcessNewlyAppearedBlocks(tilesToPut);
    }

    private void ProcessTheHints()
    {
        bool vertical;
        string firstOne;
        for (int selected = 0; selected < hints.Count; selected++)
        {
            firstOne = tileSet[(int)hints[selected][0].x, (int)hints[selected][0].y];
            vertical = hints[selected][0].x == hints[selected][1].x;

            //Debug.Log("==============");
            if (!vertical)
            {
                // explode towards left
                for (int indx = (int)hints[selected][0].x - 1; indx >= 0; indx--)
                {
                    if (tileSet[indx, (int)hints[selected][0].y] != firstOne)
                    {
                        break;
                    }

                    GameGUI.ExplodeTile(indx, (int)hints[selected][0].y, false);
                    tileSet[indx, (int)hints[selected][0].y] = "X";
                }

                // explode towards right
                for (int indx = (int)hints[selected][0].x + 1; indx < levelInfo.Width; indx++)
                {
                    if (tileSet[indx, (int)hints[selected][0].y] != firstOne)
                    {
                        break;
                    }

                    GameGUI.ExplodeTile(indx, (int)hints[selected][0].y, false);
                    tileSet[indx, (int)hints[selected][0].y] = "X";
                }

                // self explode
                GameGUI.ExplodeTile((int)hints[selected][0].x, (int)hints[selected][0].y, true);
                tileSet[(int)hints[selected][0].x, (int)hints[selected][0].y] = "X";

            }
            else
            {
                // explode towards bottom
                int yIndex = 0;
                int yTop = (int)hints[selected][0].y;
                for (int indy = (int)hints[selected][0].y - 1; indy >= 0; indy--)
                {
                    if (tileSet[(int)hints[selected][0].x, indy] != firstOne)
                    {
                        break;
                    }

                    yIndex += 1;
                    GameGUI.ExplodeTile((int)hints[selected][0].x, indy, false);
                    tileSet[(int)hints[selected][0].x, indy] = "X";
                }

                // explode towards top
                for (int indy = (int)hints[selected][0].y + 1; indy < levelInfo.Height; indy++)
                {
                    if (tileSet[(int)hints[selected][0].x, indy] != firstOne)
                    {
                        break;
                    }

                    yIndex += 1;
                    yTop = indy;
                    GameGUI.ExplodeTile((int)hints[selected][0].x, indy, false);
                    tileSet[(int)hints[selected][0].x, indy] = "X";
                }

                GameGUI.ExplodeTile((int)hints[selected][0].x, (int)hints[selected][0].y, false);
                tileSet[(int)hints[selected][0].x, (int)hints[selected][0].y] = "X";
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
        Vector2 to;
        List<Vector2[]> tempHints = new List<Vector2[]>();
        for (int i = 0; i < tilesToSlide.Count; i++)
        {
            hints.Clear();
            to = tilesToSlide[i].To;
            // search for a special gem
            TestForAMatchAround((int)to.x, (int)to.y);
            for (int h = 0; h < hints.Count; h++)
            {
                tempHints.Add(hints[h]);
            }
        }

        for (int i = 0; i < tilesToPut.Count; i++)
        {
            hints.Clear();
            TestForAMatchAround((int)tilesToPut[i].x, (int)tilesToPut[i].y);
            for (int h = 0; h < hints.Count; h++)
            {
                tempHints.Add(hints[h]);
            }
        }

        if (tempHints.Count > 0)
        {
            hints.Clear();
            for (int i = 0; i < tempHints.Count; i++)
            {
                PrepareToProcessSpeacialMatching();
                hints.Add(tempHints[i]);
            }

            StartCoroutine(MoveOverAfter(0.5f));
        } else
        {
            ReleaseTiles();
        }
    }

    IEnumerator MoveOverAfter(float delay)
    {
        yield return new WaitForSeconds(delay);

        StartCoroutine(ExplodeTiles());
    }

    IEnumerator SwapTilesBackAndForthOnGUI(int x1, int y1, int x2, int y2)
    {
        GameGUI.SwapTiles(x1, y1, x2, y2, false);
        yield return new WaitForSeconds(GameGUI.SwapDuration);
        GameGUI.SwapTiles(x2, y2, x1, y1, false);
        yield return new WaitForSeconds(GameGUI.SwapDuration);

        ReleaseTiles();
    }

    IEnumerator SwapTilesOnceOnGUI(int x1, int y1, int x2, int y2)
    {
        GameGUI.SwapTiles(x1, y1, x2, y2, true);
        yield return new WaitForSeconds(GameGUI.SwapDuration);

        //ReleaseTiles();
        releaseTileX = -1;
        releaseTileY = -1;
    }

    public void ReleaseTiles()
    {
        StartCoroutine(ReleaseTilesNow());
    }

    IEnumerator ReleaseTilesNow()
    {
        yield return new WaitForSeconds(GameGUI.SwapDuration * 1.25f);

        GameGUI.CanSwapTiles = true;
        releaseTileX = -1;
        releaseTileY = -1;
    }

    private void DisplayDebugHints()
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
    }

    public void SetEnemy(int currentEmeny)
    {
        this.currentEnemy = currentEmeny;
    }

    int GetNextTile()
    {
        // regular process
        return UnityEngine.Random.Range(0, availabletiles.Count);

        // deterministic process
        //int[] nextOnes = new int[] { 6, 3, 6, 0, /**/ 3, 4, 0, /**/ 4, 6, 1, /**/ 6, 6, 4, 6, 4 /**/, 3, 0, 4, 3, 1, 3 /**/, 3, 0, 6, 4 };

        /*try
        {
            return nextOnes[nextCount++];
        }
        catch
        {
            Debug.LogWarning("!!");
            return UnityEngine.Random.Range(0, availabletiles.Count);
        }*/
    }

    IEnumerator ProcessSpecialGem(int x, int y, int releaseX, int releaseY)
    {
        bool horizontal = x != releaseX;

        yield return new WaitForSeconds(GameGUI.SwapDuration);

        switch (tileSet[x, y])
        {
            case "S1":
                GameGUI.LineDestroyEffect(x, y, !horizontal);
                yield return new WaitForSeconds(0.4f);

                ProcessLineBlast(x, y, horizontal, tileSet[releaseX, releaseY] == "S4");
                break;
            case "S2":
                GameGUI.ColorBlastEffect(x, y);
                yield return new WaitForSeconds(0.51f);

                ProcessColorBlast(x, y, tileSet[releaseX, releaseY]);
                break;
            case "S3":

                break;
            case "S4":
                break;
            case "S5":
                List<Vector2> changedTiles = ProcessRandomColorChange(x, y, tileSet[releaseX, releaseY]);
                GameGUI.ColorChangeEffect(tileSet[releaseX, releaseY], changedTiles);

                break;
        }

        tilesSpecialCoords.Clear();
        tilesSpecial.Clear();

        ProcessScrolling();
        HitEnemy();
        //ReleaseTiles();

        ProcessNewlyAppearedBlocks(tilesToPut);
    }

    private List<Vector2> ProcessRandomColorChange(int x, int y, string code)
    {
        List<Vector2> result = new List<Vector2>();

        // change numElements different elements from different colors to the ones which are in the long T6
        int numElements = UnityEngine.Random.Range(5, 7);
        int _x;
        int _y;
        bool overlapPresent;
        for (int i = 0; i < numElements; i++)
        {
            _x = UnityEngine.Random.Range(0, levelInfo.Width);
            _y = UnityEngine.Random.Range(0, levelInfo.Height);
            overlapPresent = false;
            do
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
            tileSet[_x, _y] = tileSet[x, y];
            result.Add(new Vector2(_x, _y));
        }

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
                GameGUI.ExplodeTile(indx, y, false);
                tileSet[indx, y] = "X";
            }
        }

        if (!horizontal || bothDir)
        {
            // explode vertical
            for (int indx = 0; indx < levelInfo.Height; indx++)
            {
                if(bothDir && indx == y)
                {
                    continue;
                }

                GameGUI.ExplodeTile(x, indx, false);
                tileSet[x, indx] = "X";
            }
        }
    }

    public long GetScore()
    {
        return score;
    }

    public long IncrementScore(int score)
    {
        this.score += score;

        return this.score;
    }
}