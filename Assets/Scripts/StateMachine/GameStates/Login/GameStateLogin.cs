using System.Security.Cryptography;
using System.Text;
using BubbleBots.Server.Player;
using BubbleBots.Server.Signature;
using UnityEngine;

public class GameStateLogin : GameState
{
    private GameScreenLogin _gameScreenLogin;
    private GoogleLogin _googleLogin;
    private AutoLogin _autoLogin;
    private string _tempEmail;
    private string _tempHashedPass;

    public override string GetGameStateName()
    {
        return "game state login";
    }

    public override void Enable()
    {
        SoundManager.Instance.FadeInMusic();
        SoundManager.Instance.PlayStartMusicNew();
        _gameScreenLogin = Screens.Instance.PushScreen<GameScreenLogin>();
        GameEventsManager.Instance.AddGlobalListener(OnGameEvent);
        _googleLogin = new GoogleLogin();
        _autoLogin = new AutoLogin();
        _autoLogin.TryAutoLogin(AutoLoginSuccess, AutoLoginFail);
    }

    private void AutoLoginSuccess(User user)
    {
        LoginResult loginResult = new LoginResult()
        {
            user = user,
            token = UserManager.Instance.GetPlayerJwtToken(),
            web3Info = new PostWeb3Login()
            {
                address = UserManager.Instance.GetPlayerWalletAddress(),
                signature = UserManager.Instance.GetPlayerSignature()
            }
        };
        LoginSuccessSetData(loginResult);
    }

    private void AutoLoginFail(string error)
    {
        Debug.LogError("Auto Login failed with: " + error);
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
                _gameScreenLogin.OnPlayAsGuestPressed();
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
                _gameScreenLogin.ShowLoginScreen1stStep();
                break;
            case ButtonId.LoginEmailPassSignUp:
                _gameScreenLogin.ShowEmailPassSignupScreen();
                break;
            case ButtonId.LoginEmailPass:
                _gameScreenLogin.ShowEmailPassLoginSignupScreen();
                break;
            case ButtonId.LoginEmailPassSignUpLogin2ndStep:
                Login2ndStep();
                break;
            case ButtonId.LoginGuestPlay:
                PlayAsGuest();
                UserManager.PlayerType = PlayerType.Guest;
                AnalyticsManager.Instance.InitAnalyticsGuest();
                break;
            case ButtonId.LoginGoogle:
                _gameScreenLogin.ShowLoadingScreen();
                _googleLogin.StartLogin(LoginSuccessSetData, GoogleLoginFail);
                break;
        }
    }

    private void SignUpEmailPass()
    {
        string email = _gameScreenLogin.GetSignUpInputFieldEmail();
        string pass = _gameScreenLogin.GetSignUpInputFieldPass();
        var provider = new SHA256Managed();
        var hash = provider.ComputeHash(Encoding.UTF8.GetBytes(pass));
        string hashString = string.Empty;
        foreach (byte x in hash)
        {
            hashString += $"{x:x2}";
        }

        _tempEmail = email;
        _tempHashedPass = hashString;
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
        _gameScreenLogin.ShowSignUpLogin2ndStep();
    }

    private void EmailPassSignUpFail(string error)
    {
        Debug.Log("error: " + error);
    }

    private void LoginEmailPass()
    {
        string email = _gameScreenLogin.GetLoginInputFieldEmail();
        string pass = _gameScreenLogin.GetLoginInputFieldPass();
        var provider = new SHA256Managed();
        var hash = provider.ComputeHash(Encoding.UTF8.GetBytes(pass));
        string hashString = string.Empty;
        foreach (byte x in hash)
        {
            hashString += $"{x:x2}";
        }

        _tempEmail = email;
        _tempHashedPass = hashString;
        EmailPassSignUp data = new EmailPassSignUp()
        {
            email = email,
            password = hashString
        };
        string formData = JsonUtility.ToJson(data);
        ServerManager.Instance.SendLoginDataToServer(SignatureLoginAPI.Login1StStep, formData, EmailPassSignUpSuccess, EmailPassSignUpFail);
    }

    private void Login2ndStep()
    {
        string code = _gameScreenLogin.GetLoginInputFieldCode();
        Login2ndStep data = new Login2ndStep()
        {
            email = _tempEmail,
            password = _tempHashedPass,
            twoFaCode = code
        };
        string formData = JsonUtility.ToJson(data);
        ServerManager.Instance.SendLoginDataToServer(SignatureLoginAPI.Login2NdStep, formData, Login2ndStepSuccess, Login2ndStepFail);
    }

    private void Login2ndStepSuccess(string success)
    {
        Debug.Log("success: " + success);
        LoginResult result = JsonUtility.FromJson<LoginResult>(success);
        LoginSuccessSetData(result);
    }

    private void Login2ndStepFail(string error)
    {
        Debug.Log("error: " + error);
    }

    private void PlayAsGuest()
    {
        UserManager.PlayerType = PlayerType.Guest;
        AnalyticsManager.Instance.InitAnalyticsGuest();
        _gameScreenLogin.HideLoadingScreen();
        stateMachine.PushState(new GameStateFreeMode());
    }

    private void GoToMainMenu()
    {
        stateMachine.PushState(new GameStateMainMenu());
    }

    private void GoogleLoginFail(string reason)
    {
        Debug.LogError("Google login fail: " + reason);
        _gameScreenLogin.HideLoadingScreen();
    }

    private void LoginSuccessSetData(LoginResult result)
    {
        AnalyticsManager.Instance.InitAnalyticsWithWallet(result.web3Info.address);
        UserManager.PlayerType = PlayerType.LoggedInUser;
        UserManager.Instance.SetJwtToken(result.token);
        UserManager.Instance.SetWalletAddress(result.web3Info.address);
        UserManager.Instance.SetSignature(result.web3Info.signature);
        StoreManager.Instance.InitialiseStore(result.web3Info.address);
        ServerManager.Instance.GetPlayerDataFromServer(PlayerAPI.Get, GetPlayerSuccess, result.web3Info.address, GetPlayerFail);
    }

    private void GetPlayerSuccess(string result)
    {
        GetPlayerDataResult playerData = JsonUtility.FromJson<GetPlayerDataResult>(result);
        UserManager.Instance.SetPlayerUserName(playerData.nickname, false);
        GoToMainMenu();
    }

    private void GetPlayerFail(string result)
    {
        Debug.Log("Get player fail: " + result);
    }

    public override void Disable()
    {
        GameEventsManager.Instance.RemoveGlobalListener(OnGameEvent);
        Screens.Instance.PopScreen(_gameScreenLogin);
        base.Disable();
    }
}