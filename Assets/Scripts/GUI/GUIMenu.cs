using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using static GUIGame;
using System.Runtime.InteropServices;

public class GUIMenu : MonoBehaviour
{
    public ServerPlayerController serverPlayerController;
    public Image MenuImage;
    public Image GameImage;
    public Image WinDialogImage;
    public Image WinNoMoreMoves;
    public TextMeshProUGUI TxtStatus;
    public Button PlayButton;

    public GameObject PnlPlayerInfo;
    public GameObject PnlPlayerOffline;
    public GameObject PnlLoadingPlace;
    public GameObject PnlPrompt;
    public GameObject PnlHighScores;
    public GameObject PnlRobotSelection;
    public GameObject BtnBackFromHighscores;
    public GameObject BtnAirdrop;

    GamePlayManager gamePlayManager = null;
    SoundManager soundManager;
    string fullName = "";
    int selectedRobot1 = -1;
    int selectedRobot2 = -1;
    int selectedRobot3 = -1;

    [DllImport("__Internal")]
    private static extern void Airdrop();

    private void Awake()
    {
        gamePlayManager = FindObjectOfType<GamePlayManager>();
        soundManager = FindObjectOfType<SoundManager>();
    }

    private void Start()
    {
        if(LeaderboardManager.Instance.CurrentPlayerType == PlayerType.Guest)
        {
            StartPlayingAsGuest();
        }
        else
        {
            InitSession(LeaderboardManager.Instance.PlayerWalletAddress);
        }
    }

    public void SwitchToMap()
    {
        MenuImage.GetComponent<CanvasGroup>().DOFade(0, 0.5f);

        StartCoroutine(TurnOffGUI());
    }

    public void SetSelectedRobots(int robot1, int robot2, int robot3)
    {
        selectedRobot1 = robot1;
        selectedRobot2 = robot2;
        selectedRobot3 = robot3;
    }

    public void StartLevel1()
    {
        MenuImage.GetComponent<CanvasGroup>().DOFade(0, 0.5f);
        Debug.Log(soundManager);
        soundManager.PlayLevelMusic();
        GameImage.GetComponent<GUIGame>().TxtKilledRobots.text = "0";
        GameImage.GetComponent<GUIGame>().RenewEnemyRobots();
        LeaderboardManager.Instance.ResetKilledRobots();
        SwitchToMultiplayer("level1", 1);
    }

    public void SwitchToMultiplayer(string levelFile, int levelNumber)
    {
        StartCoroutine(TurnOffGUI());
        StartCoroutine(TurnOnPlay(levelFile, levelNumber));
    }

