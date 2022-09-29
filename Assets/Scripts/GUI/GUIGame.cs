using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using static LevelManager;
using Unity.VisualScripting;
using TMPro;

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
    public RobotEffects[] EnemyRobots;
    public Slider[] EnemyGauges;
    public RobotEffects[] PlayerRobots;
    public Slider[] PlayerGauges;
    public int TopBias = 100;
    [HideInInspector]
    public bool CanSwapTiles = true;
    public TextMeshProUGUI TxtScore;

    Image[,] backgroundTiles;
    GamePlayManager gamePlayManager;
    LevelInformation levelInfo;
    SkinManager skinManager;
    List<GameObject> explosionEffects = new List<GameObject>();
    int currentEnemy = 0;
    int currentPlayer = 0;

    public delegate void OnGUIEvent(object param);

    private void Awake()
    {
        gamePlayManager = FindObjectOfType<GamePlayManager>();
        skinManager = FindObjectOfType<SkinManager>();
    }

    public void DamageToPlayerRobot(float damage)
    {
        if (currentPlayer >= PlayerGauges.Length)
        {
            return;
        }

        PlayerGauges[currentPlayer].DOValue(PlayerGauges[currentPlayer].value - damage, SwapDuration);
        PlayerRobots[currentPlayer].Damage();
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

    public void SetRobotGauges(int[] values)
    {
        for (int g = 0; g < EnemyGauges.Length; g++)
        {
            EnemyGauges[g].maxValue = values[g];
        }
    }

    public void TargetEnemy(int currentEnemy)
    {
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
                child.gameObject.name != "TxtScore")
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
        CanSwapTiles = false;
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

    public void ExplodeTile(int x, int y, bool destroyTile)
    {
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
            DebugTileList();
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
            GameObject colorChangingEffect = Instantiate(ColorChangingEffect);
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

            colorChangingEffect.transform.position = tile.position + new Vector3(0, 1, -5);
            colorChangingEffect.SetActive(true);

            Destroy(colorChangingEffect, 0.5f);

            Image tileImage = tile.GetComponent<Image>();
            if (tileImage == null)
            {
                tileImage = tile.AddComponent<Image>();
            }

            tileImage.sprite = skinManager.Skins[skinManager.SelectedSkin].FindSpriteFromKey(key);
        }
    }

}
