using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using static GUIGame;
using System.Runtime.InteropServices;
using Newtonsoft.Json.Linq;

public class GUIMenu : MonoBehaviour
{
    public Image MenuImage;
    public Image GameImage;
    public Image WinDialogImage;
    public Image WinNoMoreMoves;
    public TextMeshProUGUI TxtStatus;
    public Button PlayButton;

    public GameObject PnlPlayerInfo;
    public GameObject PnlPlayerOffline;
    public GameObject PnlPrompt;
    public GameObject PnlHighScores;
    public GameObject PnlRobotSelection;
    public GameObject BtnBackFromHighscores;
    public GameObject BtnAirdrop;

    GamePlayManager gamePlayManager = null;
    string fullName = "";
    int selectedRobot1 = -1;
    int selectedRobot2 = -1;
    int selectedRobot3 = -1;

    [DllImport("__Internal")]
    private static extern void Airdrop();

    private void Awake()
    {
        gamePlayManager = FindObjectOfType<GamePlayManager>();
    }

    private void Start()
    {
        if(UserManager.PlayerType == PlayerType.Guest)
        {
            StartPlayingAsGuest();
        }
        else
        {
            InitSession(UserManager.Instance.GetPlayerWalletAddress());
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
        SoundManager.Instance.FadeOutMusic(() =>
        {
            SoundManager.Instance.PlayLevelMusicNew();
            SoundManager.Instance.FadeInMusic();
        });
        GameImage.GetComponent<GUIGame>().TxtKilledRobots.text = "0";
        GameImage.GetComponent<GUIGame>().RenewEnemyRobots();
        UserManager.RobotsKilled = 0;
        SwitchToMultiplayer(1);
    }

    public void SwitchToMultiplayer(int levelNumber)
    {
        StartCoroutine(TurnOffGUI());
        StartCoroutine(TurnOnPlay());
    }

    IEnumerator TurnOffGUI()
    {
        yield return new WaitForSeconds(0.6f);

        PnlPlayerInfo.SetActive(false);
    }

    IEnumerator TurnOnPlay()
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

        gamePlayManager?.StartGamePlay();

        yield return new WaitForSeconds(1);

        Transform txtStatus = GameImage.transform.Find("TxtStatus");
        txtStatus.gameObject.SetActive(true);
        txtStatus.SetAsLastSibling();
        txtStatus.GetComponent<TextMeshProUGUI>().color = new Color(1, 1, 1, 0);
        txtStatus.GetComponent<TextMeshProUGUI>().DOFade(1, 0.5f);
        txtStatus.GetComponent<TextMeshProUGUI>().text = "LEVEL " + (gamePlayManager.GetCurrentLevel() + 1);

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

        StartCoroutine(TurnOnPlay());
    }

    public void DisplayNoMoreMoves()
    {
        WinNoMoreMoves.gameObject.SetActive(true);
    }

    public void HideNoMoreMoves()
    {
        WinNoMoreMoves.gameObject.SetActive(false);
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
            UserManager.Instance.SetPlayerUserName(fullName, true);
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
        PnlPlayerInfo.transform.Find("WindowBackground/BtnRank/TxtStart").GetComponent<TextMeshProUGUI>().text = UserManager.Instance.GetPlayerRank().ToString().PadLeft(5, '0');

        GameObject templateItem = PnlHighScores.transform.Find("TxtRowTemplate").gameObject;
        templateItem.GetComponent<TextMeshProUGUI>().text = UserManager.Instance.GetPlayerRank().ToString();
        templateItem.transform.Find("TxtTitleNickName").GetComponent<TextMeshProUGUI>().text = UserManager.Instance.GetPlayerUserName().ToUpper();
        templateItem.transform.Find("TxtTitleScore").GetComponent<TextMeshProUGUI>().text = UserManager.Instance.GetPlayerScore() == -1 ? "N/A" : UserManager.Instance.GetPlayerScore().ToString();
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

        UserManager.Instance.GetTop100Scores((data) =>
        {

            List<ScoreInfo> result = new List<ScoreInfo>();
            JArray scoreData = JArray.Parse(data.ToString());
            for (int i = 0; i < scoreData.Count; i++)
            {
                string playerName;
                if (scoreData[i]["nickname"] != null)
                {
                    playerName = scoreData[i]["nickname"].ToString();
                }
                else
                {
                    playerName = "Player " + Random.Range(1000, 10000);
                }

                result.Add(new ScoreInfo(int.Parse(scoreData[i]["score"].ToString()), playerName, "", i + 1, scoreData[i]["address"].ToString() == UserManager.Instance.GetPlayerWalletAddress()));
            }

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

    public void InitSession(string address)
    {
        Debug.Log("Init session for " + address + "...");

        PlayButton.gameObject.SetActive(true);
        TxtStatus.gameObject.SetActive(false);
        GameImage.gameObject.SetActive(false);

        SetPlayerRank(UserManager.Instance.GetPlayerScore(), UserManager.Instance.GetPlayerRank());
        DisplayPlayerInfo(true);
        DisplayFullName(UserManager.Instance.GetPlayerUserName());
        DisplayPlayerRank();
    }

    public void DisplayRobotSelection()
    {
        SoundManager.Instance.FadeOutMusic(() =>
        {
            SoundManager.Instance.PlayRobotSelectMusicNew();
            SoundManager.Instance.FadeInMusic();
        });

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