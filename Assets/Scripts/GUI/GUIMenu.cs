using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using System;
using static GUIGame;
using System.Net.Sockets;
using UnityEditor;
using UnityEngine.SocialPlatforms.Impl;

public class GUIMenu : MonoBehaviour
{
    public Image MenuImage;
    public Image MapImage;
    public Image GameImage;
    public Image WinDialogImage;
    public Image WinNoMoreMoves;
    public TextMeshProUGUI TxtStatus;
    public Button PlayButton;
    public GameObject Robot1;
    public GameObject Robot2;

    public GameObject PnlPlayerInfo;
    public GameObject PnlPlayerOffline;
    public GameObject PnlLoadingPlace;
    public GameObject PnlPrompt;
    public GameObject PnlWelcome;
    public GameObject PnlHighScores;

    GamePlayManager gamePlayManager = null;
    WalletManager walletManager;
    string fullName = "";

    private void Awake()
    {
        Robot1.SetActive(false);
        Robot2.SetActive(false);

        gamePlayManager = FindObjectOfType<GamePlayManager>();
        walletManager = FindObjectOfType<WalletManager>();
    }

    private void Start()
    {
        PnlWelcome.SetActive(true);
        PnlPrompt.SetActive(false);
        PnlHighScores.SetActive(false);

        Debug.Log("Stored wallet address is: " + LeaderboardManager.Instance.PlayerWalletAddress);
        if(!string.IsNullOrEmpty(LeaderboardManager.Instance.PlayerWalletAddress))
        {
            PnlWelcome.SetActive(false);
            InitSession(LeaderboardManager.Instance.PlayerWalletAddress);
        } else
        {

        }

        /*if (!authenticated)
        {
            DisplayPlayerInfo(false);
            DisplayPlayerInfoLoading(true);
            DisplayPlayerOffline(false);

            if (!string.IsNullOrEmpty(PlayerId) && !string.IsNullOrEmpty(SessionToken))
            {
                StartCoroutine(Authenticate());
            }
            else if (!string.IsNullOrEmpty(PlayerId) && !string.IsNullOrEmpty(Password))
            {
                StartCoroutine(Login());
            }
        }
        else
        {
            DisplayPlayerInfo(true);
            DisplayPlayerInfoLoading(false);
            DisplayPlayerOffline(false);
        }*/
    }

    public void SwitchToMap()
    {
        MenuImage.GetComponent<CanvasGroup>().DOFade(0, 0.5f);

        StartCoroutine(TurnOffGUI());
    }

    public void StartLevel1()
    {
        MenuImage.GetComponent<CanvasGroup>().DOFade(0, 0.5f);

        SwitchToMultiplayer("level1", 1);
    }

    public void SwitchToMultiplayer(string levelFile, int levelNumber)
    {
        /*MenuImage.gameObject.SetActive(true);
        MenuImage.GetComponent<CanvasGroup>().alpha = 0;
        MenuImage.GetComponent<CanvasGroup>().DOFade(1, 0.5f);*/

        //StartCoroutine(StartMultiplayerSequence(levelFile, levelNumber));
        StartCoroutine(TurnOffGUI());
        StartCoroutine(TurnOnPlay(levelFile, levelNumber));

    }

    IEnumerator StartMultiplayerSequence(string levelFile, int levelNumber)
    {
        DisplayStatusText("");

        yield return new WaitForSeconds(1);
        DisplayStatusText("FINDING OPPONENT...");

        yield return new WaitForSeconds(UnityEngine.Random.Range(0.63f, 1.14f));
        DisplayStatusText("OPPONENT FOUND...");

        yield return new WaitForSeconds(UnityEngine.Random.Range(0.32f, 0.49f));
        DisplayStatusText("CONNECTING...");

        yield return new WaitForSeconds(UnityEngine.Random.Range(0.63f, 1.14f));
        DisplayStatusText("STARTING THE GAME...");

        MapImage.gameObject.SetActive(false);
        MenuImage.GetComponent<CanvasGroup>().DOFade(0, 0.5f);

        StartCoroutine(TurnOffGUI());
        StartCoroutine(TurnOnPlay(levelFile, levelNumber));

        yield break;
    }

    IEnumerator TurnOffGUI()
    {
        //MenuImage.raycastTarget = false;
        yield return new WaitForSeconds(0.6f);

        //MenuImage.gameObject.SetActive(false);
        MenuImage.transform.Find("PlayerInfo").gameObject.SetActive(false);

        /*MapImage.gameObject.SetActive(true);
        MapImage.GetComponent<CanvasGroup>().alpha = 0;
        MapImage.GetComponent<CanvasGroup>().DOFade(1, 0.5f);
        yield return new WaitForSeconds(0.5f);*/
    }

