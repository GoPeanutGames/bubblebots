using System;
using BubbleBots.Server.Signature;
using UnityEngine;

public class AutoLogin
{
    private Action<User> _successCallback;
    private Action<string> _failCallback;

    public void TryAutoLogin(Action<User> success, Action<string> fail)
    {
        _successCallback = success;
        _failCallback = fail;
        string jwtToken = UserManager.Instance.GetPlayerJwtToken();
        if (!string.IsNullOrEmpty(jwtToken))
        {
            TryLogin();
        }
        else
        {
            _failCallback?.Invoke("No Token saved");
        }
    }

    private void TryLogin()
    {
        ServerManager.Instance.GetLoginSignatureDataFromServer(SignatureLoginAPI.AutoLoginGet, AutoLoginSuccess, "", AutoLoginFail);
    }

    private void AutoLoginSuccess(string result)
    {
        User user = JsonUtility.FromJson<User>(result);
        _successCallback?.Invoke(user);
    }

    private void AutoLoginFail(string result)
    {
        _failCallback?.Invoke(result);
    }
}