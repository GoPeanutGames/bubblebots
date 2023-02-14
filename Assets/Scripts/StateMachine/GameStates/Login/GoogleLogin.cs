using System;
using BubbleBots.Server.Signature;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine;

public class GoogleLogin
{
    private Action<LoginResult> _successCallback;
    private Action<string> _failCallback;
    
    public void StartLogin(Action<LoginResult> success, Action<string> fail)
    {
        _successCallback = success;
        _failCallback = fail;
        LoginWithGoogle();
    }
    
    private void LoginWithGoogle()
    {
#if UNITY_ANDROID
        var config = new PlayGamesClientConfiguration.Builder()
            .AddOauthScope("profile")
            .AddOauthScope("email")
            .RequestEmail()
            .RequestIdToken()
            .RequestServerAuthCode(false)
            .Build();

        PlayGamesPlatform.InitializeInstance(config);
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();


        Social.localUser.Authenticate(ProcessAuthentication);
#endif
    }
    
    internal void ProcessAuthentication(bool success, string code)
    {
#if UNITY_ANDROID
        if (success)
        {
            Debug.Log("google token " + PlayGamesPlatform.Instance.GetIdToken());
            LoginOnServerWithGoogleToken(PlayGamesPlatform.Instance.GetIdToken());
        }
        else
        {
            _failCallback?.Invoke("Google Failed, code: " + code);
        }
#endif
    }
    
    private void LoginOnServerWithGoogleToken(string token)
    {
        GoogleLoginData googleLoginData = new GoogleLoginData()
        {
            accessToken = token
        };
        string formData = JsonUtility.ToJson(googleLoginData);
        ServerManager.Instance.SendLoginDataToServer(SignatureLoginAPI.GoogleLogin, formData, LoginServerSuccess, LoginServerFail);
    }

    private void LoginServerSuccess(string result)
    {
        LoginResult loginResult = JsonUtility.FromJson<LoginResult>(result);
        _successCallback?.Invoke(loginResult);
    }

    private void LoginServerFail(string fail)
    {
        _failCallback?.Invoke("Login server failed, code: " + fail);
    }
}