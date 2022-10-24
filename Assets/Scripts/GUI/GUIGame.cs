using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using static LevelManager;
using Unity.VisualScripting;
using TMPro;
using UnityEngine.Rendering;

public class GUIGame : MonoBehaviour
{
    public Image[] BackgroundTiles;
    public int Spacing = 4;
    public int TileWidth = 150;
    public float SwapDuration = 0.33f;
    public GameObject ExplosionEffect;
    public GameObject LineExplosionEffect;
    public GameObject ColorExplosionEffect;
    public GameObject ColorChangingEffect;
    public GameObject[] EnemyBullets;
    public RobotEffects[] EnemyRobots;
    public Slider[] EnemyGauges;
    public RobotEffects[] PlayerRobots;
    public Slider[] PlayerGauges;
    public int TopBias = 100;
    [HideInInspector]
    public bool CanSwapTiles = true;
    public TextMeshProUGUI TxtScore;
    public Sprite[] RobotSprites;
    public Transform WinDialogImage;

    Image[,] backgroundTiles;
    GamePlayManager gamePlayManager;
    LevelInformation levelInfo;
    SkinManager skinManager;
    List<GameObject> explosionEffects = new List<GameObject>();
    int currentEnemy = 0;
    int currentPlayer = 0;
    string lastLockedBy = "";

    public delegate void OnGUIEvent(object param);

    private void Awake()
    {
        gamePlayManager = FindObjectOfType<GamePlayManager>();
        skinManager = FindObjectOfType<SkinManager>();
    }

    public void DamageToPlayerRobot(float damage)
    {
        if (damage >= PlayerGauges[currentPlayer].value)
        {
            // current robot is dead
            PlayerGauges[currentPlayer].DOValue(0, SwapDuration);
            PlayerRobots[currentPlayer].Die();
            currentPlayer += 1;
        }

        if (PlayerGauges[0].value + PlayerGauges[1].value + PlayerGauges[2].value <= 0) //(currentPlayer >= PlayerGauges.Length)
        {
            gamePlayManager.ResetHintTime();
            DisplayLose();
            return;
        }

        PlayerGauges[currentPlayer].DOValue(PlayerGauges[currentPlayer].value - damage, SwapDuration);

        GameObject bullet = Instantiate(EnemyBullets[currentEnemy], EnemyBullets[currentEnemy].transform.parent);
        bullet.transform.position = EnemyBullets[currentEnemy].transform.position;
        bullet.gameObject.SetActive(true);
        bullet.transform.DOMove(new Vector3(PlayerRobots[currentPlayer].transform.position.x, PlayerRobots[currentPlayer].transform.position.y, bullet.transform.position.z), 0.25f).SetEase(Ease.Linear);
        StartCoroutine(HideAndDestroyAfter(bullet, 0.21f, 1));
    }

    private void DisplayLose()
    {
        WinDialogImage.gameObject.SetActive(true);
        Transform imgWin = WinDialogImage.transform.Find("ImgWin");
        Transform imgLose = WinDialogImage.transform.Find("ImgLose");
        imgWin.gameObject.SetActive(false);
        imgLose.gameObject.SetActive(true);

        imgLose.transform.localScale = Vector3.zero;
        imgLose.transform.DOScale(Vector3.one, 0.5f);
    }

    IEnumerator HideAndDestroyAfter(GameObject target, float timeToHide, float timeToDestroy)
    {
        yield return new WaitForSeconds(timeToHide);
        PlayerRobots[currentPlayer].Damage();

        target.SetActive(false);

        yield return new WaitForSeconds(timeToDestroy);

        Destroy(target);
    }

    public void DamageToEnemyRobot(float damage)
    {
        if(currentEnemy >= EnemyGauges.Length)
        {
            return;
        }

        EnemyGauges[currentEnemy].DOValue(EnemyGauges[currentEnemy].value - damage, SwapDuration);
        EnemyRobots[currentEnemy].Damage();

        TxtScore.text = gamePlayManager.IncrementScore(Mathf.FloorToInt(damage * 10)).ToString().PadLeft(6, '0');
    }

    public void KillEnemy()
    {
        EnemyRobots[currentEnemy].Die();
    }

    public void SetPlayerGauges()
    {
        for (int g = 0; g < PlayerGauges.Length; g++)
        {
            PlayerGauges[g].value = PlayerGauges[g].maxValue;
        }
    }

    public void SetRobotGauges(int[] values)
    {
        for (int g = 0; g < EnemyGauges.Length; g++)
        {
            EnemyGauges[g].maxValue = values[g];
        }
    }

