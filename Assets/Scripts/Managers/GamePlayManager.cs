#define SPECIAL_MATCHING_OFF

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static LevelManager;

public class GamePlayManager : MonoBehaviour
{
    public LevelManager levelManager;
    public GUIMenu MenuGUI;
    public GUIGame GameGUI;
    public SkinManager skinManager;
    public float DamageOfRobot1 = 0.2f;
    public float DamageOfRobot2 = 0.2f;
    public Animator Robot1Anim;
    public Animator Robot2Anim;
    public GameObject HitEffect1;
    public GameObject HitEffect2;
    public int EnemyHP = 20;

    LevelInformation levelInfo;
    string[,] tileSet;
    List<Vector2> tilesToPut = new List<Vector2>();
    List<Vector2[]> hints = new List<Vector2[]>();
    List<int> availabletiles = new List<int>();
    int releaseTileX = -1;
    int releaseTileY = -1;
    bool canSwapTiles = true;
    bool enemyDead = false;
    int numHit = 0;
    int numLevel = 1;
    int currentLevel = 0;
    int nextCount = 0;

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
        numHit = 0;
        Robot1Anim.CrossFade("Idle", 0.1f);
        //Robot2Anim.CrossFade("Idle", 0.1f);
    }

    void RenderLevel(LevelInformation levelInfo)
    {
        this.levelInfo = levelInfo;
        GameGUI.RenderLevelBackground(levelInfo);

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
        /*tileIndices[0, 7] = 3;
        tileIndices[1, 7] = 6;
        tileIndices[2, 7] = 4;
        tileIndices[3, 7] = 3;
        tileIndices[4, 7] = 4;
        tileIndices[5, 7] = 0;
        tileIndices[6, 7] = 6;
        tileIndices[7, 7] = 0;

        tileIndices[0, 6] = 6;
        tileIndices[1, 6] = 0;
        tileIndices[2, 6] = 0;
        tileIndices[3, 6] = 6;
        tileIndices[4, 6] = 3;
        tileIndices[5, 6] = 6;
        tileIndices[6, 6] = 0;
        tileIndices[7, 6] = 3;

        tileIndices[0, 5] = 6;
        tileIndices[1, 5] = 4;
        tileIndices[2, 5] = 6;
        tileIndices[3, 5] = 1;
        tileIndices[4, 5] = 4;
        tileIndices[5, 5] = 3;
        tileIndices[6, 5] = 4;
        tileIndices[7, 5] = 0;

        tileIndices[0, 4] = 3;
        tileIndices[1, 4] = 4;
        tileIndices[2, 4] = 4;
        tileIndices[3, 4] = 6;
        tileIndices[4, 4] = 0;
        tileIndices[5, 4] = 0;
        tileIndices[6, 4] = 6;
        tileIndices[7, 4] = 1;

        tileIndices[0, 3] = 6;
        tileIndices[1, 3] = 3;
        tileIndices[2, 3] = 1;
        tileIndices[3, 3] = 0;
        tileIndices[4, 3] = 4;
        tileIndices[5, 3] = 6;
        tileIndices[6, 3] = 1;
        tileIndices[7, 3] = 3;

        tileIndices[0, 2] = 6;
        tileIndices[1, 2] = 0;
        tileIndices[2, 2] = 4;
        tileIndices[3, 2] = 0;
        tileIndices[4, 2] = 3;
        tileIndices[5, 2] = 0;
        tileIndices[6, 2] = 4;
        tileIndices[7, 2] = 1;

        tileIndices[0, 1] = 1;
        tileIndices[1, 1] = 0;
        tileIndices[2, 1] = 0;
        tileIndices[3, 1] = 3;
        tileIndices[4, 1] = 3;
        tileIndices[5, 1] = 6;
        tileIndices[7, 1] = 1;
        tileIndices[6, 1] = 3;

        tileIndices[0, 0] = 6;
        tileIndices[1, 0] = 3;
        tileIndices[2, 0] = 4;
        tileIndices[3, 0] = 6;
        tileIndices[4, 0] = 4;
        tileIndices[5, 0] = 4;
        tileIndices[6, 0] = 6;
        tileIndices[7, 0] = 6;*/

        int index;
        for (int x = 0; x < levelInfo.Width; x++)
        {
            for (int y = 0; y < levelInfo.Height; y++)
            {
                index = availabletiles[GetNextTile()];
                //index = tileIndices[x,y];
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

        // swapping a special tile?
        if (tileSet[x1, y1] == "s5" || tileSet[x2, y2] == "s4")
        {
            if (x1 == x2)
            {
                // vertical
            }
            else
            {
                // horizontal
            }
        }
        else
        {
            SwapKeys(x1, y1, x2, y2);
            TestForAMatchAround(x1, y1);
            TestForAMatchAround(x2, y2);
            SwapKeys(x1, y1, x2, y2);
        }
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
        if(!canSwapTiles)
        {
            return;
        }

        releaseTileX = x;
        releaseTileY = y;
    }

    public void MoveOverTile(int x, int y)
    {
        if (!canSwapTiles || (x == releaseTileX && y == releaseTileY) || releaseTileX == -1 || releaseTileY == -1)
        {
            return;
        }

        if (Mathf.Abs(x - releaseTileX) <= 1 && Mathf.Abs(y - releaseTileY) <= 1 && tileSet[x, y] != tileSet[releaseTileX, releaseTileY])
        {
            canSwapTiles = false;
#if SPECIAL_MATCHING
            bool sm5InLine = false;
            bool sm4InLine = false;
            bool smBigT = false;
#endif

            hints.Clear();
            CheckForAMatchWitchSwapingTiles(x, y, releaseTileX, releaseTileY);

            if(hints.Count > 0)
            {
#if SPECIAL_MATCHING
                if (hints[0].Length >= 5)
                {
                    Debug.Log("5 in line!");
                    sm5InLine = true;
                } else if (hints[0].Length == 4)
                {
                    Debug.Log("4 in line!");
                    sm4InLine = true;
                }
#endif

                StartCoroutine(SwapTilesOnceOnGUI(x, y, releaseTileX, releaseTileY));
                SwapKeys(x, y, releaseTileX, releaseTileY);
                StartCoroutine(ExplodeTiles());
                //HitEnemy();

#if SPECIAL_MATCHING
                if (sm5InLine)
                {
                    // place a special tile here
                    if(x != releaseTileX)
                    {
                        // place vertical
                        tileSet[releaseTileX + 2, releaseTileY] = "S5";
                        GameGUI.ReappearAt(releaseTileX + 2, releaseTileY, "S5");
                    } else
                    {
                        // place horizontal
                        tileSet[releaseTileX, releaseTileY + 2] = "S5";
                        GameGUI.ReappearAt(releaseTileX, releaseTileY + 2, "S5");
                    }

                    sm5InLine = false;
                }
                else if (sm4InLine)
                {
                    // place a special tile here
                    if (x != releaseTileX)
                    {
                        // place vertical
                        tileSet[releaseTileX + 2, releaseTileY] = "S4";
                        GameGUI.ReappearAt(releaseTileX + 2, releaseTileY, "S4");
                    }
                    else
                    {
                        // place horizontal
                        tileSet[releaseTileX, releaseTileY + 2] = "S4";
                        GameGUI.ReappearAt(releaseTileX, releaseTileY + 2, "S4");
                    }

                    sm4InLine = false;
                }
                else if (smBigT)
                {
                    // place a special tile here
                    if (x != releaseTileX)
                    {
                        // place vertical
                        tileSet[releaseTileX + 2, releaseTileY] = "ST";
                        GameGUI.ReappearAt(releaseTileX + 2, releaseTileY, "ST");
                    }
                    else
                    {
                        // place horizontal
                        tileSet[releaseTileX, releaseTileY + 2] = "ST";
                        GameGUI.ReappearAt(releaseTileX, releaseTileY + 2, "ST");
                    }

                    smBigT = false;
                }
#endif
            } else
            {
                CheckForAMatchWitchSwapingTiles(releaseTileX, releaseTileY, x, y);

                if (hints.Count > 0)
                {
#if SPECIAL_MATCHING
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
#endif
                    StartCoroutine(SwapTilesOnceOnGUI(x, y, releaseTileX, releaseTileY));
                    SwapKeys(x, y, releaseTileX, releaseTileY);
                    StartCoroutine(ExplodeTiles());
                    //HitEnemy();

#if SPECIAL_MATCHING
                    if (sm5InLine)
                    {
                        // place a special tile here
                        if (x != releaseTileX)
                        {
                            // place vertical
                            tileSet[releaseTileX + 2, releaseTileY] = "S5";
                            GameGUI.ReappearAt(releaseTileX + 2, releaseTileY, "S5");
                        }
                        else
                        {
                            // place horizontal
                            tileSet[releaseTileX, releaseTileY + 2] = "S5";
                            GameGUI.ReappearAt(releaseTileX, releaseTileY + 2, "S5");
                        }

                        sm5InLine = false;
                    }
                    else if (sm4InLine)
                    {
                        // place a special tile here
                        if (x != releaseTileX)
                        {
                            // place vertical
                            tileSet[releaseTileX + 2, releaseTileY] = "S4";
                            GameGUI.ReappearAt(releaseTileX + 2, releaseTileY, "S4");
                        }
                        else
                        {
                            // place horizontal
                            tileSet[releaseTileX, releaseTileY + 2] = "S4";
                            GameGUI.ReappearAt(releaseTileX, releaseTileY + 2, "S4");
                        }

                        sm4InLine = false;
                    }
                    else if (smBigT)
                    {
                        // place a special tile here
                        if (x != releaseTileX)
                        {
                            // place vertical
                            tileSet[releaseTileX + 2, releaseTileY] = "ST";
                            GameGUI.ReappearAt(releaseTileX + 2, releaseTileY, "ST");
                        }
                        else
                        {
                            // place horizontal
                            tileSet[releaseTileX, releaseTileY + 2] = "ST";
                            GameGUI.ReappearAt(releaseTileX, releaseTileY + 2, "ST");
                        }

                        smBigT = false;
                    }
#endif
                }
                else
                {
                    StartCoroutine(SwapTilesBackAndForthOnGUI(x, y, releaseTileX, releaseTileY));
                }
            }
        }
    }

    private void HitEnemy()
    {
        GameGUI.DamageToRobot2(DamageOfRobot1);
        Robot1Anim.CrossFade("YBotHit", 0.1f);
        var hitEffect = Instantiate(HitEffect1);
        hitEffect.transform.position = HitEffect1.transform.position;
        hitEffect.SetActive(true);
        Destroy(hitEffect, 2);

        numHit += 1;
        if (numHit % 2 == 0)
        {
            HitPlayer();
        }

        if (numHit >= EnemyHP)
        {
            KillEnemy();
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

        yield return new WaitForSeconds(1);

        RenderLevel(levelInfo);
        MenuGUI.HideNoMoreMoves();
    }

    private void HitPlayer()
    {
        GameGUI.DamageToRobot1(DamageOfRobot2);
        //Robot2Anim.CrossFade("XBotHit", 0.1f);
        var hitEffect = Instantiate(HitEffect2);
        hitEffect.transform.position = HitEffect2.transform.position;
        hitEffect.SetActive(true);
        Destroy(hitEffect, 2);
    }

    private void KillEnemy()
    {
        if(enemyDead)
        {
            return;
        }

        StartCoroutine(KillEnemyNow());
    }

    IEnumerator KillEnemyNow()
    {
        yield return new WaitForSeconds(GameGUI.SwapDuration);

        enemyDead = true;
        Robot1Anim.CrossFade("YBotDie", 0.1f);
        canSwapTiles = false;
        numLevel += 1;

        MenuGUI.gameObject.SetActive(true);
        MenuGUI.UnlockLevel(currentLevel + 1);
        MenuGUI.DisplayWin();
    }

    IEnumerator ExplodeTiles()
    {
        //DisplayDebugHints();
        yield return new WaitForSeconds(GameGUI.SwapDuration);
        List<SlideInformation> tilesToSlide = new List<SlideInformation>();
        tilesToPut.Clear();

        bool vertical;
        int newIndex;

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

        // process the hints
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

        // process scrolling
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
                        if(tileSet[x, sy] != "X")
                        {
                            foundAt = sy;
                            break;
                        }
                    }

                    if(foundAt == -1)
                    {
                        // fill from here until the top
                        for (int py = y; py < levelInfo.Height; py++)
                        {
                            newIndex = GetNextTile();
                            tileSet[x, py] = skinManager.Skins[skinManager.SelectedSkin].TileSet[newIndex].Key;
                            GameGUI.AppearAt(x, py, tileSet[x, py]);

                            AddTilesToPut(new Vector2(x, py));
                        }

                        break;
                    } else
                    {
                        // scroll the tile from x, foundAt to x, y
                        GameGUI.ScrollTileDown(x, foundAt, foundAt - y);
                        tileSet[x, y] = tileSet[x, foundAt];
                        tileSet[x, foundAt] = "X";

                        tilesToSlide.Add(new SlideInformation(new Vector2(x, y), new Vector2(x, foundAt)));
                    }
                }
            }
        }

        HitEnemy();

        // final step: test if new blocks appeared
        ProcessNewlyAppearedBlocks(tilesToSlide, tilesToPut);
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

    public void ProcessNewlyAppearedBlocks(List<SlideInformation> tilesToSlide, List<Vector2> tilesToPut)
    {
        Vector2 to;
        List<Vector2[]> tempHints = new List<Vector2[]>();
        for (int i = 0; i < tilesToSlide.Count; i++)
        {
            hints.Clear();
            to = tilesToSlide[i].To;
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
                hints.Add(tempHints[i]);
            }

            StartCoroutine(MoveOverAfter(0.5f));
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

        ReleaseTiles();
    }

    public void ReleaseTiles()
    {
        canSwapTiles = true;
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

            s += Environment.NewLine;
        }

        Debug.Log("They are: " + s);
    }

    private void Start()
    {
        GameGUI.SetRobotGauges(EnemyHP);
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
}
