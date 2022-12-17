using System;
using System.Collections.Generic;
using BubbleBots.Server.Player;
using BubbleBots.User;
using CodeStage.AntiCheat.ObscuredTypes;
using CodeStage.AntiCheat.Storage;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public enum PlayerType
{
    Guest,
    LoggedInUser
}

public class UserManager : MonoSingleton<UserManager>
{
    public static PlayerType PlayerType;
    public static int RobotsKilled = 0;
    public static Action<GetPlayerWallet> CallbackWithResources;

    private User CurrentUser;

    private ObscuredString sessionToken =
        "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJhdXRoIjoiYWYtdXNlciIsImFnZW50IjoiIiwidG9rZW4iOiJmcmV5LXBhcmstc3RhdmUtaHVydGxlLXNvcGhpc20tbW9uYWNvLW1ha2VyLW1pbm9yaXR5LXRoYW5rZnVsLWdyb2Nlci11bmNpYWwtcG9uZ2VlIiwiaWF0IjoxNjYzNjk4NDkzfQ.wEOeF3Up1aJOtFUOLWB4AGKf-NBS609UoL4kIgrSGms";

    private readonly Dictionary<PrefsKey, string> prefsKeyMap = new()
    {
        { PrefsKey.Nickname, "full_name" },
        { PrefsKey.WalletAddress, "wallet_address" },
        { PrefsKey.SessionToken, "session_token" },
        { PrefsKey.Rank, "rank" },
        { PrefsKey.Signature, "signature" }
    };

    private void GetUserOrSetDefault()
    {
        CurrentUser = new()
        {
            UserName = ObscuredPrefs.Get(prefsKeyMap[PrefsKey.Nickname],
                "Player" + Random.Range(1000, 10000)),
            WalletAddress = ObscuredPrefs.Get(prefsKeyMap[PrefsKey.WalletAddress], ""),
            SessionToken =
                ObscuredPrefs.Get<string>(ObscuredPrefs.Get(prefsKeyMap[PrefsKey.SessionToken], sessionToken)),
            Score = 0,
            Rank = ObscuredPrefs.Get(prefsKeyMap[PrefsKey.Rank], 9999),
            Signature = ObscuredPrefs.Get(prefsKeyMap[PrefsKey.Signature], "")
        };
    }

    private void OnNicknameSet(string data)
    {
        Debug.Log("Nickname set");
    }

    protected override void Awake()
    {
        base.Awake();
        GetUserOrSetDefault();
    }

    public void SetWalletAddress(string address)
    {
        CurrentUser.WalletAddress = address;
        ObscuredPrefs.Set(prefsKeyMap[PrefsKey.WalletAddress], address);
    }

    public void SetSignature(string signature)
    {
        CurrentUser.Signature = signature;
        ObscuredPrefs.Set(prefsKeyMap[PrefsKey.Signature], signature);
    }

    public void SetPlayerUserName(string userName, bool sendToServer)
    {
        if (string.IsNullOrEmpty(userName))
        {
            return;
        }

        CurrentUser.UserName = userName;
        ObscuredPrefs.Set(prefsKeyMap[PrefsKey.Nickname], userName);
        if (sendToServer)
        {
            string sanitizedUsername = userName.Replace("\"", "'").Trim();
            ChangeUserNameData formData = new()
            {
                address = CurrentUser.WalletAddress,
                nickname = sanitizedUsername
            };
            string jsonFormData = JsonUtility.ToJson(formData);
            ServerManager.Instance.SendPlayerDataToServer(PlayerAPI.UpdateNickname, jsonFormData, OnNicknameSet);
        }
    }

    public string GetPlayerWalletAddress()
    {
        return CurrentUser.WalletAddress;
    }

    public string GetPlayerUserName()
    {
        return CurrentUser.UserName;
    }

    public string GetPlayerSignature()
    {
        return CurrentUser.Signature;
    }
    
    public void SetPlayerRank(int rank)
    {
        CurrentUser.Rank = rank;
        ObscuredPrefs.Set(prefsKeyMap[PrefsKey.Rank], rank);
    }

    public int GetPlayerRank()
    {
        return CurrentUser.Rank;
    }

    public void SetPlayerScore(int score)
    {
        CurrentUser.Score = score;
    }

    public int GetPlayerScore()
    {
        return CurrentUser.Score;
    }

    //stub
    private const string playerBubblesKey = "playerBubbles";
    public int GetBubbles()
    {
        return PlayerPrefs.GetInt("playerBubblesKey");
    }

    public void AddBubbles(int bubbles)
    {
        int currentBubbles = GetBubbles();
        currentBubbles += bubbles;
        PlayerPrefs.SetInt("playerBubblesKey", currentBubbles);
    }

    public void GetTop100Scores(Action<string> onComplete)
    {
        ServerManager.Instance.GetPlayerDataFromServer(PlayerAPI.Top100, onComplete);
    }

    public void GetPlayerResources()
    {
        ServerManager.Instance.GetPlayerDataFromServer(PlayerAPI.Wallet, (jsonData) =>
        {
            GetPlayerWallet walletData = JsonUtility.FromJson<GetPlayerWallet>(jsonData);
            CallbackWithResources?.Invoke(walletData);
        }, CurrentUser.WalletAddress);
    }

#if UNITY_EDITOR
    [MenuItem("Peanut Games/Clear Prefs")]
    public static void ClearPrefs()
    {
        ObscuredPrefs.DeleteAll();
    }
#endif
}