    IEnumerator TurnOnPlay(string levelFile, int levelNumber)
    {
        Transform child;
        for (int i = 1; i < GameImage.transform.childCount; i++)
        {
            child = GameImage.transform.GetChild(i);
            if (!child.gameObject.name.StartsWith("Sld") && child.gameObject.name != "ImgBottom" &&
                !child.gameObject.name.StartsWith("ImgPlayerRobot") && !child.gameObject.name.StartsWith("BackgroundTile") &&
                !child.gameObject.name.StartsWith("Robot") && !child.gameObject.name.StartsWith("UI") &&
                child.gameObject.name != "TxtScore" && child.gameObject.name != "TxtStatus")
            {
                Destroy(GameImage.transform.GetChild(i).gameObject);
            }
        }

        //yield return new WaitForSeconds(0.5f);
        yield return new WaitForEndOfFrame();

        MapImage.gameObject.SetActive(false);
        GameImage.gameObject.SetActive(true);
        GameImage.GetComponent<CanvasGroup>().alpha = 0;
        GameImage.GetComponent<CanvasGroup>().DOFade(1, 0.5f);

        yield return new WaitForSeconds(1);

        Transform txtStatus = GameImage.transform.Find("TxtStatus");
        txtStatus.gameObject.SetActive(true);
        txtStatus.SetAsLastSibling();
        txtStatus.GetComponent<TextMeshProUGUI>().color = new Color(1, 1, 1, 0);
        txtStatus.GetComponent<TextMeshProUGUI>().DOFade(1, 0.5f);
        txtStatus.GetComponent<TextMeshProUGUI>().text = "LEVEL " + levelNumber;

        yield return new WaitForSeconds(2);

        txtStatus.GetComponent<TextMeshProUGUI>().DOFade(0, 0.5f);

        yield return new WaitForSeconds(0.5f);

        gamePlayManager?.StartLevel(levelFile, levelNumber);
        MenuImage.gameObject.SetActive(false);
    }

    public void DisplayStatusText(string message)
    {
        PlayButton.gameObject.SetActive(false);
        TxtStatus.gameObject.SetActive(true);

        TxtStatus.text = message;
    }

    public void DisplayWin()
    {
        WinDialogImage.gameObject.SetActive(true);
        //StartCoroutine(DisplayWinNow(numLevel, score));
    }

    /*IEnumerator DisplayWinNow(int numLevel, long score)
    {
    }*/

    public void HideWin()
    {
        Robot1.SetActive(false);
        Robot2.SetActive(false);

        //MapImage.gameObject.SetActive(true);

        WinDialogImage.gameObject.SetActive(false);
        MenuImage.gameObject.SetActive(true);

        LeaderboardManager.Instance.SaveScore(gamePlayManager.GetScore());
        //gamePlayManager?.StartLevel("level" + gamePlayManager.GetNumLevel(), gamePlayManager.GetNumLevel());
        StartCoroutine(TurnOnPlay("level" + gamePlayManager.GetNumLevel(), gamePlayManager.GetNumLevel()));
    }

    public void DisplayNoMoreMoves()
    {
        WinNoMoreMoves.gameObject.SetActive(true);
    }

    public void HideNoMoreMoves()
    {
        WinNoMoreMoves.gameObject.SetActive(false);
    }

    internal void UnlockLevel(int numLevel)
    {
        Transform level = MapImage.transform.Find("Level" + numLevel);

        if(level != null)
        {
            level.GetComponent<LevelInfo>().Locked = false;
            level.transform.Find("ImgLocked").gameObject.SetActive(false);
            level.transform.Find("BtnLevel").GetComponent<Button>().interactable = true;
            level.transform.Find("BtnLevel/Text").gameObject.SetActive(true);

            MenuImage.gameObject.SetActive(false);
        }
    }

    public void DisplayPlayerInfoLoading(bool value)
    {
        PnlLoadingPlace.SetActive(value);
    }

    public void DisplayPlayerInfo(bool value)
    {
        PnlPlayerInfo.SetActive(value);
    }

    public void DisplayPlayerOffline(bool value)
    {
        PnlPlayerOffline.SetActive(value);
    }

    public void DisplayFullName(string fullName)
    {
        this.fullName = fullName;
        PnlPlayerInfo.transform.Find("TxtPlayerName").GetComponent<TextMeshProUGUI>().text = fullName;
    }

    public void SetFullName()
    {
        MenuImage.gameObject.SetActive(false);
        Prompt("ENTER YOUR FULL NAME:", "YOUR FULL NAME", fullName, (param) =>
        {
            DisplayFullName((string)param);
            LeaderboardManager.Instance.SetFullName(fullName);
        });
    }

    public void Prompt(string title, string placeHolder, string defaultValue, OnGUIEvent onSet)
    {
        PnlPrompt.SetActive(true);
        PnlPrompt.transform.SetAsLastSibling();
        PnlPrompt.transform.Find("TxtInfo").GetComponent<TextMeshProUGUI>().text = title;

        TMP_InputField input = PnlPrompt.transform.Find("InpPrompt").GetComponent<TMP_InputField>();
        TextMeshProUGUI placehoder = PnlPrompt.transform.Find("InpPrompt/Text Area/Placeholder").GetComponent<TextMeshProUGUI>();
        placehoder.text = placeHolder;
        input.text = defaultValue;

        Button btnOk = PnlPrompt.transform.Find("BtnChange").GetComponent<Button>();
        btnOk.onClick.RemoveAllListeners();
        btnOk.onClick.AddListener(() =>
        {
            if (input.text.Trim() != "")
            {
                PnlPrompt.SetActive(false);
                MenuImage.gameObject.SetActive(true);
                onSet?.Invoke(input.text);
            }
        });

        Button btnCancel = PnlPrompt.transform.Find("BtnCancel").GetComponent<Button>();
        btnCancel.onClick.RemoveAllListeners();
        btnCancel.onClick.AddListener(() =>
        {
            MenuImage.gameObject.SetActive(true);
            PnlPrompt.SetActive(false);
        });
    }

