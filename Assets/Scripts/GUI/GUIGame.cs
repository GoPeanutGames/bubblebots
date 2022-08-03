using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using static LevelManager;

public class GUIGame : MonoBehaviour
{
    public Image BackgroundTile;
    public int Spacing = 4;
    public int TileWidth = 150;
    public float SwapDuration = 0.33f;
    public GameObject ExplosionEffect;
    public Slider RobotGauge1;
    public Slider RobotGauge2;

    Image[,] backgroundTiles;
    GamePlayManager gamePlayManager;
    LevelInformation levelInfo;
    SkinManager skinManager;
    List<GameObject> explosionEffects = new List<GameObject>();

    private void Awake()
    {
        gamePlayManager = FindObjectOfType<GamePlayManager>();
        skinManager = FindObjectOfType<SkinManager>();
    }

    public void DamageToRobot1(float damage)
    {
        RobotGauge1.DOValue(RobotGauge1.value - damage, SwapDuration);
    }

    public void DamageToRobot2(float damage)
    {
        RobotGauge2.DOValue(RobotGauge2.value - damage, SwapDuration);
    }

    public void RenderLevel(LevelInformation levelInfo)
    {
        RobotGauge1.value = 1;
        RobotGauge2.value = 1;

        this.levelInfo = levelInfo;

        backgroundTiles = new Image[levelInfo.Width,levelInfo.Height];
        Image backgroundTile;
        BackgroundTile.GetComponent<RectTransform>().sizeDelta = new Vector2(TileWidth, TileWidth);
        int tileWidth = TileWidth; //(int)BackgroundTile.GetComponent<RectTransform>().sizeDelta.x;
        int tileHeight = TileWidth; //(int)BackgroundTile.GetComponent<RectTransform>().sizeDelta.y;

        for (int x = 0; x < levelInfo.Width; x++)
        {
            for (int y = 0; y < levelInfo.Height; y++)
            {
                backgroundTile = Instantiate(BackgroundTile, gameObject.transform);
                backgroundTile.gameObject.SetActive(true);
                backgroundTile.gameObject.name = "TileBackground_" + x + "_" + y;
                backgroundTile.GetComponent<RectTransform>().anchoredPosition = new Vector2(tileWidth / 2f - levelInfo.Width / 2f * tileWidth + x * tileWidth + x * Spacing, -levelInfo.Height / 2f * tileHeight + y * tileHeight + y * Spacing);

                backgroundTiles[x, y] = backgroundTile;
            }
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
        rect.anchoredPosition3D = new Vector3(TileWidth / 2f - levelInfo.Width / 2f * TileWidth + i * TileWidth + i * Spacing, -levelInfo.Height / 2f * TileWidth + j * TileWidth + j * Spacing, 0);
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
        StartCoroutine(SwapTilesNow(x1, y1, x2, y2, changeInfo));
    }

    IEnumerator SwapTilesNow(int x1, int y1, int x2, int y2, bool changeInfo)
    {
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

        explosionEffect.transform.position = tile.position;
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

    internal void AppearAt(int x, int y, string key)
    {
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

        RectTransform rect = tile.AddComponent<RectTransform>();
        rect.anchoredPosition3D = new Vector3(TileWidth / 2f - levelInfo.Width / 2f * TileWidth + x * TileWidth + x * Spacing, -levelInfo.Height / 2f * TileWidth + y * TileWidth + y * Spacing, 0);
        rect.sizeDelta = new Vector2(TileWidth, TileWidth);
        rect.localScale = new Vector3(1, 1, 1);

        Image tileImage = tile.AddComponent<Image>();
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
}
