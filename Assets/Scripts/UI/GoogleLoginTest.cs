using BubbleBots.Server.Signature;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.UI;

public class GoogleLoginTest : MonoBehaviour
{
    [SerializaField] public Button serverLoginButton;
    [SerializaField] public Button googleLoginButton;
    [SerializaField] public TMPro.TextMeshProUGUI output;
    // Start is called before the first frame update


    private void Awake()
    {
        serverLoginButton.enabled = false;
        googleLoginButton.enabled = true;
    }


    internal void ProcessAuthentication(bool success, string code)
    {
        if (success)
        {
            output.text = "";
            output.text += "\n" + "Auth succesfull";
            output.text += "\n" + code;
            output.text += "\n" + "user name: " + Social.localUser.userName;
            output.text += "\n" + "idToken: " + PlayGamesPlatform.Instance.GetIdToken();
            output.text += "\n" + "AuthCode: " + PlayGamesPlatform.Instance.GetServerAuthCode();
            googleLoginButton.enabled = false;
            serverLoginButton.enabled = true;
            PlayGamesPlatform.Instance.GetAnotherServerAuthCode(true, (code) => {
                output.text += "\n" + "AuthCode 2: " + code;
            });
        }
        else
        {
            output.text = "";
            output.text += "\n" + "Auth failed " + code;
        }
        Canvas.ForceUpdateCanvases();
    }

    public void LoginGoogle()
    {
        var config = new PlayGamesClientConfiguration.Builder()
        .AddOauthScope("profile")
        .AddOauthScope("profile")
        .RequestEmail()
        .RequestIdToken()
        .RequestServerAuthCode(false)
        .Build();

        PlayGamesPlatform.InitializeInstance(config);
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();

        Social.localUser.Authenticate(ProcessAuthentication);
    }

    public void LoginServer()
    {
        GoogleLogin loginData = new GoogleLogin()
        {
            accessToken = PlayGamesPlatform.Instance.GetIdToken()
        };
        string formData = JsonUtility.ToJson(loginData);
        ServerManager.Instance.SendLoginDataToServer(SignatureLoginAPI.GoogleLogin, formData,
            (success) =>
        {
            output.text += "\n" + "Server login successfull";
            output.text += "\n" + success;
        },
            (fail) =>
        {
            output.text += "\n" + "Server login fail";
            output.text += "\n" + fail;
        }
        );
    }

}


