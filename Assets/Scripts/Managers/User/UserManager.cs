using System;
using System.Collections;
using System.Collections.Generic;
using BubbleBots.Server.Player;
using BubbleBots.User;
using CodeStage.AntiCheat.Storage;
using UnityEditor;
using UnityEngine;

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
            UserName = ObscuredPrefs.Get(prefsKeyMap[PrefsKey.Nickname], ""),
            WalletAddress = ObscuredPrefs.Get(prefsKeyMap[PrefsKey.WalletAddress], ""),
            SessionToken = ObscuredPrefs.Get(prefsKeyMap[PrefsKey.SessionToken], ""),
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

    private void Start()
    {
        if (!string.IsNullOrEmpty(CurrentUser.WalletAddress))
        {
            CrashManager.Instance.SetCustomCrashKey(CrashTypes.WalletAddress, CurrentUser.WalletAddress);
        }
    }

    public void SetWalletAddress(string address)
    {
        if (string.IsNullOrEmpty(address))
        {
            Debug.LogWarning("Address empty: " + address);
            return;
        }
        CurrentUser.WalletAddress = address;
        CrashManager.Instance.SetCustomCrashKey(CrashTypes.WalletAddress, CurrentUser.WalletAddress);
        ObscuredPrefs.Set(prefsKeyMap[PrefsKey.WalletAddress], address);
    }

    public void SetSignature(string signature)
    {
        if (string.IsNullOrEmpty(signature))
        {
            Debug.LogWarning("Signature empty: " + signature);
            return;
        }
        CurrentUser.Signature = signature;
        ObscuredPrefs.Set(prefsKeyMap[PrefsKey.Signature], signature);
    }

    public void SetJwtToken(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            Debug.LogWarning("Token empty: " + token);
            return;
        }
        CurrentUser.SessionToken = token;
        ObscuredPrefs.Set(prefsKeyMap[PrefsKey.SessionToken], token);
    }

    public void SetPlayerUserName(string userName, bool sendToServer)
    {
        if (string.IsNullOrEmpty(userName))
        {
            Debug.LogWarning("Username empty: " + userName);
            return;
        }
        CurrentUser.UserName = userName;
        ObscuredPrefs.Set(prefsKeyMap[PrefsKey.Nickname], userName);
        if (sendToServer)
        {
            string sanitizedUsername = userName.Replace("\"", "'").Trim();
            ChangeUserNameData formData = new()
            {
                signature = Instance.GetPlayerSignature(),
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

    public string GetPlayerJwtToken()
    {
        return CurrentUser.SessionToken;
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

    public void GetPlayerResources()
    {
        ServerManager.Instance.GetPlayerDataFromServer(PlayerAPI.Wallet, (jsonData) =>
        {
            GetPlayerWallet walletData = JsonUtility.FromJson<GetPlayerWallet>(jsonData);
            CallbackWithResources?.Invoke(walletData);
        }, CurrentUser.WalletAddress);
    }

    private IEnumerator CallGetPlayerResourcesAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        GetPlayerResources();
    }

    public void GetPlayerResourcesAfter(float seconds)
    {
        StartCoroutine(CallGetPlayerResourcesAfter(seconds));
    }

#if UNITY_EDITOR
    [MenuItem("Peanut Games/Clear Prefs")]
    public static void ClearPrefs()
    {
        ObscuredPrefs.DeleteAll();
    }
#endif
}