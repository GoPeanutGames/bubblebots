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
    List<Vector2[]> hints = new List<Vector2[]>();
    List<int> availabletiles = new List<int>();
    int releaseTileX = -1;
    int releaseTileY = -1;
    bool canSwapTiles = true;
    int numHit = 0;
    int numLevel = 1;

    public string[,] TileSet
    {
        get
        {
            return tileSet;
        }
    }

    public void PrepareLevel(string levelFile)
    {
        MenuGUI.SwitchToMultiplayer(levelFile);
    }

    public void StartLevel(string levelFile)
    {
        LevelInformation levelInfo;

        try
        {
            levelInfo = levelManager.LoadLevel(levelFile);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
            return;
        }

        RenderLevel(levelInfo);
        numHit = 0;
        Robot1Anim.CrossFade("YBotIdle", 0.1f);
        Robot2Anim.CrossFade("XBotIdle", 0.1f);
    }

    void RenderLevel(LevelInformation levelInfo)
    {
        this.levelInfo = levelInfo;
        GameGUI.RenderLevel(levelInfo);

        do
        {
            FillTiles(levelInfo);
        } while (!MoreMovesArePossible());

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

        int index;
        for (int x = 0; x < levelInfo.Width; x++)
        {
            for (int y = 0; y < levelInfo.Height; y++)
            {
                index = availabletiles[UnityEngine.Random.Range(0, availabletiles.Count)];
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

        SwapKeys(x1,y1,x2,y2);
        TestForAMatchAround(x1, y1, x2, y2);
        SwapKeys(x1, y1, x2, y2);
    }

    private void TestForAMatchAround(int x, int y, int xo, int yo)
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

        releaseTileX = x; //>= 0 && x < levelInfo.Width ? x : 0;
        releaseTileY = y; //>= 0 && y < levelInfo.Height ? y : 0;
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

            hints.Clear();
            CheckForAMatchWitchSwapingTiles(x, y, releaseTileX, releaseTileY);

            if(hints.Count > 0)
            {
                Debug.Log("Swap tiles (101): " + x + "," + y + " with " + releaseTileX + "," + releaseTileY);
                DisplayDebugHints();
                //Debug.Break();

                StartCoroutine(SwapTilesOnceOnGUI(x, y, releaseTileX, releaseTileY));
                SwapKeys(x, y, releaseTileX, releaseTileY);
                StartCoroutine(ExplodeTiles());
                HitEnemy();
            } else
            {
                CheckForAMatchWitchSwapingTiles(releaseTileX, releaseTileY, x, y);

                if (hints.Count > 0)
                {
                    Debug.Log("Swap tiles (102): " + x + "," + y + " with " + releaseTileX + "," + releaseTileY);
                    DisplayDebugHints();
                    //Debug.Break();

                    StartCoroutine(SwapTilesOnceOnGUI(x, y, releaseTileX, releaseTileY));
                    SwapKeys(x, y, releaseTileX, releaseTileY);
                    StartCoroutine(ExplodeTiles());
                    HitEnemy();
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
        if (++numHit >= EnemyHP)
        {
            KillEnemy();
        }

        if(numHit % 2 == 0)
        {
            HitPlayer();
        }
    }

    private void HitPlayer()
    {
        GameGUI.DamageToRobot1(DamageOfRobot2);
        Robot2Anim.CrossFade("XBotHit", 0.1f);
        var hitEffect = Instantiate(HitEffect2);
        hitEffect.transform.position = HitEffect2.transform.position;
        hitEffect.SetActive(true);
        Destroy(hitEffect, 2);
    }

    private void KillEnemy()
    {
        Robot1Anim.CrossFade("YBotDie", 0.1f);
        canSwapTiles = false;
        numLevel += 1;

        MenuGUI.gameObject.SetActive(true);
        MenuGUI.UnlockLevel(numLevel);
        MenuGUI.DisplayWin();
    }

    IEnumerator ExplodeTiles()
    {
        //DisplayDebugHints();
        yield return new WaitForSeconds(GameGUI.SwapDuration);

        bool vertical;
        int newIndex;
        DisplayDebugHints();
        int longest = 0;
        int selected = 0;
        
        for (int i = 0; i < hints.Count; i++)
        {
            if(hints[i].Length > longest)
            {
                longest = hints[i].Length;
                selected = i;
            }
        }
        
        vertical = hints[selected][0].x == hints[selected][1].x;

        //Debug.Log("==============");
        if (!vertical)
        {
            // explode towards left
            for (int indx = (int)hints[selected][0].x - 1; indx >= 0; indx--)
            {
                if (tileSet[indx, (int)hints[selected][0].y] != tileSet[(int)hints[selected][0].x, (int)hints[selected][0].y])
                {
                    break;
                }

                GameGUI.ExplodeTile(indx, (int)hints[selected][0].y, false);

                for (int y = (int)hints[selected][0].y; y < levelInfo.Height; y++)
                {
                    GameGUI.ScrollTileDown(indx, y, 1);
                    if (y < levelInfo.Height - 1)
                    {
                        tileSet[indx, y] = tileSet[indx, y + 1];
                    }
                }

                newIndex = availabletiles[UnityEngine.Random.Range(0, availabletiles.Count)];
                tileSet[indx, levelInfo.Height - 1] = skinManager.Skins[skinManager.SelectedSkin].TileSet[newIndex].Key;
                GameGUI.AppearAt(indx, levelInfo.Height - 1, tileSet[indx, levelInfo.Height - 1]);
            }

            // explode towards right
            for (int indx = (int)hints[selected][0].x + 1; indx < levelInfo.Width; indx++)
            {
                if (tileSet[indx, (int)hints[selected][0].y] != tileSet[(int)hints[selected][0].x, (int)hints[selected][0].y])
                {
                    break;
                }

                GameGUI.ExplodeTile(indx, (int)hints[selected][0].y, false);

                for (int y = (int)hints[selected][0].y; y < levelInfo.Height; y++)
                {
                    GameGUI.ScrollTileDown(indx, y, 1);
                    if (y < levelInfo.Height - 1)
                    {
                        tileSet[indx, y] = tileSet[indx, y + 1];
                    }
                }

                newIndex = availabletiles[UnityEngine.Random.Range(0, availabletiles.Count)];
                tileSet[indx, levelInfo.Height - 1] = skinManager.Skins[skinManager.SelectedSkin].TileSet[newIndex].Key;
                GameGUI.AppearAt(indx, levelInfo.Height - 1, tileSet[indx, levelInfo.Height - 1]);
            }

            // self explode
            GameGUI.ExplodeTile((int)hints[selected][0].x, (int)hints[selected][0].y, true);

            for (int y = (int)hints[selected][0].y; y < levelInfo.Height; y++)
            {
                GameGUI.ScrollTileDown((int)hints[selected][0].x, y, 1);
                if (y < levelInfo.Height - 1)
                {
                    tileSet[(int)hints[selected][0].x, y] = tileSet[(int)hints[selected][0].x, y + 1];
                }
            }

            newIndex = availabletiles[UnityEngine.Random.Range(0, availabletiles.Count)];
            tileSet[(int)hints[selected][0].x, levelInfo.Height - 1] = skinManager.Skins[skinManager.SelectedSkin].TileSet[newIndex].Key;
            GameGUI.AppearAt((int)hints[selected][0].x, levelInfo.Height - 1, tileSet[(int)hints[selected][0].x, levelInfo.Height - 1]);
        }
        else
        {
            // explode towards bottom
            int yIndex = 0;
            int yTop = (int)hints[selected][0].y;
            for (int indy = (int)hints[selected][0].y - 1; indy >= 0; indy--)
            {
                if (tileSet[(int)hints[selected][0].x, indy] != tileSet[(int)hints[selected][0].x, (int)hints[selected][0].y])
                {
                    break;
                }

                yIndex += 1;
                GameGUI.ExplodeTile((int)hints[selected][0].x, indy, false);
            }

            // explode towards top
            for (int indy = (int)hints[selected][0].y + 1; indy < levelInfo.Height; indy++)
            {
                if (tileSet[(int)hints[selected][0].x, indy] != tileSet[(int)hints[selected][0].x, (int)hints[selected][0].y])
                {
                    break;
                }

                yIndex += 1;
                yTop = indy;
                GameGUI.ExplodeTile((int)hints[selected][0].x, indy, false);
            }

            GameGUI.ExplodeTile((int)hints[selected][0].x, (int)hints[selected][0].y, false);

            for (int y = yTop + 1; y < levelInfo.Height; y++)
            {
                GameGUI.ScrollTileDown((int)hints[selected][0].x, y, yIndex + 1);
                tileSet[(int)hints[selected][0].x, y - (yIndex + 1)] = tileSet[(int)hints[selected][0].x, y];
            }

            for (int y = 1; y <= yIndex + 1; y++)
            {
                newIndex = availabletiles[UnityEngine.Random.Range(0, availabletiles.Count)];
                tileSet[(int)hints[selected][0].x, levelInfo.Height - y] = skinManager.Skins[skinManager.SelectedSkin].TileSet[newIndex].Key;
                GameGUI.AppearAt((int)hints[selected][0].x, levelInfo.Height - y, tileSet[(int)hints[selected][0].x, levelInfo.Height - y]);
            }
        }
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
}
