using CodeStage.AntiCheat.ObscuredTypes;
using CodeStage.AntiCheat.Storage;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class LeaderboardManager : MonoBehaviour
{
    public string ServerURL = "http://localhost:8090";
    public string HashKey = "Hsh123_?";
    [HideInInspector]
    public string PlayerFullName = "";
    public string PlayerWalletAddress = "0xb354B942464755968a655144Bb1f69422b7D5ea2";
    [HideInInspector]
    public ObscuredLong Score = 0;
    [HideInInspector]
    public ObscuredInt Rank = int.MaxValue;

    public ObscuredString PlayerId = "tolgak";
    public ObscuredString Password = "123";
    public delegate void LeaderboardEvent(object param);

    string SessionToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJhdXRoIjoiYWYtdXNlciIsImFnZW50IjoiIiwidG9rZW4iOiJmcmV5LXBhcmstc3RhdmUtaHVydGxlLXNvcGhpc20tbW9uYWNvLW1ha2VyLW1pbm9yaXR5LXRoYW5rZnVsLWdyb2Nlci11bmNpYWwtcG9uZ2VlIiwiaWF0IjoxNjYzNjk4NDkzfQ.wEOeF3Up1aJOtFUOLWB4AGKf-NBS609UoL4kIgrSGms";
    bool authenticated = false;
    GUIMenu gui;

    LeaderboardEvent onAuthenticationComplete = null;

    SecurityController security;

    public static LeaderboardManager Instance;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        if (ObscuredPrefs.HasKey("user_name"))
        {
            PlayerId = ObscuredPrefs.Get("user_name", "");
        }

        if (ObscuredPrefs.HasKey("password"))
        {
            Password = ObscuredPrefs.Get("password", "");
        }

        if (ObscuredPrefs.HasKey("session_token"))
        {
            SessionToken = ObscuredPrefs.Get("session_token", SessionToken);
        }

        if (ObscuredPrefs.HasKey("full_name"))
        {
            PlayerFullName = ObscuredPrefs.Get("full_name", "");
        }

        if (ObscuredPrefs.HasKey("rank"))
        {
            Rank = ObscuredPrefs.Get("rank", 9999);
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        gui = FindObjectOfType<GUIMenu>();
        gui.DisplayPlayerInfo(false);
        gui.DisplayPlayerInfoLoading(true);
        gui.DisplayPlayerOffline(false);

        onAuthenticationComplete = (param) => {
            GetPlayerScore((parameter) =>
            {
                string[] parameters = ((string)parameter).Split(",");
                long score = long.Parse(parameters[0]);
                int rank = int.Parse(parameters[1]);

                this.Score = score;
                this.Rank = rank;

                gui.SetPlayerRank(score, rank);
            });

            GetTop100Scores((scores) =>
            {
                gui.DisplayPlayerInfo(true);
                gui.DisplayPlayerInfoLoading(false);
                gui.DisplayPlayerOffline(false);

                gui.PlayButton.gameObject.SetActive(true);
            });
        };

        if (!authenticated)
        {
            gui.DisplayPlayerInfo(false);
            gui.DisplayPlayerInfoLoading(true);
            gui.DisplayPlayerOffline(false);

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
            gui.DisplayPlayerInfo(true);
            gui.DisplayPlayerInfoLoading(false);
            gui.DisplayPlayerOffline(false);
        }
    }

    private IEnumerator Authenticate()
    {
        //Debug.Log(ServerURL + "/login.php?user_name=" + PlayerId + "&password=" + SecurityController.ComputeSha256Hash(Password));

        authenticated = true;
        onAuthenticationComplete?.Invoke(null);

        yield break;

        using (UnityWebRequest webRequest = UnityWebRequest.Get(ServerURL + "/authenticate.php?user_name=" + PlayerId + "&session_token=" + SessionToken))
        {
            yield return webRequest.SendWebRequest();

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError("Connection failed for reason: " + webRequest.error);

                    gui.DisplayPlayerInfo(false);
                    gui.DisplayPlayerInfoLoading(false);
                    gui.DisplayPlayerOffline(true);
                    break;
                case UnityWebRequest.Result.Success:
                    string data = webRequest.downloadHandler.text;
                    Debug.Log("Received data: " + data);

                    JObject o = JObject.Parse(data);
                    if (o["success"].ToString() == "True")
                    {
                        authenticated = true;
                        SessionToken = o["token"].ToString();
                        PlayerFullName = o["full_name"].ToString();
                        ObscuredPrefs.Set("session_token", SessionToken);
                        onAuthenticationComplete?.Invoke(null);
                    }
                    else
                    {
                        Debug.LogError(o["result"].ToString());
                    }

                    break;
            }
        }
    }

    private IEnumerator Login()
    {
        authenticated = true;
        onAuthenticationComplete?.Invoke(null);

        yield break;

        //Debug.Log(ServerURL + "/login.php?user_name=" + PlayerId + "&password=" + SecurityController.ComputeSha256Hash(Password));
        using (UnityWebRequest webRequest = UnityWebRequest.Get(ServerURL + "/login.php?user_name=" + PlayerId + "&password=" + SecurityController.ComputeSha256Hash(Password)))
        {
            yield return webRequest.SendWebRequest();

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError("Connection failed for reason: " + webRequest.error);

                    gui.DisplayPlayerInfo(false);
                    gui.DisplayPlayerInfoLoading(false);
                    gui.DisplayPlayerOffline(true);
                    break;
                case UnityWebRequest.Result.Success:
                    string data = webRequest.downloadHandler.text;
                    Debug.Log("Received data (login): " + data);

                    JObject o = JObject.Parse(data);
                    if (o["success"].ToString() == "True")
                    {
                        authenticated = true;
                        SessionToken = o["token"].ToString();
                        PlayerFullName = o["full name"].ToString();
                        gui.DisplayFullName(o["full name"].ToString());
                        onAuthenticationComplete?.Invoke(null);
                    }
                    else
                    {
                        gui.DisplayPlayerInfo(false);
                        gui.DisplayPlayerInfoLoading(false);
                        gui.DisplayPlayerOffline(true);

                        Debug.LogError(o["reason"].ToString());
                    }

                    break;
            }
        }
    }

    public void SaveScore(long score)
    {
        this.Score = score;

        StartCoroutine(SaveScoreNow(score));
    }

    private IEnumerator SaveScoreNow(long score)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(ServerURL + "/set_score.php?user_name=" + PlayerId + "&session_token=" + SessionToken + "&score=" + score))
        {
            yield return webRequest.SendWebRequest();

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError("Connection failed for reason: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    string data = webRequest.downloadHandler.text;
                    Debug.Log("Received data (set score): " + data);

                    JObject o = JObject.Parse(data);
                    if (o["success"].ToString() == "True")
                    {
                        Debug.Log("Score has been set with success");
                    }
                    else
                    {
                        Debug.LogError(o["reason"].ToString());
                    }

                    break;
            }
        }
    }

    public void GetPlayerScore(LeaderboardEvent onComplete)
    {
        StartCoroutine(GetPlayerScoreNow(onComplete));
    }

    private IEnumerator GetPlayerScoreNow(LeaderboardEvent onComplete)
    {
        long score = 0;
        int rank = int.MaxValue;

        using (UnityWebRequest webRequest = UnityWebRequest.Get(ServerURL + "/bubblebots/score/" + PlayerWalletAddress))
        {
            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.SetRequestHeader("Accept", "*/*");

            yield return webRequest.SendWebRequest();

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError("Connection failed for reason: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    string data = webRequest.downloadHandler.text;
                    Debug.Log("Received data (get score): " + data);

                    JObject o = JObject.Parse(data);
                    score = long.Parse(o["score"].ToString());
                    rank = int.Parse(o["rank"].ToString());
                    ObscuredPrefs.Get("rank", Rank);

                    break;
            }
        }

        onComplete?.Invoke(score.ToString() + "," + rank.ToString());
    }

    public void GetTop100Scores(LeaderboardEvent onComplete)
    {
        StartCoroutine(GetTop100ScoresNow(onComplete));
    }

    private IEnumerator GetTop100ScoresNow(LeaderboardEvent onComplete)
    {
        List<ScoreInfo> result = new List<ScoreInfo>();

        //using (UnityWebRequest webRequest = UnityWebRequest.Get(ServerURL + "/get_score.php?user_name=" + PlayerId + "&session_token=" + SessionToken + "&filter=" + PlayerId))
        using (UnityWebRequest webRequest = UnityWebRequest.Get(ServerURL + "/bubblebots/score"))
        {
            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.SetRequestHeader("Accept", "*/*");

            yield return webRequest.SendWebRequest();

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError("Connection failed for reason: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    string data = webRequest.downloadHandler.text;
                    Debug.Log("Received data (scores): " + data);

                    string playerName;
                    JArray scoreData = JArray.Parse(data.ToString());
                    for (int i = 0; i < scoreData.Count; i++)
                    {
                        if (scoreData[i]["nickname"] != null)
                        {
                            playerName = scoreData[i]["nickname"].ToString();
                        } else {
                            playerName = "Player " + UnityEngine.Random.Range(1000, 10000);/*scoreData[i]["full_name"].ToString();*/
                        }

                        if (scoreData[i]["address"].ToString() == PlayerWalletAddress)
                        {
                            PlayerFullName = playerName;
                            Rank = i + 1;
                        }

                        result.Add(new ScoreInfo(int.Parse(scoreData[i]["score"].ToString()), playerName, ""/*scoreData[i]["country"].ToString()*/, i + 1, scoreData[i]["address"].ToString() == PlayerWalletAddress));
                    }

                    break;
            }
        }

        onComplete?.Invoke(result);
    }

    public void SetFullName(string fullName)
    {
        StartCoroutine(SetFullNameNow(fullName));
    }

    public IEnumerator SetFullNameNow(string fullName)
    {
        PlayerFullName = fullName;
        ObscuredPrefs.Set("full_name", PlayerFullName);

        string formData = "{\"address\":\"" + PlayerWalletAddress + "\",\"nickname\":\"" + fullName.Replace("\"", "'").Trim() + "\"}";

        //using (UnityWebRequest webRequest = UnityWebRequest.Get(ServerURL + "/set_fullname.php?user_name=" + PlayerId + "&session_token=" + SessionToken + "&full_name=" + fullName))
        using (UnityWebRequest webRequest = UnityWebRequest.Post(ServerURL + "/bubblebots/nickname", formData))
        {
            UploadHandler customUploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(formData));
            customUploadHandler.contentType = "application/json";
            webRequest.uploadHandler = customUploadHandler;

            webRequest.SetRequestHeader("Authorization", "Bearer " + SessionToken);
            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.SetRequestHeader("Accept", "*/*");

            yield return webRequest.SendWebRequest();

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError("Connection failed for reason: " + webRequest.error);

                    gui.DisplayPlayerInfo(false);
                    gui.DisplayPlayerInfoLoading(false);
                    gui.DisplayPlayerOffline(true);
                    break;
                case UnityWebRequest.Result.Success:
                    string data = webRequest.downloadHandler.text;
                    Debug.Log("Received data (set full name): " + data);

                    JObject o = JObject.Parse(data);
                    if (o.ContainsKey("rank"))
                    {
                        Debug.Log("Full name has been set to " + fullName);

                        Rank = int.Parse(o["rank"].ToString());
                        ObscuredPrefs.Get("rank", Rank);
                    }
                    else
                    {
                        gui.DisplayPlayerInfo(false);
                        gui.DisplayPlayerInfoLoading(false);
                        gui.DisplayPlayerOffline(true);

                        Debug.LogError(o["result"].ToString());
                    }

                    break;
            }
        }
    }
}