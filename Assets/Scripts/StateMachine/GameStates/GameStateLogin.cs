using System;
using System.Text;
using BubbleBots.Server.Player;
using BubbleBots.Server.Signature;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine;

public class GameStateLogin : GameState
{
    private GameScreenLogin gameScreenLogin;
    private string tempAddress;
    private string tempSignature;
    private string tempEmail;
    private string tempHashedPass;

    public override string GetGameStateName()
    {
        return "game state login";
    }

    public override void Enable()
    {
        SoundManager.Instance.FadeInMusic();
        SoundManager.Instance.PlayStartMusicNew();
        gameScreenLogin = Screens.Instance.PushScreen<GameScreenLogin>();
        GameEventsManager.Instance.AddGlobalListener(OnGameEvent);
        //todo: figure stuff out for login
        if (false)
        {
            TryLoginFromSave();
        }
    }

    private void TryLoginFromSave()
    {
        string address = UserManager.Instance.GetPlayerWalletAddress();
        string signature = UserManager.Instance.GetPlayerSignature();
        if (string.IsNullOrEmpty(address) == false && string.IsNullOrEmpty(signature) == false)
        {
            PostWeb3Login web3LoginData = new PostWeb3Login()
            {
                address = address,
                signature = signature
            };
            string data = JsonUtility.ToJson(web3LoginData);
            ServerManager.Instance.SendLoginDataToServer(SignatureLoginAPI.Web3LoginCheck, data, (res) =>
            {
                ResponseWeb3Login loginResponse = JsonUtility.FromJson<ResponseWeb3Login>(res);
                if (loginResponse.status)
                {
                    StartLogin(address, signature);
                }
            });
        }
    }

    private void OnGameEvent(GameEventData gameEvent)
    {
        if (gameEvent.eventName == GameEvents.ButtonTap)
        {
            OnButtonTap(gameEvent);
        }
    }

    private void OnButtonTap(GameEventData data)
    {
        GameEventString buttonTapData = data as GameEventString;
        switch (buttonTapData.stringData)
        {
            case ButtonId.LoginGuest:
                gameScreenLogin.OnPlayAsGuestPressed();
                break;
            case ButtonId.LoginMobileDownload:
                Application.OpenURL("https://peanutgames.com/");
                break;
            case ButtonId.LoginEmailPassSignUpSubmit:
                SignUpEmailPass();
                break;
            case ButtonId.LoginEmailPassLoginSubmit:
                LoginEmailPass();
                break;
            case ButtonId.LoginEmailPassLogin:
                gameScreenLogin.ShowLoginScreen1stStep();
                break;
            case ButtonId.LoginEmailPassSignUp:
                gameScreenLogin.ShowEmailPassSignupScreen();
                break;
            case ButtonId.LoginEmailPass:
                gameScreenLogin.ShowEmailPassLoginSignupScreen();
                break;
            case ButtonId.LoginEmailPassSignUpLogin2ndStep:
                Login2ndStep();
                break;
            case ButtonId.LoginGuestPlay:
#if UNITY_EDITOR
                // PlayAsGuest();
                GoToMainMenu();
                UserManager.PlayerType = PlayerType.Guest;
                //UserManager.PlayerType = PlayerType.LoggedInUser;
                AnalyticsManager.Instance.InitAnalyticsGuest();
#else
                PlayAsGuest();
#endif
                break;
            case ButtonId.LoginGoogle:
                LoginWithGoogle();
                break;
        }
    }

    private void SignUpEmailPass()
    {
        string email = gameScreenLogin.GetSignUpInputFieldEmail();
        string pass = gameScreenLogin.GetSignUpInputFieldPass();
        var provider = new System.Security.Cryptography.SHA256Managed();
        var hash = provider.ComputeHash(Encoding.UTF8.GetBytes(pass));
        string hashString = string.Empty;
        foreach (byte x in hash)
        {
            hashString += $"{x:x2}";
        }
        tempEmail = email;
        tempHashedPass = hashString;
        EmailPassSignUp data = new EmailPassSignUp()
        {
            email = email,
            password = hashString
        };
        string formData = JsonUtility.ToJson(data);
        ServerManager.Instance.SendLoginDataToServer(SignatureLoginAPI.EmailPassSignUp, formData, EmailPassSignUpSuccess, EmailPassSignUpFail);
    }

    private void EmailPassSignUpSuccess(string success)
    {
        gameScreenLogin.ShowSignUpLogin2ndStep();
    }

    private void EmailPassSignUpFail(string error)
    {
        Debug.Log("error: " + error);
    }