    IEnumerator TurnOffGUI()
    {
        yield return new WaitForSeconds(0.6f);

        MenuImage.transform.Find("PlayerInfo").gameObject.SetActive(false);
        MenuImage.transform.Find("PlayAsGuest").gameObject.SetActive(false);
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
                child.gameObject.name != "TxtScore" && child.gameObject.name != "TxtStatus"
                 && child.gameObject.name != "BtnHelp")
            {
                Destroy(GameImage.transform.GetChild(i).gameObject);
            }
        }

        yield return new WaitForEndOfFrame();

        GameImage.gameObject.SetActive(true);
        GameImage.GetComponent<GUIGame>().RenewEnemyRobots();
        GameImage.GetComponent<GUIGame>().SetRobots(selectedRobot1, selectedRobot2, selectedRobot3);
        GameImage.GetComponent<CanvasGroup>().alpha = 0;
        GameImage.GetComponent<CanvasGroup>().DOFade(1, 0.5f);

        gamePlayManager?.StartLevel(levelFile, levelNumber);

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

        GameImage.GetComponent<GUIGame>().CanSwapTiles = true;
        GameImage.GetComponent<GUIGame>().TargetEnemy(0);
        txtStatus.gameObject.SetActive(false);
        MenuImage.gameObject.SetActive(false);
    }

    public void StartPlayingAsGuest()
    {
        TxtStatus.gameObject.SetActive(false);
        GameImage.gameObject.SetActive(false);

        DisplayPlayerInfo(false);
        DisplayPlayerOffline(false);

        DisplayRobotSelection();
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
        Transform imgWin = WinDialogImage.transform.Find("ImgWin");
        Transform imgLose = WinDialogImage.transform.Find("ImgLose");
        imgWin.gameObject.SetActive(true);
        imgLose.gameObject.SetActive(false);

        imgWin.transform.localScale = Vector3.zero;
        imgWin.transform.DOScale(Vector3.one, 0.5f);
        //StartCoroutine(DisplayWinNow(numLevel, score));
    }

    public void DisplayLose()
    {
        Transform imgWin = WinDialogImage.transform.Find("ImgWin");
        Transform imgLose = WinDialogImage.transform.Find("ImgLose");
        imgWin.gameObject.SetActive(false);
        imgLose.gameObject.SetActive(true);
        imgLose.transform.Find("TxtMyScore").GetComponent<TextMeshProUGUI>().text = gamePlayManager.GetScore().ToString();

        imgLose.transform.localScale = Vector3.zero;
        imgLose.transform.DOScale(Vector3.one, 0.5f);
    }
    public void HideWin()
    {
        WinDialogImage.gameObject.SetActive(false);
        MenuImage.gameObject.SetActive(true);

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
        PnlPlayerInfo.transform.Find("WindowBackground/BtnChangeName/TxtPlayerName").GetComponent<TextMeshProUGUI>().text = fullName.ToUpper();
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
        PnlPrompt.transform.Find("Window/TxtInfo").GetComponent<TextMeshProUGUI>().text = title;

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
        PnlPlayerInfo.transform.Find("WindowBackground/BtnRank/TxtStart").GetComponent<TextMeshProUGUI>().text = rank.ToString().PadLeft(5, '0');
        DisplayPlayerRank();
    }

    public void DisplayPlayerRank()
    {
        PnlPlayerInfo.transform.Find("WindowBackground/BtnRank/TxtStart").GetComponent<TextMeshProUGUI>().text = LeaderboardManager.Instance.Rank.ToString().PadLeft(5, '0');

        GameObject templateItem = PnlHighScores.transform.Find("TxtRowTemplate").gameObject;
        templateItem.GetComponent<TextMeshProUGUI>().text = LeaderboardManager.Instance.Rank + ".";
        templateItem.transform.Find("TxtTitleNickName").GetComponent<TextMeshProUGUI>().text = LeaderboardManager.Instance.PlayerFullName.ToUpper();
        templateItem.transform.Find("TxtTitleScore").GetComponent<TextMeshProUGUI>().text = LeaderboardManager.Instance.Score == -1 ? "N/A" : LeaderboardManager.Instance.Score.ToString();
    }

    public void DisplayHighScores()
    {
        GameObject loading = PnlHighScores.transform.Find("LoadingPlace").gameObject;
        loading.SetActive(true);

        GameObject scoreList = PnlHighScores.transform.Find("Window/ScoreList").gameObject;
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
            var root = lineRoot.GetComponent<RectTransform>();
            root.sizeDelta = new Vector2(root.sizeDelta.x, 42 * result.Count + 20);
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
                rect.anchoredPosition = new Vector2(-225, 10 + (i + 1) * -42);
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
        //WalletManager.Instance.LoginWithMetamask();
    }

    public void InitSession(string address)
    {
        Debug.Log("Init session for " + address + "...");

        PlayButton.gameObject.SetActive(true);
        TxtStatus.gameObject.SetActive(false);
        GameImage.gameObject.SetActive(false);

        DisplayPlayerInfo(false);
        DisplayPlayerOffline(false);

        SetPlayerRank(LeaderboardManager.Instance.Score, LeaderboardManager.Instance.Rank);
        DisplayPlayerInfo(true);
        DisplayFullName(LeaderboardManager.Instance.PlayerFullName);
        DisplayPlayerRank();
    }

    public void DisplayRobotSelection()
    {
        soundManager?.FadeOutStartMusic();
        soundManager?.FadeInRobotSelectionMusic();

        PnlRobotSelection.SetActive(true);
        gameObject.SetActive(false);
    }

    public void AirdropFromMenu()
    {
        BtnBackFromHighscores.SetActive(true);
        BtnAirdrop.SetActive(false);

        Airdrop();
        BackFromHightScores();
    }

    public void ReverseHighScoreButtons()
    {
        BtnBackFromHighscores.SetActive(false);
        BtnAirdrop.SetActive(true);
    }
}