    public void TargetEnemy(int currentEnemy)
    {
        if (!CanSwapTiles)
        {
            return;
        }

        if(EnemyGauges[currentEnemy].value <= 0)
        {
            return;
        }

        this.currentEnemy = currentEnemy;
        gamePlayManager.SetEnemy(currentEnemy);
        for (int r = 0; r < EnemyRobots.Length; r++)
        {
            if (r == currentEnemy)
            {
                EnemyRobots[r].SetTarget();
            } else
            {
                EnemyRobots[r].ClearTarget();
            }
        }
    }

    internal void InitializeEnemyRobots()
    {
        for (int i = 0; i < EnemyRobots.Length; i++)
        {
            EnemyRobots[i].Initialize();
        }
    }

    public void RenderLevelBackground(LevelInformation levelInfo)
    {
        for (int g = 0; g < EnemyGauges.Length; g++)
        {
            EnemyGauges[g].value = EnemyGauges[g].maxValue;
        }

        this.levelInfo = levelInfo;

        // remove the old ones
        Transform child;
        for (int i = 1; i < gameObject.transform.childCount; i++)
        {
            child = gameObject.transform.GetChild(i);
            if (!child.gameObject.name.StartsWith("Sld") && child.gameObject.name != "ImgBottom" &&
                !child.gameObject.name.StartsWith("ImgPlayerRobot") && !child.gameObject.name.StartsWith("BackgroundTile") &&
                !child.gameObject.name.StartsWith("Robot") && !child.gameObject.name.StartsWith("UI") &&
                child.gameObject.name != "TxtScore" && child.gameObject.name != "TxtStatus")
            {
                Destroy(gameObject.transform.GetChild(i).gameObject);
            }
        }

        // add new tiles
        backgroundTiles = new Image[levelInfo.Width,levelInfo.Height];
        Image backgroundTile;
        for (int i = 0; i < BackgroundTiles.Length; i++)
        {
            BackgroundTiles[i].GetComponent<RectTransform>().sizeDelta = new Vector2(TileWidth, TileWidth);
        }

        int tileWidth = TileWidth;
        int tileHeight = TileWidth;
        int itemNumber = 0;

        for (int x = 0; x < levelInfo.Width; x++)
        {
            for (int y = 0; y < levelInfo.Height; y++)
            {
                backgroundTile = Instantiate(BackgroundTiles[itemNumber++ % BackgroundTiles.Length], gameObject.transform);
                backgroundTile.gameObject.SetActive(true);
                backgroundTile.GetComponent<Image>().enabled = true;
                backgroundTile.gameObject.name = "TileBackground_" + x + "_" + y;
                backgroundTile.GetComponent<RectTransform>().anchoredPosition = new Vector2(tileWidth / 2f - levelInfo.Width / 2f * tileWidth + x * tileWidth + x * Spacing, -levelInfo.Height / 2f * tileHeight + y * tileHeight + y * Spacing - TopBias);

                backgroundTiles[x, y] = backgroundTile;
            }

            itemNumber += 1;
        }
    }