    internal void SetPlayerRank(long score, int rank)
    {
        MenuImage.transform.Find("PlayerInfo/BtnRank/TxtStart").GetComponent<TextMeshProUGUI>().text = rank.ToString().PadLeft(5, '0');
        DisplayPlayerRank();
    }

    public void DisplayPlayerRank()
    {
        MenuImage.transform.Find("PlayerInfo/BtnRank/TxtStart").GetComponent<TextMeshProUGUI>().text = LeaderboardManager.Instance.Rank.ToString().PadLeft(5, '0');

        GameObject templateItem = PnlHighScores.transform.Find("TxtRowTemplate").gameObject;
        templateItem.GetComponent<TextMeshProUGUI>().text = LeaderboardManager.Instance.Rank + ".";
        templateItem.transform.Find("TxtTitleNickName").GetComponent<TextMeshProUGUI>().text = LeaderboardManager.Instance.PlayerFullName;
        templateItem.transform.Find("TxtTitleScore").GetComponent<TextMeshProUGUI>().text = LeaderboardManager.Instance.Score == -1 ? "N/A" : LeaderboardManager.Instance.Score.ToString();
    }

    public void DisplayHighScores()
    {
        GameObject loading = PnlHighScores.transform.Find("LoadingPlace").gameObject;
        loading.SetActive(true);

        GameObject scoreList = PnlHighScores.transform.Find("ScoreList").gameObject;
        scoreList.SetActive(false);

        MenuImage.gameObject.SetActive(false);
        PnlHighScores.SetActive(true);

        GameObject templateItem = PnlHighScores.transform.Find("TxtRowTemplate").gameObject;
        GameObject lineItem;
        Transform lineRoot = scoreList.transform.Find("Viewport/Content");

        for (int i = 0; i < lineRoot.childCount; i++)
        {
            Destroy(lineRoot.GetChild(i).gameObject);
        }

        LeaderboardManager.Instance.GetTop100Scores((score) =>
        {
            List<ScoreInfo> result = (List<ScoreInfo>)score;

            scoreList.SetActive(true);
            loading.SetActive(false);
            RectTransform rect;
            for (int i = 0; i < result.Count; i++)
            {
                lineItem = Instantiate(templateItem, lineRoot);
                lineItem.GetComponent<TextMeshProUGUI>().text = result[i].Rank + ".";
                lineItem.GetComponent<TextMeshProUGUI>().color = result[i].Us ? Color.yellow : Color.white;

                lineItem.transform.Find("TxtTitleNickName").GetComponent<TextMeshProUGUI>().text = result[i].UserFullName;
                lineItem.transform.Find("TxtTitleNickName").GetComponent<TextMeshProUGUI>().color = result[i].Us ? Color.yellow : Color.white;

                lineItem.transform.Find("TxtTitleScore").GetComponent<TextMeshProUGUI>().text = result[i].Score.ToString();
                lineItem.transform.Find("TxtTitleScore").GetComponent<TextMeshProUGUI>().color = result[i].Us ? Color.yellow : Color.white;

                rect = lineItem.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(35, (i + 1) * -42);
            }

            DisplayPlayerRank();
        });
    }

    public void BackFromHightScores()
    {
        MenuImage.gameObject.SetActive(true);
        PnlHighScores.SetActive(false);
    }

    public void LoginWithMetamask()
    {
        walletManager?.LoginWithMetamask(null, null);
    }

    public void InitSession(string address)
    {
        Debug.Log("Init session...");
        LeaderboardManager.Instance.SetPlayerWalletAddress(address);
        PnlWelcome.SetActive(false);

        PlayButton.gameObject.SetActive(true);
        TxtStatus.gameObject.SetActive(false);
        GameImage.gameObject.SetActive(false);

        DisplayPlayerInfoLoading(true);
        DisplayPlayerOffline(false);

        LeaderboardManager.Instance.GetPlayerScore((parameter) =>
        {
            string[] parameters = ((string)parameter).Split(",");
            long score = long.Parse(parameters[0]);
            int rank = int.Parse(parameters[1]);

            SetPlayerRank(score, rank);
            DisplayPlayerInfoLoading(false);
            DisplayPlayerInfo(true);
            DisplayFullName(LeaderboardManager.Instance.PlayerFullName);
        });

        DisplayFullName(LeaderboardManager.Instance.PlayerFullName);
        DisplayPlayerRank();
    }
}