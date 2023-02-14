using System;
using System.Collections;
using System.Collections.Generic;
using BubbleBots.Server.Player;
using BubbleBots.User;
using CodeStage.AntiCheat.Storage;
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

    public List<Sprite> PlayerAvatars;
    
    private User CurrentUser;

    private readonly Dictionary<PrefsKey, string> prefsKeyMap = new()
    {
        { PrefsKey.Nickname, "full_name" },
        { PrefsKey.WalletAddress, "wallet_address" },
        { PrefsKey.SessionToken, "session_token" },
        { PrefsKey.Signature, "signature" },
        { PrefsKey.Hints, "hints"},
        { PrefsKey.Avatar, "avatar"}
    };
    
    private readonly Dictionary<PrefsKeyToDelete, string> prefsKeysToDeleteMap = new()
    {
        { PrefsKeyToDelete.Rank, "rank" }
    };

    private void DeleteOldKeys()
    {
        ObscuredPrefs.DeleteKey(prefsKeysToDeleteMap[PrefsKeyToDelete.Rank]);
    }
    
    private void GetUserOrSetDefault()
    {
        DeleteOldKeys();
        CurrentUser = new()
        {
            UserName = ObscuredPrefs.Get(prefsKeyMap[PrefsKey.Nickname], ""),
            WalletAddress = ObscuredPrefs.Get(prefsKeyMap[PrefsKey.WalletAddress], ""),
            SessionToken = ObscuredPrefs.Get(prefsKeyMap[PrefsKey.SessionToken], ""),
            Signature = ObscuredPrefs.Get(prefsKeyMap[PrefsKey.Signature], ""),
            Hints = ObscuredPrefs.Get(prefsKeyMap[PrefsKey.Hints], true), 
            Avatar = ObscuredPrefs.Get(prefsKeyMap[PrefsKey.Avatar], 0) 
        };
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

    public void SetPlayerUserName(string userName, bool sendToServer, Action<string> onSuccess = null, Action<string> onFail = null)
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
            ServerManager.Instance.SendPlayerDataToServer(PlayerAPI.UpdateNickname, jsonFormData, onSuccess, onFail);
        }
    }

    public void ChangePlayerAvatar(int avatar)
    {
        CurrentUser.Avatar = avatar;
        ObscuredPrefs.Set(prefsKeyMap[PrefsKey.Avatar], avatar);
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

    public int GetPlayerAvatar()
    {
        return CurrentUser.Avatar;
    }
    
    public void SetPlayerHints(bool hints)
    {
        CurrentUser.Hints = true;
        ObscuredPrefs.Set(prefsKeyMap[PrefsKey.Hints], hints);
    }
    
    public bool GetPlayerHints()
    {
        return CurrentUser.Hints;
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

    public static void ClearPrefs()
    {
        ObscuredPrefs.DeleteAll();
    }
}