    public void RenderTiles(string[,] tileSet, int width, int height)
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                RenderTileOnObject(i, j, tileSet);
            }
        }
    }

    void RenderTileOnObject(int i, int j, string[,] tileSet)
    {
        GameObject tile;
        Image tileImage;
        GUITile guiTile;

        ClearSubElements(backgroundTiles[i, j].transform);
        tile = new GameObject();
        //tile.transform.SetParent(backgroundTiles[i, j].transform);
        tile.transform.SetParent(transform);
        tile.name = "Tile_" + i + "_" + j;

        //yield return new WaitForEndOfFrame();

        RectTransform rect = tile.AddComponent<RectTransform>();
        rect.anchoredPosition3D = new Vector3(TileWidth / 2f - levelInfo.Width / 2f * TileWidth + i * TileWidth + i * Spacing, -levelInfo.Height / 2f * TileWidth + j * TileWidth + j * Spacing - TopBias, 0);
        rect.sizeDelta = new Vector2(TileWidth, TileWidth);
        rect.localScale = new Vector3(1, 1, 1);

        tileImage = tile.AddComponent<Image>();
        tileImage.sprite = skinManager.Skins[skinManager.SelectedSkin].FindSpriteFromKey(tileSet[i, j]);

        guiTile = tile.AddComponent<GUITile>();
        guiTile.X = i;
        guiTile.Y = j;
        guiTile.Key = tileSet[i, j];
    }

    private void ClearSubElements(Transform transform)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }

    public void SwapTiles(int x1, int y1, int x2, int y2, bool changeInfo)
    {
        if(!(x1 == x2 || y1 == y2))
        {
            return;
        }

        StartCoroutine(SwapTilesNow(x1, y1, x2, y2, changeInfo));
    }

    IEnumerator SwapTilesNow(int x1, int y1, int x2, int y2, bool changeInfo)
    {
        LockTiles("L1");
        Transform tile1 = transform.Find("Tile_" + x1 + "_" + y1);
        Transform tile2 = transform.Find("Tile_" + x2 + "_" + y2);

        Vector2 tile1Pos = tile1.GetComponent<RectTransform>().anchoredPosition;
        Vector2 tile2Pos = tile2.GetComponent<RectTransform>().anchoredPosition;
        tile1.GetComponent<RectTransform>().DOAnchorPos(tile2Pos, SwapDuration);
        tile2.GetComponent<RectTransform>().DOAnchorPos(tile1Pos, SwapDuration);

        yield return new WaitForSeconds(SwapDuration);

        tile1.GetComponent<RectTransform>().DOAnchorPos(tile2Pos, 0);
        tile2.GetComponent<RectTransform>().DOAnchorPos(tile1Pos, 0);

        if (changeInfo)
        {
            tile1.gameObject.name = "Tile_" + x2 + "_" + y2;
            tile2.gameObject.name = "Tile_" + x1 + "_" + y1;
            tile1.GetComponent<GUITile>().X = x2;
            tile1.GetComponent<GUITile>().Y = y2;
            tile2.GetComponent<GUITile>().X = x1;
            tile2.GetComponent<GUITile>().Y = y1;
        }
    }

    public void LockTiles(string lockSource)
    {
        lastLockedBy = lockSource;
        CanSwapTiles = false;
    }

    public void ExplodeTile(int x, int y, bool destroyTile)
    {
        if (gamePlayManager.GetEnemyDead())
        {
            return;
        }

        GameObject explosionEffect = InstantiateOrReuseExplosion();
        Transform tile = transform.Find("Tile_" + x + "_" + y);
        
        // TODO: Remove in the future versions
        if(tile == null)
        {
            return;
        }

        explosionEffect.transform.position = tile.position + new Vector3(0, 1, -5);
        explosionEffect.SetActive(true);

        StartCoroutine(DespawnExplosion(explosionEffect));

        Transform tileToDisactivate = transform.Find("Tile_" + x + "_" + y);
        tileToDisactivate.gameObject.SetActive(false); //transform.localScale = Vector3.zero;
        tileToDisactivate.name += "_deleted";

        gamePlayManager.StartHintingCountDown();
    }

    IEnumerator DespawnExplosion(GameObject effect)
    {
        yield return new WaitForSeconds(1);

        effect.SetActive(false);
    }

    private GameObject InstantiateOrReuseExplosion()
    {
        for (int i = 0; i < explosionEffects.Count; i++)
        {
            if (explosionEffects[i] != null && !explosionEffects[i].activeSelf)
            {
                return explosionEffects[i];
            }
        }

        GameObject newExplosion = Instantiate(ExplosionEffect);
        explosionEffects.Add(newExplosion);

        return newExplosion;
    }

    public void ScrollTileDown(int x, int y, int howMany)
    {
        StartCoroutine(ScrollTileDownNow(x, y, howMany));
    }

    IEnumerator ScrollTileDownNow(int x, int y, int howMany)
    {
        Transform tile = transform.Find("Tile_" + x + "_" + y);

        // TODO: Remove in future versions
        if(tile == null)
        {
            yield break;
        }

        Vector2 tilePos = tile.GetComponent<RectTransform>().anchoredPosition;
        tile.GetComponent<RectTransform>().DOAnchorPos(tilePos + Vector2.down * TileWidth * howMany + Vector2.down * howMany * Spacing, SwapDuration);

        yield return new WaitForSeconds(SwapDuration);

        tile.gameObject.name = "Tile_" + x + "_" + (y - howMany);
        tile.GetComponent<GUITile>().X = x;
        tile.GetComponent<GUITile>().Y = y - howMany;
    }

    // very similar to the AppearAt but replaces the existing one instead
    internal void ReappearAt(int x, int y, string key)
    {
        Transform trans = transform.Find("Tile_" + x + "_" + y);
        if(trans != null)
        {
            Destroy(trans.gameObject);
        }
        
        AppearAt(x, y, key);
    }

    internal void AppearAt(int x, int y, string key)
    {
        StartCoroutine(AppearDelayed(x, y, key));
    }

    IEnumerator AppearDelayed(int x, int y, string key)
    {
        yield return new WaitForSeconds(SwapDuration);

        Transform trans = transform.Find("Tile_" + x + "_" + y + "_deleted");
        GameObject tile;

        if(trans == null)
        {
            tile = new GameObject();
        } else
        {
            tile = trans.gameObject;
        }

        //tile.transform.SetParent(backgroundTiles[i, j].transform);
        tile.transform.SetParent(transform);
        tile.name = "Tile_" + x + "_" + y;
        tile.SetActive(true);

        RectTransform rect = tile.GetComponent<RectTransform>();
        if (rect == null)
        {
            rect = tile.AddComponent<RectTransform>();
        }

        rect.anchoredPosition3D = new Vector3(TileWidth / 2f - levelInfo.Width / 2f * TileWidth + x * TileWidth + x * Spacing, -levelInfo.Height / 2f * TileWidth + y * TileWidth + y * Spacing - TopBias, 0);
        rect.sizeDelta = new Vector2(TileWidth, TileWidth);
        rect.localScale = new Vector3(1, 1, 1);
        rect.localScale = Vector3.zero;
        rect.DOScale(Vector3.one, SwapDuration);

        Image tileImage = tile.GetComponent<Image>();
        if(tileImage == null)
        {
            tileImage = tile.AddComponent<Image>();
        }

        tileImage.sprite = skinManager.Skins[skinManager.SelectedSkin].FindSpriteFromKey(key);
        tileImage.DOFade(0, 0);
        tileImage.DOFade(1, SwapDuration);

        GUITile guiTile = tile.GetComponent<GUITile>();
        if (guiTile == null)
        {
            guiTile = tile.AddComponent<GUITile>();
        }

        guiTile.X = x;
        guiTile.Y = y;
        guiTile.Key = key;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.F1))
        {
            //DamageToPlayerRobot(10);
        }
    }

    private void DebugTileList()
    {
        string line;

        for (int j = levelInfo.Height - 1; j >= 0; j--)
        {
            line = "";
            for (int i = 0; i < levelInfo.Width; i++)
            {
                line += gamePlayManager.TileSet[i, j];

                if(i != levelInfo.Width - 1)
                {
                    line += ", ";
                }
            }

            Debug.Log(line);
        }
    }

    public void LineDestroyEffect(int x, int y, bool vertical)
    {
        GameObject explosionEffect1 = Instantiate(LineExplosionEffect);
        GameObject explosionEffect2 = Instantiate(LineExplosionEffect);
        Transform tile = transform.Find("Tile_" + x + "_" + y);

        // TODO: Remove in the future versions
        if (tile == null)
        {
            tile = transform.Find("Tile_" + x + "_" + y + "_deleted");
            if (tile == null)
            {
                Debug.LogWarning("Line destroy effect failed to find the tile Tile_" + x + "_" + y);
                return;
            }
        }

        explosionEffect1.transform.position = tile.position + new Vector3(0, 1, -5);
        explosionEffect1.SetActive(true);
        explosionEffect1.transform.DOMove(explosionEffect1.transform.position + (vertical ? Vector3.up * 35 : Vector3.left * 35), 0.33f).SetEase(Ease.Linear);

        explosionEffect2.transform.position = tile.position + new Vector3(0, 1, -5);
        explosionEffect2.SetActive(true);
        explosionEffect2.transform.DOMove(explosionEffect1.transform.position + (vertical ? Vector3.up * -35 : Vector3.left * -35), 0.33f).SetEase(Ease.Linear);

        Destroy(explosionEffect1, 0.35f);
        Destroy(explosionEffect2, 0.35f);
    }

    public void ColorBlastEffect(int x, int y)
    {
        GameObject colorExplosionEffect = Instantiate(ColorExplosionEffect);
        Transform tile = transform.Find("Tile_" + x + "_" + y);

        if (tile == null)
        {
            tile = transform.Find("Tile_" + x + "_" + y + "_deleted");
            if (tile == null)
            {
                Debug.LogWarning("Color blast effect failed to find the tile Tile_" + x + "_" + y);
                return;
            }
        }

        colorExplosionEffect.transform.position = tile.position + new Vector3(0, 1, -5);
        colorExplosionEffect.SetActive(true);

        Destroy(colorExplosionEffect, 0.5f);
    }

    public void ColorChangeEffect(string key, List<Vector2> changedTiles)
    {
        int x, y;
        for (int i = 0; i < changedTiles.Count; i++)
        {
            x = (int)changedTiles[i].x;
            y = (int)changedTiles[i].y;
            Transform tile = transform.Find("Tile_" + x + "_" + y);

            if (tile == null)
            {
                tile = transform.Find("Tile_" + x + "_" + y + "_deleted");
                if (tile == null)
                {
                    Debug.LogWarning("Color change effect failed to find the tile Tile_" + x + "_" + y);
                    return;
                }
            }

            Image tileImage = tile.GetComponent<Image>();
            if (tileImage == null)
            {
                tileImage = tile.AddComponent<Image>();
            }
            
            var duplicate = Instantiate(tile, tile.transform.parent);
            duplicate.transform.SetAsLastSibling();
            var dimage = duplicate.GetComponent<Image>();
            dimage.sprite = skinManager.Skins[skinManager.SelectedSkin].FindSpriteFromKey(key);

            StartCoroutine(ChangeColor(tileImage, dimage, key));
            Destroy(duplicate.gameObject, 1f);
        }
    }

    IEnumerator ChangeColor(Image tileImage, Image dimage, string key)
    {
        dimage.color = new Color(1, 1, 1, 0);
        dimage.DOFade(1, 0.9f);

        yield return new WaitForSeconds(0.95f);
        tileImage.sprite = skinManager.Skins[skinManager.SelectedSkin].FindSpriteFromKey(key);
        dimage.gameObject.SetActive(false);
    }

    public void StartNextWave()
    {
        LockTiles("L0");

        StartCoroutine(SwapWaves());
    }

    IEnumerator SwapWaves()
    {
        for (int i = 0; i < EnemyRobots.Length; i++)
        {
            EnemyRobots[i].FadeOut();// GetComponent<Image>().DOFade(0, 0.3f).SetEase(Ease.Linear);
        }

        yield return new WaitForSeconds(0.67f);

        for (int g = 0; g < EnemyGauges.Length; g++)
        {
            EnemyGauges[g].value = EnemyGauges[g].maxValue;
        }

        for (int i = 0; i < EnemyRobots.Length; i++)
        {
            EnemyRobots[i].FadeIn(); // GetComponent<Image>().DOFade(1, 0.3f).SetEase(Ease.Linear);
        }

        yield return new WaitForSeconds(0.3f);

        CanSwapTiles = true;
    }

    public void SetRobots(int robot1, int robot2, int robot3)
    {
        transform.Find("ImgPlayerRobot1").GetComponent<Image>().sprite = RobotSprites[robot1];
        transform.Find("ImgPlayerRobot2").GetComponent<Image>().sprite = RobotSprites[robot2];
        transform.Find("ImgPlayerRobot3").GetComponent<Image>().sprite = RobotSprites[robot3];
    }

    public void DisplayHintAt(int x1, int y1)
    {
        if (gamePlayManager.GetEnemyDead())
        {
            return;
        }

        StartCoroutine(DisplayHintAtNow(x1, y1));
    }

    IEnumerator DisplayHintAtNow(int x1, int y1)
    {
        Transform tile1 = transform.Find("Tile_" + x1 + "_" + y1);

        if (tile1 == null)
        {
            tile1 = transform.Find("Tile_" + x1 + "_" + y1 + "_deleted");
            if (tile1 == null)
            {
                Debug.LogWarning("Color blast effect failed to find the tile Tile_" + x1 + "_" + y1);
                yield break;
            }
        }

        float _x1 = tile1.GetComponent<RectTransform>().anchoredPosition.x;
        float _y1 = tile1.GetComponent<RectTransform>().anchoredPosition.y;

        for (int i = 0; i < 8; i++)
        {
            if (!CanSwapTiles)
            {
                tile1.GetComponent<RectTransform>().anchoredPosition = new Vector2(_x1, _y1);
                yield break;
            }

            tile1.GetComponent<RectTransform>().DOAnchorPos(new Vector2(_x1 + (i % 2 == 0 ? 1 : -1) * UnityEngine.Random.Range(0f, 10f), _y1 + (i % 2 == 0 ? 1 : -1) * UnityEngine.Random.Range(0f, 10f)), 0.15f).SetEase(Ease.Linear);
            yield return new WaitForSeconds(0.15f);
        }

        tile1.GetComponent<RectTransform>().DOAnchorPos(new Vector2(_x1, _y1), 0.15f).SetEase(Ease.Linear);
    }

    public void DisplayDebug()
    {
        Debug.Log("DateTimeNow: " + DateTime.Now);
        Debug.Log("CanSwapTiles: " + CanSwapTiles);
        Debug.Log("TimeForNewHint: " + gamePlayManager.GetTimeForNewHint());
        Debug.Log("LastLockedBy: " + lastLockedBy);
        Debug.Log("DebugTileList: ");

        DebugTileList();
    }
}
