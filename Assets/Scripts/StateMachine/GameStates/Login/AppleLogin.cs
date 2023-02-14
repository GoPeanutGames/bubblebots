using System;
using System.Text;
using AppleAuth;
using AppleAuth.Enums;
using AppleAuth.Interfaces;
using AppleAuth.Native;
using BubbleBots.Server.Signature;
using UnityEngine;

public class AppleLogin
{
    private IAppleAuthManager _appleAuthManager;

    private Action<LoginResult> _successCallback;
    private Action<string> _failCallback;

    public void StartLogin(Action<LoginResult> success, Action<string> fail)
    {
        _successCallback = success;
        _failCallback = fail;
        PayloadDeserializer deserializer = new PayloadDeserializer();
        _appleAuthManager = new AppleAuthManager(deserializer);
        Login();
    }

    public void Update()
    {
        _appleAuthManager?.Update();
    }

    private void Login()
    {
        var loginArgs = new AppleAuthLoginArgs(LoginOptions.IncludeEmail | LoginOptions.IncludeFullName);

        // Perform the login
        _appleAuthManager.LoginWithAppleId(
            loginArgs,
            credential =>
            {
                if (credential is IAppleIDCredential appleIDCredential)
                {
                    var idToken = Encoding.UTF8.GetString(
                        appleIDCredential.IdentityToken,
                        0,
                        appleIDCredential.IdentityToken.Length);
                    LoginOnServerWithApple(idToken);
                }
                else
                {
                    _failCallback("Sign-in with Apple error. Message: appleIDCredential is null");
                }
            },
            error =>
            {
                _failCallback("Sign-in with Apple error. Message: " + error);
            }
        );
    }

    private void LoginOnServerWithApple(string idToken)
    {
        AppleLoginData appleLoginData = new AppleLoginData()
        {
            appleToken = idToken
        };
        string formData = JsonUtility.ToJson(appleLoginData);
        ServerManager.Instance.SendLoginDataToServer(SignatureLoginAPI.AppleLogin, formData, LoginServerSuccess, LoginServerFail);
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