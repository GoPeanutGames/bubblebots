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
    public string PlayerWalletAddress = null;
    [HideInInspector]
    public ObscuredLong Score = 0;
    [HideInInspector]
    public ObscuredInt Rank = int.MaxValue;

    public ObscuredString PlayerId = "tolgak";
    public ObscuredString Password = "123";
    public delegate void LeaderboardEvent(object param);

    string SessionToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJhdXRoIjoiYWYtdXNlciIsImFnZW50IjoiIiwidG9rZW4iOiJmcmV5LXBhcmstc3RhdmUtaHVydGxlLXNvcGhpc20tbW9uYWNvLW1ha2VyLW1pbm9yaXR5LXRoYW5rZnVsLWdyb2Nlci11bmNpYWwtcG9uZ2VlIiwiaWF0IjoxNjYzNjk4NDkzfQ.wEOeF3Up1aJOtFUOLWB4AGKf-NBS609UoL4kIgrSGms";
    GUIMenu gui;

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
        } else
        {
            PlayerFullName = "Player" + Random.Range(1000, 10000);
        }

        if (ObscuredPrefs.HasKey("wallet_address"))
        {
            PlayerWalletAddress = ObscuredPrefs.Get("wallet_address", "");
        }

        if (ObscuredPrefs.HasKey("rank"))
        {
            Rank = ObscuredPrefs.Get("rank", 9999);
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Start()
    {
        gui = FindObjectOfType<GUIMenu>();
    }

    public void SaveScore(long score)
    {
        this.Score = score;

        StartCoroutine(SaveScoreNow(score));
    }

    private IEnumerator SaveScoreNow(long score)
    {
        string formData = "{\"address\":\"" + PlayerWalletAddress + "\",\"score\":" + score.ToString().Replace("\"", "'").Trim() + "}";
        using (UnityWebRequest webRequest = UnityWebRequest.Post(ServerURL + "/bubblebots/score", formData))
        //using (UnityWebRequest webRequest = UnityWebRequest.Get(ServerURL + "/set_score.php?user_name=" + PlayerId + "&session_token=" + SessionToken + "&score=" + score))
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
                    break;
                case UnityWebRequest.Result.Success:
                    string data = webRequest.downloadHandler.text;
                    Debug.Log("Received data (set score): " + data);

                    JObject o = JObject.Parse(data);
                    if (o["rank"] != null)
                    {
                        Debug.Log("Score has been set with success");
                        Rank = int.Parse(o["rank"].ToString());
                        Score = score;

                        ObscuredPrefs.Set("rank", (int)Rank);
                        gui.DisplayPlayerRank();
                    }
                    else
                    {
                        Debug.LogError("Score could not be set (" + data + ")");
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

                    JObject o = JObject.Parse(data);
                    score = long.Parse(o["score"].ToString());
                    if (o["rank"] != null)
                    {
                        rank = int.Parse(o["rank"].ToString());
                        ObscuredPrefs.Get("rank", rank);
                        Rank = rank;
                    }

                    if (o["nickname"] != null)
                    {
                        PlayerFullName = o["nickname"].ToString();
                    }

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

    public void SetPlayerWalletAddress(string address)
    {
        PlayerWalletAddress = address;
        ObscuredPrefs.Set("wallet_address", PlayerWalletAddress);
    }
}