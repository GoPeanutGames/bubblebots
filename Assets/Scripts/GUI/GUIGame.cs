using System;
using System.Collections;
using System.Collections.Generic;
using BubbleBots.Match3.Data;
using BubbleBots.Match3.Models;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GUIGame : MonoBehaviour
{
    public GameObject BackgroundTile;
    public int Spacing = 4;
    // public int TileWidth = 150;
    public float SwapDuration = 0.33f;
    public float SpecialSwapDuration = 0.33f;
    public float DefaultSwapDuration = 0.33f;
    public Slider[] EnemyGauges;
    public int TopBias = 100;
    [HideInInspector]
    public bool CanSwapTiles = true;
    public TextMeshProUGUI TxtScore;
    public TextMeshProUGUI TxtKilledRobots;
    public Sprite[] RobotSprites;
    public GameObject BoardParent;
    public Material ShineMaterial;

    public TextMeshProUGUI unclaimedBubblesScore;

    public GameObject bubblesTextPrefab;
    public GameObject bubblesImagePrefab;

    Image[,] backgroundTiles;
    List<GameObject> explosionEffects = new List<GameObject>();

    private List<Vector2Int> hintTiles;
    private Coroutine showHint;
    private float TileSize;

    public delegate void OnGUIEvent(object param);

    public void UpdateScore(int currentScore)
    {
        TxtScore.text = currentScore.ToString().PadLeft(6, '0');
    }

    public void KillEnemy()
    {
        TxtKilledRobots.text = UserManager.RobotsKilled.ToString();
    }

    public void TargetEnemy(int currentEnemy)
    {
        TargetEnemy(currentEnemy, true);
    }

    public void TargetEnemy(int currentEnemy, bool manual)
    {
        if (manual && FindObjectOfType<Match3GameplayManager>().inputLocked)
        {
            return;
        }

        if (EnemyGauges[currentEnemy].value <= 0)
        {
            return;
        }

        GameEventsManager.Instance.PostEvent(new GameEventInt() { eventName = GameEvents.FreeModeEnemyChanged, intData = currentEnemy });
        FindObjectOfType<FreeToPlayGameplayManager>()?.TargetEnemy(currentEnemy);
        FindObjectOfType<NetherModeGameplayManager>()?.TargetEnemy(currentEnemy);
    }

    public void RenderLevelBackground(int levelWidth, int levelHeight)
    {
        //still wtf
        foreach (Transform childTransform in BoardParent.transform)
        {
            Destroy(childTransform.gameObject);
        }
        // add new tiles
        RectTransform bParentRectTransform = BoardParent.GetComponent<RectTransform>();
        TileSize = Mathf.Min(bParentRectTransform.rect.width / levelWidth, bParentRectTransform.rect.height / levelHeight);
        BackgroundTile.GetComponent<RectTransform>().sizeDelta = new Vector2(TileSize, TileSize);
        backgroundTiles = new Image[levelWidth, levelHeight];
        GameObject backgroundTile;

        float tileWidth = TileSize;
        float tileHeight = TileSize;

        for (int x = 0; x < levelWidth; x++)
        {
            for (int y = 0; y < levelHeight; y++)
            {
                backgroundTile = Instantiate(BackgroundTile, BoardParent.transform);
                backgroundTile.gameObject.SetActive(true);
                backgroundTile.GetComponent<Image>().enabled = true;
                backgroundTile.GetComponent<RectTransform>().anchoredPosition = new Vector2(tileWidth / 2f - levelWidth / 2f * tileWidth + x * tileWidth + x * Spacing, -levelHeight / 2f * tileHeight + y * tileHeight + y * Spacing - TopBias);
                backgroundTile.GetComponent<RectTransform>().localScale = new Vector3(.97f, .97f, .97f);
                backgroundTile.AddComponent<Canvas>();
                backgroundTiles[x, y] = backgroundTile.GetComponent<Image>();
            }
        }
    }

    void RenderTileOnObject(int i, int j, string id, int levelWidth, int levelHeight, LevelData levelData)
    {
        GameObject tile;
        Image tileImage;
        GUITile guiTile;

        tile = new GameObject();
        tile.transform.SetParent(BoardParent.transform);
        tile.name = "Tile_" + i + "_" + j;

        RectTransform rect = tile.AddComponent<RectTransform>();
        rect.anchoredPosition3D = new Vector3(TileSize / 2f - levelWidth / 2f * TileSize + i * TileSize + i * Spacing, -levelHeight / 2f * TileSize + j * TileSize + j * Spacing - TopBias, 0);
        rect.localScale = new Vector3(.9f, .9f, .9f);
        rect.sizeDelta = new Vector2(TileSize, TileSize);

        tileImage = tile.AddComponent<Image>();
        tileImage.sprite = levelData.GetGemData(id).gemSprite;

        guiTile = tile.AddComponent<GUITile>();
        tile.AddComponent<GraphicRaycaster>();
        guiTile.X = i;
        guiTile.Y = j;
        guiTile.Key = id;
    }

    public void SwapTiles(int x1, int y1, int x2, int y2, bool changeInfo)
    {
        StopHintShowing();
        if (!(x1 == x2 || y1 == y2))
        {
            return;
        }

        StartCoroutine(SwapTilesNow(x1, y1, x2, y2, changeInfo));
    }

    public void ShowHintNow(Hint hint)
    {
        if (hint.isSpecial)
        {
            showHint = StartCoroutine(ShowHint(new List<Vector2Int>() { hint.pos1 }));
        }
        else
        {
            showHint = StartCoroutine(ShowHint(hint.match));
        }
    }

    public void SwapTilesFail(int x1, int y1, int x2, int y2)
    {
        StopHintShowing();
        if (!(x1 == x2 || y1 == y2))
        {
            return;
        }

        StartCoroutine(SwapTilesFailNow(x1, y1, x2, y2));
    }

    IEnumerator SwapTilesNow(int x1, int y1, int x2, int y2, bool changeInfo)
    {
        Transform tile1 = null;
        Transform tile2 = null;
        Vector2 tile1Pos = Vector2.zero;
        Vector2 tile2Pos = Vector2.zero;

        try
        {
            LockTiles("L1");
            tile1 = BoardParent.transform.Find("Tile_" + x1 + "_" + y1);
            tile2 = BoardParent.transform.Find("Tile_" + x2 + "_" + y2);

            tile1Pos = tile1.GetComponent<RectTransform>().anchoredPosition;
            tile2Pos = tile2.GetComponent<RectTransform>().anchoredPosition;
            tile1.GetComponent<RectTransform>().DOAnchorPos(tile2Pos, SwapDuration);
            tile2.GetComponent<RectTransform>().DOAnchorPos(tile1Pos, SwapDuration);
        }
        catch (Exception ex)
        {
            Debug.LogError("E101: " + ex.Message);
            Debug.LogError(ex.StackTrace);

            yield break;
        }

        yield return new WaitForSeconds(SwapDuration);

        try
        {
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
        catch (Exception ex)
        {
            Debug.LogError("E102: " + ex.Message);
            Debug.LogError(ex.StackTrace);
        }
    }

    IEnumerator SwapTilesFailNow(int x1, int y1, int x2, int y2)
    {
        Transform tile1 = null;
        Transform tile2 = null;

        try
        {
            LockTiles("L1");
            tile1 = BoardParent.transform.Find("Tile_" + x1 + "_" + y1);
            tile2 = BoardParent.transform.Find("Tile_" + x2 + "_" + y2);

            tile1.GetComponent<RectTransform>().DOPunchAnchorPos(10 * new Vector2(x1 - x2, y1 - y2), SwapDuration, 50, 10);
            tile2.GetComponent<RectTransform>().DOPunchAnchorPos(10 * new Vector2(x2 - x1, y2 - y1), SwapDuration, 50, 10);
        }
        catch (Exception ex)
        {
            Debug.LogError("E101: " + ex.Message);
            Debug.LogError(ex.StackTrace);
            yield break;
        }
        yield return new WaitForSeconds(SwapDuration);
    }


    IEnumerator ShowHint(List<Vector2Int> tiles)
    {
        Transform tile = null;
        hintTiles = tiles;
        int numBlinks = 2;
        while (numBlinks-- > 0)
        {
            for (int i = 0; i < tiles.Count; ++i)
            {
                tile = BoardParent.transform.Find("Tile_" + tiles[i].x + "_" + tiles[i].y);
                if (tile != null && tile.GetComponent<Image>() != null)
                {
                    tile.GetComponent<Image>().DOColor(new Color(1, 1, 1, 0.5f), SwapDuration);
                }
            }
            yield return new WaitForSeconds(SwapDuration);
            for (int i = 0; i < tiles.Count; ++i)
            {
                tile = BoardParent.transform.Find("Tile_" + tiles[i].x + "_" + tiles[i].y);
                if (tile != null && tile.GetComponent<Image>() != null)
                {
                    tile.GetComponent<Image>().DOColor(new Color(1, 1, 1, 1), SwapDuration);
                }
            }
            yield return new WaitForSeconds(SwapDuration);
        }
    }

    private void StopHintShowing()
    {
        if (showHint == null)
        {
            return;
        }
        StopCoroutine(showHint);
        Transform tile = null;
        if (hintTiles == null)
        {
            return;
        }
        for (int i = 0; i < hintTiles.Count; ++i)
        {
            tile = BoardParent.transform.Find("Tile_" + hintTiles[i].x + "_" + hintTiles[i].y);
            if (tile != null && tile.GetComponent<Image>() != null)
            {
                tile.GetComponent<Image>().DOComplete();
                tile.GetComponent<Image>().color = new Color(1, 1, 1, 1);
            }
        }
    }

    public void LockTiles(string lockSource)
    {
        CanSwapTiles = false;
    }

    public void ExplodeBubble(int x, int y, long value)
    {

        Transform tile = BoardParent.transform.Find("Tile_" + x + "_" + y);
        if (tile == null)
        {
            tile = BoardParent.transform.Find("Tile_" + x + "_" + y + "_deleted");
        }

        if (tile == null)
        {
            return;
        }


        //GameObject bubbleImage = Instantiate(bubblesImagePrefab, this.transform);
        //RectTransform rect = bubbleImage.GetComponent<RectTransform>();
        //if (rect == null)
        //{
        //    rect = bubbleImage.AddComponent<RectTransform>();
        //}

        //bubbleImage.transform.position = tile.position;
        //rect.SetAsLastSibling();
        ////DOTween.To(() => bubbleImage.transform.position, x =>
        ////{
        ////    bubbleImage.transform.position = x;
        ////}, unclaimedBubblesImage.transform.position, 3 * SwapDuration);

        //StartCoroutine(DespawnObject(bubbleImage, 3 * SwapDuration));


        GameObject bubbleText = Instantiate(bubblesTextPrefab, this.transform);
        bubbleText.transform.position = tile.position;
        bubblesTextPrefab.transform.SetAsLastSibling();
        bubbleText.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = "+" + value.ToString();
        bubbleText.GetComponentInChildren<TMPro.TextMeshProUGUI>().DOFade(0, 2f);
        StartCoroutine(DespawnObject(bubbleText, 2f));
    }

    IEnumerator DespawnObject(GameObject bubbleImage, float duration)
    {
        yield return new WaitForSeconds(duration);

        Destroy(bubbleImage);
    }


    public void SpawnTileDamageToEnemyRobot(int x, int y, string id)
    {
        if (true)
        {
            EnemyRobot targetedRobot = GetComponent<GameScreenGame>().GetTargetedRobot();

            Transform explodedTile = BoardParent.transform.Find("Tile_" + x + "_" + y);
            if (explodedTile == null)
            {
                explodedTile = BoardParent.transform.Find("Tile_" + x + "_" + y + "_deleted");
            }

            if (targetedRobot != null && explodedTile != null)
            {
                GameObject animObject = new GameObject();
                animObject.transform.SetParent(BoardParent.transform);
                animObject.name = "moveToTopTile_" + x + "_" + y;
                animObject.SetActive(true);

                RectTransform rect = animObject.GetComponent<RectTransform>();
                if (rect == null)
                {
                    rect = animObject.AddComponent<RectTransform>();
                }

                Vector3 position = backgroundTiles[x, y].transform.GetComponent<RectTransform>().anchoredPosition3D;
                rect.anchoredPosition3D = position;
                rect.sizeDelta = new Vector2(TileSize, TileSize);
                rect.localScale = Vector3.one;

                GameObject bullet = Instantiate(VFXManager.Instance.GetMissileForId(id), animObject.transform);
                bullet.SetActive(true);
                bullet.GetComponent<OrbDamageMissile>().Init();

                animObject.transform.DOMove(targetedRobot.transform.position, .36f).OnComplete(() =>
                {
                    SoundManager.Instance.PlayLightningExplosion();
                    animObject.GetComponentInChildren<OrbDamageMissile>().Explode();
                    Destroy(animObject, 3f);
                });
            }
        }


    }

    //public void Update()
    //{
    //    //if (Input.GetMouseButtonDown(0))
    //    //{
    //    //    //TestFire();
    //    //}
    //}



    //public void TestFire()
    //{
    //    int x = 0;
    //    int y = 0;

    //    EnemyRobot targetedRobot = GetComponent<GameScreenGame>().GetTargetedRobot();

    //    Transform explodedTile = BoardParent.transform.Find("Tile_" + x + "_" + y);
    //    if (explodedTile == null)
    //    {
    //        explodedTile = BoardParent.transform.Find("Tile_" + x + "_" + y + "_deleted");
    //    }

    //    if (targetedRobot != null && explodedTile != null)
    //    {
    //        GameObject animObject = new GameObject();
    //        animObject.transform.SetParent(BoardParent.transform);
    //        animObject.name = "moveToTopTile_" + x + "_" + y;
    //        animObject.SetActive(true);

    //        RectTransform rect = animObject.GetComponent<RectTransform>();
    //        if (rect == null)
    //        {
    //            rect = animObject.AddComponent<RectTransform>();
    //        }

    //        Vector3 position = backgroundTiles[x, y].transform.GetComponent<RectTransform>().anchoredPosition3D;
    //        rect.anchoredPosition3D = position;
    //        rect.sizeDelta = new Vector2(TileSize, TileSize);
    //        rect.localScale = Vector3.one;

    //        GameObject bullet = Instantiate(VFXManager.Instance.GetMissileForId("-1"), animObject.transform);
    //        bullet.SetActive(true);
    //        bullet.GetComponent<OrbDamageMissile>().Init();

    //        //SoundManager.Instance.PlayLightningMissile();
    //        animObject.transform.DOMove(targetedRobot.transform.position, .36f).OnComplete(() =>
    //        {
    //            SoundManager.Instance.PlayLightningExplosion();
    //            animObject.GetComponentInChildren<OrbDamageMissile>().Explode();
    //            Destroy(animObject, 2f);
    //        });
    //    }
    //}

    public void ExplodeTile(int x, int y, string id, bool destroyTile)
    {
        SpawnTileDamageToEnemyRobot(x, y, id);

        GameObject explosionEffect = InstantiateOrReuseExplosion();
        Transform tile = BoardParent.transform.Find("Tile_" + x + "_" + y);

        // TODO: Remove in the future versions
        if (tile == null)
        {
            //Debug.Log("EXP1");
            return;
        }

        explosionEffect.transform.position = tile.position + new Vector3(0, 1, -10);
        explosionEffect.SetActive(true);

        StartCoroutine(DespawnExplosion(explosionEffect));

        Transform tileToDisactivate = BoardParent.transform.Find("Tile_" + x + "_" + y);
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

        GameObject newExplosion = Instantiate(VFXManager.Instance.ExplosionEffect);
        explosionEffects.Add(newExplosion);

        return newExplosion;
    }

    public void DestroyExplosionEffects()
    {
        for (int i = 0; i < explosionEffects.Count; i++)
        {
            Destroy(explosionEffects[i].gameObject);
        }
    }

    public void ScrollTile(int x, int y, int howMany, float duration, int direction = 1)
    {
        StartCoroutine(ScrollTileNow(x, y, howMany, duration, direction));
    }

    IEnumerator ScrollTileNow(int x, int y, int howMany, float duration, int direction = 1)
    {
        Transform tile = BoardParent.transform.Find("Tile_" + x + "_" + y);

        // TODO: Remove in future versions
        if (tile == null)
        {
            yield break;
        }

        Vector2 tilePos = tile.GetComponent<RectTransform>().anchoredPosition;
        tile.GetComponent<RectTransform>().DOAnchorPos(tilePos + direction * Vector2.down * TileSize * howMany + direction * Vector2.down * howMany * Spacing, duration);

        yield return new WaitForSeconds(duration);

        tile.gameObject.name = "Tile_" + x + "_" + (y - direction * howMany);
        tile.GetComponent<GUITile>().X = x;
        tile.GetComponent<GUITile>().Y = y - direction * howMany;
    }


    internal void AppearAt(int x, int y, string key, int levelWidth, int levelHeight, float duration, LevelData levelData)
    {
        StartCoroutine(AppearDelayed(x, y, key, levelWidth, levelHeight, duration, levelData));
    }

    IEnumerator AppearDelayed(int x, int y, string key, int levelWidth, int levelHeight, float duration, LevelData levelData)
    {
        Transform trans = BoardParent.transform.Find("Tile_" + x + "_" + y + "_deleted");
        GameObject tile;

        if (trans == null)
        {
            tile = new GameObject();
        }
        else
        {
            tile = trans.gameObject;
        }

        tile.transform.SetParent(BoardParent.transform);
        tile.name = "Tile_" + x + "_" + y;
        tile.SetActive(true);

        RectTransform rect = tile.GetComponent<RectTransform>();
        if (rect == null)
        {
            rect = tile.AddComponent<RectTransform>();
        }

        rect.anchoredPosition3D = new Vector3(TileSize / 2f - levelWidth / 2f * TileSize + x * TileSize + x * Spacing, -levelHeight / 2f * TileSize + y * TileSize + y * Spacing - TopBias, 0);
        rect.sizeDelta = new Vector2(TileSize, TileSize);
        rect.localScale = new Vector3(1, 1, 1);
        rect.localScale = Vector3.zero;
        rect.DOScale(Vector3.one, duration);

        Image tileImage = tile.GetComponent<Image>();
        if (tileImage == null)
        {
            tileImage = tile.AddComponent<Image>();
        }

        tileImage.sprite = levelData.GetGemData(key).gemSprite;
        tileImage.DOFade(0, 0);
        tileImage.DOFade(1, duration);

        GUITile guiTile = tile.GetComponent<GUITile>();
        if (guiTile == null)
        {
            guiTile = tile.AddComponent<GUITile>();
        }

        if (tile.GetComponent<GraphicRaycaster>() == null)
        {
            tile.AddComponent<GraphicRaycaster>();
        }

        guiTile.X = x;
        guiTile.Y = y;
        guiTile.Key = key;

        yield return new WaitForSeconds(duration);
    }

    public void LineDestroyEffect(int x, int y, bool vertical, float duration = 0.33f)
    {
        GameObject explosionEffect1 = Instantiate(VFXManager.Instance.LineExplosionEffect);
        GameObject explosionEffect2 = Instantiate(VFXManager.Instance.LineExplosionEffect);
        Transform tile = backgroundTiles[x, y].transform;

        // TODO: Remove in the future versions
        if (tile == null)
        {
            tile = BoardParent.transform.Find("Tile_" + x + "_" + y + "_deleted");
            if (tile == null)
            {
                Debug.LogWarning("Line destroy effect failed to find the tile Tile_" + x + "_" + y);
                return;
            }
        }

        explosionEffect1.transform.position = tile.position + new Vector3(0, 1, -10);
        explosionEffect1.SetActive(true);
        explosionEffect1.transform.DOMove(explosionEffect1.transform.position + (vertical ? Vector3.up * 35 : Vector3.left * 35), duration).SetEase(Ease.Linear);

        explosionEffect2.transform.position = tile.position + new Vector3(0, 1, -10);
        explosionEffect2.SetActive(true);
        explosionEffect2.transform.DOMove(explosionEffect1.transform.position + (vertical ? Vector3.up * -35 : Vector3.left * -35), duration).SetEase(Ease.Linear);

        Destroy(explosionEffect1, duration + 0.01f);
        Destroy(explosionEffect2, duration + 0.01f);
    }

    public void ColorBlastEffect(int x, int y)
    {
        GameObject colorExplosionEffect = Instantiate(VFXManager.Instance.ColorExplosionEffect);
        Transform tile = BoardParent.transform.Find("Tile_" + x + "_" + y);

        if (tile == null)
        {
            tile = BoardParent.transform.Find("Tile_" + x + "_" + y + "_deleted");
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

    public void ColorBombEffect(int x, int y, float duration = .2f, int numShakes = 3)
    {
        Transform tile = BoardParent.transform.Find("Tile_" + x + "_" + y);

        if (tile == null)
        {
            tile = BoardParent.transform.Find("Tile_" + x + "_" + y + "_deleted");
            if (tile == null)
            {
                Debug.LogWarning("Color blast effect failed to find the tile Tile_" + x + "_" + y);
                return;
            }
        }
        Image tileImage = tile.GetComponent<Image>();
        StartCoroutine(ShakeObject(tileImage, duration, numShakes));
    }

    IEnumerator ShakeObject(Image tileImage, float duration, int numShakes)
    {
        for (int i = 0; i < numShakes; ++i)
        {
            tileImage.transform.DOScale(new Vector3(.9f, .9f, .9f), duration);
            yield return new WaitForSeconds(duration + 0.01f);
            tileImage.transform.DOScale(Vector3.one, duration);
            yield return new WaitForSeconds(duration + 0.01f);
        }
    }


    public void ShakeEffect(int x, int y, int numShakes, float shakeDuration)
    {
        Transform tile = BoardParent.transform.Find("Tile_" + x + "_" + y);

        if (tile == null)
        {
            tile = BoardParent.transform.Find("Tile_" + x + "_" + y + "_deleted");
            if (tile == null)
            {
                Debug.LogWarning("Color blast effect failed to find the tile Tile_" + x + "_" + y);
                return;
            }
        }
        Image tileImage = tile.GetComponent<Image>();
        StartCoroutine(ShakeObject(tileImage, numShakes, shakeDuration));
    }


    IEnumerator ShakeObject(Image tileImage, int numShakes, float shakeDuration)
    {
        while (numShakes-- > 0)
        {
            tileImage.transform.DOScale(new Vector3(.9f, .9f, .9f), shakeDuration);
            yield return new WaitForSeconds(shakeDuration);
            tileImage.transform.DOScale(Vector3.one, shakeDuration);
            yield return new WaitForSeconds(shakeDuration);
        }
    }

    public void ColorChangeEffect(string key, List<Vector2Int> changedTiles, LevelData levelData, float duration)
    {
        int x, y;
        for (int i = 0; i < changedTiles.Count; i++)
        {
            x = (int)changedTiles[i].x;
            y = (int)changedTiles[i].y;
            Transform tile = BoardParent.transform.Find("Tile_" + x + "_" + y);

            if (tile == null)
            {
                tile = BoardParent.transform.Find("Tile_" + x + "_" + y + "_deleted");
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
            dimage.sprite = levelData.GetGemData(key).gemSprite;

            StartCoroutine(ChangeColor(tileImage, dimage, key, levelData, duration));
            Destroy(duplicate.gameObject, duration + 0.02f);
        }
    }
    public void ChangeColorScale(int posX, int posY, string key, LevelData levelData)
    {
        StartCoroutine(ScaleDownAndChangeColorAndScaleUp(posX, posY, key, levelData));
    }

    IEnumerator ScaleDownAndChangeColorAndScaleUp(int posX, int posY, string key, LevelData levelData)
    {
        Transform tile = BoardParent.transform.Find("Tile_" + posX + "_" + posY);

        if (tile == null)
        {
            tile = BoardParent.transform.Find("Tile_" + posX + "_" + posY + "_deleted");
            if (tile == null)
            {
                Debug.LogWarning("Color change effect failed to find the tile Tile_" + posX + "_" + posY);
            }
        }
        Image tileImage = tile.GetComponent<Image>();
        if (tileImage == null)
        {
            tileImage = tile.AddComponent<Image>();
        }

        tileImage.transform.DOScale(0, 0.2f);

        yield return new WaitForSeconds(0.2f);
        tileImage.sprite = levelData.GetGemData(key).gemSprite;
        tileImage.transform.DOScale(1, 0.2f);
        yield return new WaitForSeconds(0.2f);
    }

    IEnumerator ChangeColor(Image tileImage, Image dimage, string key, LevelData levelData, float duration)
    {
        dimage.color = new Color(1, 1, 1, 0);
        dimage.DOFade(1, duration);

        yield return new WaitForSeconds(duration);
        tileImage.sprite = levelData.GetGemData(key).gemSprite;
        dimage.gameObject.SetActive(false);
    }

    public void SetRobots(int robot1, int robot2, int robot3)
    {
        transform.Find("ImgPlayerRobot1").GetComponent<Image>().sprite = RobotSprites[robot1];
        transform.Find("ImgPlayerRobot2").GetComponent<Image>().sprite = RobotSprites[robot2];
        transform.Find("ImgPlayerRobot3").GetComponent<Image>().sprite = RobotSprites[robot3];
    }

    IEnumerator DisplayHintAtNow(int x1, int y1)
    {
        Transform tile1 = BoardParent.transform.Find("Tile_" + x1 + "_" + y1);

        if (tile1 == null)
        {
            tile1 = BoardParent.transform.Find("Tile_" + x1 + "_" + y1 + "_deleted");
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

            tile1.GetComponent<RectTransform>().DOAnchorPos(new Vector2(_x1 + (i % 2 == 0 ? 1 : -1) * Random.Range(0f, 10f), _y1 + (i % 2 == 0 ? 1 : -1) * Random.Range(0f, 10f)), 0.15f).SetEase(Ease.Linear);
            yield return new WaitForSeconds(0.15f);
        }

        tile1.GetComponent<RectTransform>().DOAnchorPos(new Vector2(_x1, _y1), 0.15f).SetEase(Ease.Linear);
    }

    public void DisplayHelpButton()
    {
        Application.OpenURL("https://www.youtube.com/watch?v=w10rwbbQVr8");
    }


    //refactored code
    public void RenderTiles(BoardModel boardModel, LevelData levelData)
    {
        for (int i = 0; i < boardModel.width; i++)
        {
            for (int j = 0; j < boardModel.height; j++)
            {
                RenderTileOnObject(i, j, boardModel[i][j].gem.GetId(), boardModel.width, boardModel.height, levelData);
            }
        }
    }

    public void ShowLevelText(int level, float duration, float fadeDuration)
    {
        StartCoroutine(DisplayLevelText(level, duration, fadeDuration));
    }

    IEnumerator DisplayLevelText(int level, float duration, float fadeDuration)
    {
        Transform txtStatus = this.transform.Find("TxtStatus");
        txtStatus.gameObject.SetActive(true);
        txtStatus.SetAsLastSibling();
        txtStatus.GetComponent<TextMeshProUGUI>().color = new Color(1, 1, 1, 0);
        txtStatus.GetComponent<TextMeshProUGUI>().DOFade(1, 0.5f);
        txtStatus.GetComponent<TextMeshProUGUI>().text = "LEVEL " + (level + 1);
        yield return new WaitForSeconds(duration);
        txtStatus.GetComponent<TextMeshProUGUI>().DOFade(0, fadeDuration);
        yield return new WaitForSeconds(0.5f);
    }

    public void SetUnclaimedBubblesText(int val)
    {
        unclaimedBubblesScore.GetComponent<TextMeshProUGUI>().text = val.ToString();
    }

    public void SetBubblesText(int val)
    {
        //bubblesScore.GetComponent<TMPro.TextMeshProUGUI>().text = val.ToString();
    }

    private Vector2Int currentSpecialPosition = -Vector2Int.one;

    public void HighlightSpecial(Vector2Int specialPosition)
    {
        if (currentSpecialPosition != -Vector2Int.one)
        {
            backgroundTiles[currentSpecialPosition.x, currentSpecialPosition.y].material = null;
        }
        if (specialPosition != -Vector2Int.one)
        {
            backgroundTiles[specialPosition.x, specialPosition.y].material = ShineMaterial;
        }
        currentSpecialPosition = specialPosition;
    }

    public void DehighlightSpecial()
    {
        if (currentSpecialPosition != -Vector2Int.one)
        {
            backgroundTiles[currentSpecialPosition.x, currentSpecialPosition.y].material = null;
        }
        currentSpecialPosition = -Vector2Int.one;
    }
}