using CodeStage.AntiCheat.ObscuredTypes;
using CodeStage.AntiCheat.Storage;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SocialPlatforms.Impl;
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
    public ObscuredInt Rank = 0; // int.MaxValue;

    public ObscuredString PlayerId = "tolgak";
    public ObscuredString Password = "123";
    public delegate void LeaderboardEvent(object param);
    public bool GuestMode
    {
        get
        {
            return guestMode;
        }
    }
    public ObscuredInt RobotsKilled
    {
        get
        {
            return robotsKilled;
        }
    }

    ObscuredInt robotsKilled = 0;
    bool guestMode = false;
    string crptoPassword;
    string SessionToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJhdXRoIjoiYWYtdXNlciIsImFnZW50IjoiIiwidG9rZW4iOiJmcmV5LXBhcmstc3RhdmUtaHVydGxlLXNvcGhpc20tbW9uYWNvLW1ha2VyLW1pbm9yaXR5LXRoYW5rZnVsLWdyb2Nlci11bmNpYWwtcG9uZ2VlIiwiaWF0IjoxNjYzNjk4NDkzfQ.wEOeF3Up1aJOtFUOLWB4AGKf-NBS609UoL4kIgrSGms";
    GUIMenu gui;

    public static LeaderboardManager Instance;

    private void Awake()
    {
        crptoPassword = "JXmnPkqMiqUR.N-7tvBLrYmkv8xcYgDV"; // "JyK!RBEL9pjzvGa-fZsPuPG.VRpyBQ@j";

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
            PlayerFullName = "Player" + UnityEngine.Random.Range(1000, 10000);
        }

        if (ObscuredPrefs.HasKey("wallet_address"))
        {
            PlayerWalletAddress = ObscuredPrefs.Get("wallet_address", "");
        }

        if (ObscuredPrefs.HasKey("rank"))
        {
            Rank = ObscuredPrefs.Get("rank", 9999);
        }

        guestMode = PlayerWalletAddress == null;
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

#if UNITY_EDITOR
    [MenuItem("Peanut Games/Clear Session")]
    static void ClearSession()
    {
        ObscuredPrefs.Set("wallet_address", "");
        Debug.Log("Player wallet address is cleared");
    }

    [MenuItem("Peanut Games/Chiper Test")]
    static void ChiperTest()
    {
        string data = "{\"address\":\"0xb354B942464755968a655144Bb1f69422b7D5ea2\",\"score\":10}";
        string password = "JyK!RBEL9pjzvGa-fZsPuPG.VRpyBQ@j";
        //string password = "JXmnPkqMiqUR.N-7tvBLrYmkv8xcYgDV";
        Debug.Log(SimpleAESEncryption.Encrypt2(data, password)); // JXmnPkqMiqUR.N-7tvBLrYmkv8xcYgDV
    }

    [MenuItem("Peanut Games/Dechiper Test")]
    static void DechiperTest()
    {
        //string data = "23809ed892807f7d3759d81ee59515163e6773410865db466ad99ac6586d8259da50bac00c2d5b74609df3e21dd48f57429d2a6af28a9e1937d6562498089f5f33f9092e2d215deb12cd8a82ccc4deafd9e31070059f9223bd11f0dc3fdeb115";
        string data = "64beffce23ccb3220a9dcf52c42760a77e294b2be35c91b9e2568eac4b498ddeca07f3a9e9ba82b75076653231e4e7665b1adff947eb4adf348517ee5b051dce37c5c88f19a1d54e11a2ac1c5a268e444acb9c443a36ff66cfe9275ad8ef7249";
        string password = "JyK!RBEL9pjzvGa-fZsPuPG.VRpyBQ@j";
        //string password = "JXmnPkqMiqUR.N-7tvBLrYmkv8xcYgDV";
        Debug.Log(SimpleAESEncryption.Decrypt2(data, password));
    }

