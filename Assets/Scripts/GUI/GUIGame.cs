using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using BubbleBots.Gameplay.Models;
using BubbleBots.Match3.Data;
using BubbleBots.Match3.Models;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GUIGame : MonoBehaviour
{
    public Image[] BackgroundTiles;
    public int Spacing = 4;
    public int TileWidth = 150;
    public float SwapDuration = 0.33f;
    public float SpecialSwapDuration = 0.33f;
    public float DefaultSwapDuration = 0.33f;
    public RobotEffects[] EnemyRobots;
    public Slider[] EnemyGauges;
    public RobotEffects[] PlayerRobots;
    public Slider[] PlayerGauges;
    public int TopBias = 100;
    [HideInInspector]
    public bool CanSwapTiles = true;
    public TextMeshProUGUI TxtScore;
    public TextMeshProUGUI TxtKilledRobots;
    public Sprite[] RobotSprites;
    public Sprite[] EnemySprites;
    //public Transform WinDialogImage;
    //public GUIMenu Menu;


    //public TextMeshProUGUI bubblesScore;
    public TextMeshProUGUI unclaimedBubblesScore;
    //public Image unclaimedBubblesImage;

    public GameObject bubblesTextPrefab;
    public GameObject bubblesImagePrefab;

    Image[,] backgroundTiles;
    List<GameObject> explosionEffects = new List<GameObject>();
    int currentEnemy = 0;
    string lastLockedBy = "";

    public delegate void OnGUIEvent(object param);

    [DllImport("__Internal")]
    private static extern void Reload();

    [DllImport("__Internal")]
    private static extern void DisplayHelp();

    public void KillPlayerRobot(int id)
    {
        PlayerGauges[id].transform.Find("TxtHP").GetComponent<TextMeshProUGUI>().text = "0 / " + PlayerGauges[id].maxValue;
        PlayerGauges[id].DOValue(0, SwapDuration);
        PlayerRobots[id].Die();

        GameObject bullet = Instantiate(VFXManager.Instance.enemyBullets[currentEnemy], VFXManager.Instance.enemyBullets[currentEnemy].transform.parent);
        bullet.transform.position = VFXManager.Instance.enemyBullets[currentEnemy].transform.position;
        bullet.gameObject.SetActive(true);
        bullet.transform.DOMove(new Vector3(PlayerRobots[id].transform.position.x, PlayerRobots[id].transform.position.y, bullet.transform.position.z), 0.25f).SetEase(Ease.Linear);
        StartCoroutine(HideAndDestroyAfter(bullet, 0.21f, 1, id));
    }

    public void DamagePlayerRobot(int id, int damage)
    {
        PlayerGauges[id].DOValue(PlayerGauges[id].value - damage, SwapDuration);
        PlayerGauges[id].transform.Find("TxtHP").GetComponent<TextMeshProUGUI>().text = Mathf.Max(0, PlayerGauges[id].value - damage) + " / " + PlayerGauges[id].maxValue;

        GameObject bullet = Instantiate(VFXManager.Instance.enemyBullets[currentEnemy], VFXManager.Instance.enemyBullets[currentEnemy].transform.parent);
        bullet.transform.position = EnemyRobots[currentEnemy].transform.position;
        bullet.gameObject.SetActive(true);
        bullet.transform.DOMove(new Vector3(PlayerRobots[id].transform.position.x, PlayerRobots[id].transform.position.y, bullet.transform.position.z), 0.25f).SetEase(Ease.Linear);
        StartCoroutine(HideAndDestroyAfter(bullet, 0.21f, 1, id));
    }

    public void DisplayLose(int score)
    {
        //WinDialogImage.gameObject.SetActive(true);
        //Transform imgWin = WinDialogImage.transform.Find("ImgWin");
        //Transform imgLose = WinDialogImage.transform.Find("ImgLose");
        //Transform btnContinue = WinDialogImage.transform.Find("BtnContinue");
        //btnContinue.gameObject.SetActive(false);
        //imgWin.gameObject.SetActive(false);
        //imgLose.gameObject.SetActive(true);
        //imgLose.transform.Find("TxtMyScore").GetComponent<TextMeshProUGUI>().text = score.ToString();

        //imgLose.transform.localScale = Vector3.zero;
        //imgLose.transform.DOScale(Vector3.one, 0.5f);
    }

    IEnumerator HideAndDestroyAfter(GameObject target, float timeToHide, float timeToDestroy, int id)
    {
        yield return new WaitForSeconds(timeToHide);
        PlayerRobots[id].Damage();

        target.SetActive(false);

        yield return new WaitForSeconds(timeToDestroy);

        Destroy(target);
    }

    public void DamageToEnemyRobot(float enemyHp)
    {
        if(currentEnemy >= EnemyGauges.Length)
        {
            return;
        }

        EnemyGauges[currentEnemy].DOValue(Mathf.Max(0, enemyHp), SwapDuration);
        EnemyRobots[currentEnemy].Damage();
        EnemyGauges[currentEnemy].transform.Find("TxtHP").GetComponent<TextMeshProUGUI>().text = Mathf.Max(0, enemyHp) + " / " + EnemyGauges[currentEnemy].maxValue;
    }

    public void UpdateScore(int currentScore)
    {
        TxtScore.text = currentScore.ToString().PadLeft(6, '0');
    }

    public void KillEnemy()
    {
        TxtKilledRobots.text = UserManager.RobotsKilled.ToString();
        EnemyGauges[currentEnemy].DOValue(0, SwapDuration);
        EnemyRobots[currentEnemy].Die();
    }

    public void SetPlayerRobots(PlayerRoster roster)
    {
        for (int g = 0; g < PlayerGauges.Length; g++)
        {
            PlayerGauges[g].maxValue = roster.bots[g].maxHp;
            PlayerGauges[g].value = roster.bots[g].hp;
            PlayerGauges[g].transform.Find("TxtHP").GetComponent<TextMeshProUGUI>().text = PlayerGauges[g].value + " / " + PlayerGauges[g].maxValue;
            PlayerRobots[g].Initialize();
        }

        for (int i = 0; i < PlayerRobots.Length; ++i)
        {
            PlayerRobots[i].SetRobotImage(roster.bots[i].bubbleBotData.robotSelection);
        }
    }

    public void SetRobotGauges(List<BubbleBot> bots)
    {
        for (int g = 0; g < EnemyGauges.Length; g++)
        {
            EnemyGauges[g].maxValue = bots[g].hp;
            EnemyGauges[g].minValue = 0;
            EnemyGauges[g].value = bots[g].hp;
            EnemyGauges[g].transform.Find("TxtHP").GetComponent<TextMeshProUGUI>().text = bots[g].hp + " / " + bots[g].hp;
        }
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

        if(EnemyGauges[currentEnemy].value <= 0)
        {
            return;
        }

        this.currentEnemy = currentEnemy;
        GameEventsManager.Instance.PostEvent(new GameEventInt() { eventName = GameEvents.FreeModeEnemyChanged, intData = currentEnemy });
        FindObjectOfType<FreeToPlayGameplayManager>()?.TargetEnemy(currentEnemy);
        FindObjectOfType<NetherModeGameplayManager>()?.TargetEnemy(currentEnemy);

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

    public void RenderLevelBackground(int levelWidth, int levelHeight)
    {
        for (int g = 0; g < EnemyGauges.Length; g++)
        {
            EnemyGauges[g].value = EnemyGauges[g].maxValue;
            EnemyGauges[g].transform.Find("TxtHP").GetComponent<TextMeshProUGUI>().text = EnemyGauges[g].maxValue + " / " + EnemyGauges[g].maxValue;
        }

        // remove the old ones
        //wtf
        Transform child;
        for (int i = 1; i < gameObject.transform.childCount; i++)
        {
            child = gameObject.transform.GetChild(i);
            if (!child.gameObject.name.StartsWith("Sld") && child.gameObject.name != "ImgBottom" &&
                !child.gameObject.name.StartsWith("ImgPlayerRobot") && !child.gameObject.name.StartsWith("BackgroundTile") &&
                !child.gameObject.name.StartsWith("Robot") && !child.gameObject.name.StartsWith("UI") &&
                !child.gameObject.name.StartsWith("Txt") && !child.gameObject.name.StartsWith("Img") &&
                child.gameObject.name != "TxtScore" &&
                child.gameObject.name != "quitButton" &&
                child.gameObject.name != "TxtBubbles" &&
                child.gameObject.name != "ImgBubbles" &&
                child.gameObject.name != "TxtStatus" &&
                !child.gameObject.name.StartsWith("Music") &&
                child.gameObject.name != "BtnHelp")
            {
                Destroy(gameObject.transform.GetChild(i).gameObject);
            }
        }

        // add new tiles
        backgroundTiles = new Image[levelWidth, levelHeight];
        Image backgroundTile;
        for (int i = 0; i < BackgroundTiles.Length; i++)
        {
            BackgroundTiles[i].GetComponent<RectTransform>().sizeDelta = new Vector2(TileWidth, TileWidth);
        }

        int tileWidth = TileWidth;
        int tileHeight = TileWidth;
        int itemNumber = 0;

        for (int x = 0; x < levelWidth; x++)
        {
            for (int y = 0; y < levelHeight; y++)
            {
                backgroundTile = Instantiate(BackgroundTiles[itemNumber++ % BackgroundTiles.Length], gameObject.transform);
                backgroundTile.gameObject.SetActive(true);
                backgroundTile.GetComponent<Image>().enabled = true;
                backgroundTile.gameObject.name = "TileBackground_" + x + "_" + y;
                backgroundTile.GetComponent<RectTransform>().anchoredPosition = new Vector2(tileWidth / 2f - levelWidth / 2f * tileWidth + x * tileWidth + x * Spacing, -levelHeight / 2f * tileHeight + y * tileHeight + y * Spacing - TopBias);
                backgroundTile.GetComponent<RectTransform>().localScale = new Vector3(.97f, .97f, .97f);
                backgroundTile.AddComponent<Canvas>();
                backgroundTiles[x, y] = backgroundTile;
            }

            itemNumber += 1;
        }
    }
    void RenderTileOnObject(int i, int j, string id, int levelWidth, int levelHeight, LevelData levelData)
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
        rect.anchoredPosition3D = new Vector3(TileWidth / 2f - levelWidth / 2f * TileWidth + i * TileWidth + i * Spacing, -levelHeight / 2f * TileWidth + j * TileWidth + j * Spacing - TopBias, 0);
        rect.sizeDelta = new Vector2(TileWidth, TileWidth);
        rect.localScale = new Vector3(1, 1, 1);

        tileImage = tile.AddComponent<Image>();
        tileImage.sprite = levelData.GetGemData(id).gemSprite;

        guiTile = tile.AddComponent<GUITile>();
        tile.AddComponent<GraphicRaycaster>();
        guiTile.X = i;
        guiTile.Y = j;
        guiTile.Key = id;
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
        if (!(x1 == x2 || y1 == y2))
        {
            return;
        }

        StartCoroutine(SwapTilesNow(x1, y1, x2, y2, changeInfo));
    }


    public void SwapTilesFail(int x1, int y1, int x2, int y2, bool changeInfo)
    {
        if (!(x1 == x2 || y1 == y2))
        {
            return;
        }

        StartCoroutine(SwapTilesFailNow(x1, y1, x2, y2, changeInfo));
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
            tile1 = transform.Find("Tile_" + x1 + "_" + y1);
            tile2 = transform.Find("Tile_" + x2 + "_" + y2);

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

    IEnumerator SwapTilesFailNow(int x1, int y1, int x2, int y2, bool changeInfo)
    {
        Transform tile1 = null;
        Transform tile2 = null;
        Vector2 tile1Pos = Vector2.zero;
        Vector2 tile2Pos = Vector2.zero;

        try
        {
            LockTiles("L1");
            tile1 = transform.Find("Tile_" + x1 + "_" + y1);
            tile2 = transform.Find("Tile_" + x2 + "_" + y2);

            tile1Pos = tile1.GetComponent<RectTransform>().anchoredPosition;
            tile2Pos = tile2.GetComponent<RectTransform>().anchoredPosition;
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

    public void LockTiles(string lockSource)
    {
        lastLockedBy = lockSource;
        CanSwapTiles = false;
    }

    public void ExplodeBubble(int x, int y, long value)
    {
        //GameObject bubbleImage = Instantiate(bubblesImagePrefab, this.transform);
        Transform tile = transform.Find("Tile_" + x + "_" + y + "_deleted");

        // TODO: Remove in the future versions
        if (tile == null)
        {
            //Debug.Log("EXP1");
            return;
        }

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



    public void ExplodeTile(int x, int y, bool destroyTile)
    {
        GameObject explosionEffect = InstantiateOrReuseExplosion();
        Transform tile = transform.Find("Tile_" + x + "_" + y);
        
        // TODO: Remove in the future versions
        if(tile == null)
        {
            //Debug.Log("EXP1");
            return;
        }

        explosionEffect.transform.position = tile.position + new Vector3(0, 1, -10);
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

    public void ScrollTileDown(int x, int y, int howMany, float duration)
    {
        StartCoroutine(ScrollTileDownNow(x, y, howMany, duration));
    }

    IEnumerator ScrollTileDownNow(int x, int y, int howMany, float duration)
    {
        Transform tile = transform.Find("Tile_" + x + "_" + y);

        // TODO: Remove in future versions
        if(tile == null)
        {
            yield break;
        }

        Vector2 tilePos = tile.GetComponent<RectTransform>().anchoredPosition;
        tile.GetComponent<RectTransform>().DOAnchorPos(tilePos + Vector2.down * TileWidth * howMany + Vector2.down * howMany * Spacing, duration);

        yield return new WaitForSeconds(duration);

        tile.gameObject.name = "Tile_" + x + "_" + (y - howMany);
        tile.GetComponent<GUITile>().X = x;
        tile.GetComponent<GUITile>().Y = y - howMany;
    }


    internal void AppearAt(int x, int y, string key, int levelWidth, int levelHeight, float duration, LevelData levelData)
    {
        StartCoroutine(AppearDelayed(x, y, key, levelWidth, levelHeight, duration, levelData));
    }

    IEnumerator AppearDelayed(int x, int y, string key, int levelWidth, int levelHeight, float duration, LevelData levelData)
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

        RectTransform rect = tile.GetComponent<RectTransform>();
        if (rect == null)
        {
            rect = tile.AddComponent<RectTransform>();
        }

        rect.anchoredPosition3D = new Vector3(TileWidth / 2f - levelWidth / 2f * TileWidth + x * TileWidth + x * Spacing, -levelHeight / 2f * TileWidth + y * TileWidth + y * Spacing - TopBias, 0);
        rect.sizeDelta = new Vector2(TileWidth, TileWidth);
        rect.localScale = new Vector3(1, 1, 1);
        rect.localScale = Vector3.zero;
        rect.DOScale(Vector3.one, duration);

        Image tileImage = tile.GetComponent<Image>();
        if(tileImage == null)
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
        Transform tile = transform.Find("TileBackground_" + x + "_" + y);

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

    public void ColorBombEffect(int x, int y, float duration = .2f)
    {
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
        Image tileImage = tile.GetComponent<Image>();
        StartCoroutine(ShakeObject(tileImage, duration));
    }

    IEnumerator ShakeObject(Image tileImage, float duration)
    {
        for (int i = 0; i < 5; ++i)
        {
            tileImage.transform.DOScale(new Vector3(.9f, .9f, .9f), duration);
            yield return new WaitForSeconds(duration + 0.01f);
        }
    }


    public void ShakeEffect(int x, int y, int numShakes, float shakeDuration)
    {
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
        Transform tile = transform.Find("Tile_" + posX + "_" + posY);

        if (tile == null)
        {
            tile = transform.Find("Tile_" + posX + "_" + posY + "_deleted");
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

        RenewEnemyRobots();
        for (int g = 0; g < EnemyGauges.Length; g++)
        {
            EnemyGauges[g].value = EnemyGauges[g].maxValue;
            EnemyGauges[g].transform.Find("TxtHP").GetComponent<TextMeshProUGUI>().text = EnemyGauges[g].maxValue.ToString("N0") + " / " + EnemyGauges[g].maxValue.ToString("N0");
        }

        for (int i = 0; i < EnemyRobots.Length; i++)
        {
            EnemyRobots[i].FadeIn(); // GetComponent<Image>().DOFade(1, 0.3f).SetEase(Ease.Linear);
        }

        yield return new WaitForSeconds(0.3f);

        CanSwapTiles = true;
        currentEnemy = 0;
        TargetEnemy(currentEnemy);
    }

    public void SetRobots(int robot1, int robot2, int robot3)
    {
        transform.Find("ImgPlayerRobot1").GetComponent<Image>().sprite = RobotSprites[robot1];
        transform.Find("ImgPlayerRobot2").GetComponent<Image>().sprite = RobotSprites[robot2];
        transform.Find("ImgPlayerRobot3").GetComponent<Image>().sprite = RobotSprites[robot3];
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

    public void PremintButton()
    {
//        //WinDialogImage.gameObject.SetActive(false);
//#if !UNITY_EDITOR
//        Premint();
//#endif

//        Menu.gameObject.SetActive(true);
//        Menu.GetComponent<CanvasGroup>().DOFade(1, 0.35f);
//        if (UserManager.PlayerType == PlayerType.Guest)
//        {
//            //Menu.transform.Find("PlayerLogin").gameObject.SetActive(true);
//            SceneManager.LoadScene("Login");
//        }
//        else
//        {
//            Menu.transform.Find("PlayerInfo").gameObject.SetActive(true);
//            Menu.DisplayHighScores();
//            Menu.ReverseHighScoreButtons();
//        }

//        gameObject.SetActive(false);
    }

    public void RenewEnemyRobots()
    {
        int robot1 = UnityEngine.Random.Range(0, EnemySprites.Length);
        int robot2 = UnityEngine.Random.Range(0, EnemySprites.Length);
        int robot3 = UnityEngine.Random.Range(0, EnemySprites.Length);

        while (robot2 == robot1)
        {
            robot2 = UnityEngine.Random.Range(0, EnemySprites.Length);
        }

        while (robot3 == robot1 || robot3 == robot2)
        {
            robot3 = UnityEngine.Random.Range(0, EnemySprites.Length);
        }

        EnemyRobots[0].gameObject.GetComponent<Image>().sprite = EnemySprites[robot1];
        EnemyRobots[1].gameObject.GetComponent<Image>().sprite = EnemySprites[robot2];
        EnemyRobots[2].gameObject.GetComponent<Image>().sprite = EnemySprites[robot3];
    }

    public void DisplayHelpButton()
    {
        DisplayHelp();
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
        unclaimedBubblesScore.GetComponent<TMPro.TextMeshProUGUI>().text = val.ToString();
    }

    public void SetBubblesText(int val)
    {
        //bubblesScore.GetComponent<TMPro.TextMeshProUGUI>().text = val.ToString();
    }
}