    private void LoginEmailPass()
    {
        string email = gameScreenLogin.GetLoginInputFieldEmail();
        string pass = gameScreenLogin.GetLoginInputFieldPass();
        var provider = new System.Security.Cryptography.SHA256Managed();
        var hash = provider.ComputeHash(Encoding.UTF8.GetBytes(pass));
        string hashString = string.Empty;
        foreach (byte x in hash)
        {
            hashString += $"{x:x2}";
        }
        tempEmail = email;
        tempHashedPass = hashString;
        EmailPassSignUp data = new EmailPassSignUp()
        {
            email = email,
            password = hashString
        };
        string formData = JsonUtility.ToJson(data);
        ServerManager.Instance.SendLoginDataToServer(SignatureLoginAPI.Login1stStep, formData, EmailPassSignUpSuccess, EmailPassSignUpFail);
    }
    
    private void Login2ndStep()
    {
        string code = gameScreenLogin.GetLoginInputFieldCode();
        Login2ndStep data = new Login2ndStep()
        {
            email = tempEmail,
            password = tempHashedPass,
            twoFaCode = code
        };
        string formData = JsonUtility.ToJson(data);
        ServerManager.Instance.SendLoginDataToServer(SignatureLoginAPI.Login2ndStep, formData, Login2ndStepSuccess, Login2ndStepFail);
    }

    private void Login2ndStepSuccess(string success)
    {
        Debug.Log("success: " + success);
        LoginResult result = JsonUtility.FromJson<LoginResult>(success);
        StartLogin(result.web3Info.address, result.web3Info.signature);
    }

    private void Login2ndStepFail(string error)
    {
        Debug.Log("error: " + error);
    }

    private void PlayAsGuest()
    {
        UserManager.PlayerType = PlayerType.Guest;
        AnalyticsManager.Instance.InitAnalyticsGuest();
        gameScreenLogin.HideLoadingScreen();
        GoToMainMenu();
        //stateMachine.PushState(new GameStateFreeMode());
    }

    private void GoToMainMenu()
    {
        stateMachine.PushState(new GameStateMainMenu());
    }

    private void LoginWithGoogle()
    {
#if UNITY_ANDROID
        gameScreenLogin.ShowLoadingScreen();
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

    internal void LoginOnServerWithGoogleToken(string token)
    {
        GoogleLogin loginData = new GoogleLogin()
        {
            accessToken = token
        };
        string formData = JsonUtility.ToJson(loginData);
        ServerManager.Instance.SendLoginDataToServer(SignatureLoginAPI.GoogleLogin, formData,
            (success) =>
            {
                LoginResult result = JsonUtility.FromJson<LoginResult>(success);
                StartLogin(result.web3Info.address, result.web3Info.signature);
            },
            (fail) =>
            {
                Debug.Log("login server failed");
                Debug.Log(fail);
                gameScreenLogin.HideLoadingScreen();
            }
        );
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
            Debug.Log("google failed");
            gameScreenLogin.HideLoadingScreen();
        }
#endif
    }

    private void StartLogin(string address, string signature)
    {
        SoundManager.Instance.PlayMetamaskSfx();
        AnalyticsManager.Instance.InitAnalyticsWithWallet(address);
        GetOrCreatePlayer(address, signature);
        UserManager.PlayerType = PlayerType.LoggedInUser;
    }

    private void SetDataToUser(GetPlayerDataResult res)
    {
        UserManager.Instance.SetPlayerUserName(res.nickname, false);
        UserManager.Instance.SetPlayerRank(res.rank);
        UserManager.Instance.SetWalletAddress(tempAddress);
        UserManager.Instance.SetSignature(tempSignature);
    }

    private void GetOrCreatePlayer(string address, string signature)
    {
        Debug.Log("ADDRESS " + address);
        Debug.Log("SIGNATURE " + signature);
        tempAddress = address;
        tempSignature = signature;
        ServerManager.Instance.GetPlayerDataFromServer(PlayerAPI.Get,
            (string data) =>
            {
                GetPlayerDataResult result = JsonUtility.FromJson<GetPlayerDataResult>(data);
                SetDataToUser(result);
                StoreManager.Instance.InitialiseStore(address);
                GoToMainMenu();
            }
            , address,
            (string data) =>
            {
                CreatePlayerData formData = new()
                {
                    address = tempAddress,
                    signature = tempSignature
                };
                string jsonFormData = JsonUtility.ToJson(formData);
                ServerManager.Instance.SendPlayerDataToServer(PlayerAPI.Create, jsonFormData, (string data) =>
                {
                    GetPlayerDataResult result = JsonUtility.FromJson<GetPlayerDataResult>(data);
                    SetDataToUser(result);
                    GoToMainMenu();
                });
            });
    }

    public override void Disable()
    {
        GameEventsManager.Instance.RemoveGlobalListener(OnGameEvent);
        Screens.Instance.PopScreen(gameScreenLogin);
        base.Disable();
    }
}