#endif

    public void Start()
    {
        gui = FindObjectOfType<GUIMenu>();
    }

    public void SetGuestMode()
    {
        guestMode = true;
        PlayerWalletAddress = null;
    }

    public void SaveScore(long score)
    {
        if (guestMode)
        {
            return;
        }

        this.Score = score;

        StartCoroutine(SaveScoreNow(score));
    }

    private IEnumerator SaveScoreNow(long score)
    {
        string formData = "{\"address\":\"" + PlayerWalletAddress + "\",\"score\":" + score.ToString().Replace("\"", "'").Trim() + "}";
        formData = "{\"data\":\"" + SimpleAESEncryption.Encrypt2(formData, crptoPassword) + "\"}";
        //Debug.Log(ServerURL + "/bubblebots/score");
        using (UnityWebRequest webRequest = UnityWebRequest.Post(ServerURL + "/bubblebots/score", formData))
        {
            UploadHandler customUploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(formData));
            customUploadHandler.contentType = "application/json";
            webRequest.uploadHandler = customUploadHandler;

            webRequest.SetRequestHeader("Authorization", "Bearer " + SessionToken);
            webRequest.SetRequestHeader("Access-Control-Allow-Origin", "*");
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
                    //Debug.Log("Received data (set score): " + data);

                    JObject o = JObject.Parse(data);
                    if (o["rank"] != null)
                    {
                        //Debug.Log("Score has been set with success");
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
        if (guestMode)
        {
            onComplete?.Invoke("0,0");
            return;
        }

        StartCoroutine(GetPlayerScoreNow(onComplete));
    }

    private IEnumerator GetPlayerScoreNow(LeaderboardEvent onComplete)
    {
        ObscuredLong score = 0;
        int rank = int.MaxValue;

        //ServerURL = Environment.GetEnvironmentVariable("API_URL");
        using (UnityWebRequest webRequest = UnityWebRequest.Get(ServerURL + "/bubblebots/score/" + PlayerWalletAddress))
        {
            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.SetRequestHeader("Access-Control-Allow-Origin", "*");
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
                    //Debug.Log("Received data (get player score): " + data);

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
        //ServerURL = Environment.GetEnvironmentVariable("API_URL");
        using (UnityWebRequest webRequest = UnityWebRequest.Get(ServerURL + "/bubblebots/score"))
        {
            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.SetRequestHeader("Access-Control-Allow-Origin", "*");
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
                    //Debug.Log("Received data (scores): " + data);

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
        if (guestMode)
        {
            return;
        }

        StartCoroutine(SetFullNameNow(fullName));
    }

    public IEnumerator SetFullNameNow(string fullName)
    {
        PlayerFullName = fullName;
        ObscuredPrefs.Set("full_name", PlayerFullName);

        string formData = "{\"address\":\"" + PlayerWalletAddress + "\",\"nickname\":\"" + fullName.Replace("\"", "'").Trim() + "\"}";
        formData = "{\"data\":\"" + SimpleAESEncryption.Encrypt2(formData, crptoPassword) + "\"}";
        Debug.Log(formData);
        Debug.Log(ServerURL + "/bubblebots/nickname");

        //using (UnityWebRequest webRequest = UnityWebRequest.Get(ServerURL + "/set_fullname.php?user_name=" + PlayerId + "&session_token=" + SessionToken + "&full_name=" + fullName))
        //ServerURL = Environment.GetEnvironmentVariable("API_URL");
        using (UnityWebRequest webRequest = UnityWebRequest.Post(ServerURL + "/bubblebots/nickname", formData))
        {
            UploadHandler customUploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(formData));
            customUploadHandler.contentType = "application/json";
            webRequest.uploadHandler = customUploadHandler;

            //SessionToken = Environment.GetEnvironmentVariable("SECREAT_HEADER");
            webRequest.SetRequestHeader("Authorization", "Bearer " + SessionToken);
            webRequest.SetRequestHeader("Access-Control-Allow-Origin", "*");
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
                        //Debug.Log("Full name has been set to " + fullName);

                        Rank = int.Parse(o["rank"].ToString());
                        ObscuredPrefs.Get("rank", (int)Rank);
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
        if (guestMode)
        {
            return;
        }

        PlayerWalletAddress = address;
        ObscuredPrefs.Set("wallet_address", PlayerWalletAddress);
    }

    public void ResetKilledRobots()
    {
        robotsKilled = 0;
    }

    public void IncrementKilledRobots()
    {
        robotsKilled += 1;
    }
}