using System.Runtime.InteropServices;
using BubbleBots.Server.Signature;

public class GameStateLogin : GameState
{
    private GamePopupLogin _gamePopupLogin;
    private GameScreenLoading _gameScreenLoading;
    private GameScreenDarkenedBg _darkenedBg;

#if UNITY_WEBGL
    [DllImport("__Internal")]
    private static extern void Login(bool isDev);

    [DllImport("__Internal")]
    private static extern void RequestSignature(string schema, string address);
#endif
    
    public override string GetGameStateName()
    {
        return "game state login";
    }

    public override void Enter()
    {
    }

    public override void Enable()
    {
        _darkenedBg = Screens.Instance.PushScreen<GameScreenDarkenedBg>();
        _gamePopupLogin = Screens.Instance.PushScreen<GamePopupLogin>();
        GameEventsManager.Instance.AddGlobalListener(OnGameEvent);
        GameEventsManager.Instance.AddGlobalListener(OnMetamaskEvent);
    }
    
    private void OnGameEvent(GameEventData gameEvent)
    {
        if (gameEvent.eventName == GameEvents.ButtonTap)
        {
            OnButtonTap(gameEvent);
        }
    }
    
    private void OnMetamaskEvent(GameEventData data)
    {
        GameEventString metamaskEvent = data as GameEventString;
        if (data.eventName == GameEvents.MetamaskSuccess)
        {
            MetamaskLoginSuccess(metamaskEvent.stringData);
        }
        else if (data.eventName == GameEvents.SignatureSuccess)
        {
            SignatureLoginSuccess(metamaskEvent.stringData);
        }
    }

    private void OnButtonTap(GameEventData data)
    {
        GameEventString buttonTapData = data as GameEventString;
        switch (buttonTapData.stringData)
        {
            case ButtonId.LoginSignInGoogle:
                _gameScreenLoading = Screens.Instance.PushScreen<GameScreenLoading>();
                UserManager.Instance.loginManager.GoogleSignIn(GoogleOrAppleLoginSuccess, GoogleLoginFail);
                break;
            case ButtonId.LoginSignInApple:
                _gameScreenLoading = Screens.Instance.PushScreen<GameScreenLoading>();
                UserManager.Instance.loginManager.AppleSignIn(GoogleOrAppleLoginSuccess, AppleLoginFail);
                break;
            case ButtonId.LoginSignInSubmit:
                if (_gamePopupLogin.SignInValidation())
                {
                    _gameScreenLoading = Screens.Instance.PushScreen<GameScreenLoading>();
                    UserManager.Instance.loginManager.SignIn(_gamePopupLogin.GetLoginInputFieldEmail(), _gamePopupLogin.GetLoginInputFieldPass(), false, EmailPassSignUpSuccess, SignInFail);
                }
                break;
            case ButtonId.LoginSignInClose:
                stateMachine.PopState();
                break;
            case ButtonId.LoginForgotPassword:
                stateMachine.PushState(new GameStateForgotPassword());
                break;
            case ButtonId.LoginGoToSignUp:
                stateMachine.PopState();
                stateMachine.PushState(new GameStateRegister());
                break;
        }
    }

    private void LoginWithMetamask()
    {
        bool isDev = EnvironmentManager.Instance.IsDevelopment();
#if UNITY_WEBGL
        Login(isDev);
#endif
    }
    
    public void MetamaskLoginSuccess(string address)
    {
        // tempAddress = address;
        ServerManager.Instance.GetLoginSignatureDataFromServer(SignatureLoginAPI.Get, (schema) => { RequestSignatureFromMetamask(schema.ToString()); }, address);
        // gameScreenLogin.ShowLoadingScreen();
        // gameScreenLogin.HideLoginScreen();
    }

    private void RequestSignatureFromMetamask(string schema)
    {
#if UNITY_WEBGL
        // RequestSignature(schema, tempAddress);
#endif
    }

    public void SignatureLoginSuccess(string signature)
    {
        // SoundManager.Instance.PlayMetamaskSfx();
        // StartLogin(tempAddress, signature);
    }
    
    private void SignInFail()
    {
        Screens.Instance.PopScreen(_gameScreenLoading);
        _gamePopupLogin.SetSignInWrongError();
    }
    
    private void GoogleLoginFail()
    {
        _gamePopupLogin.SetSignInPlatformError("Google");
        Screens.Instance.PopScreen(_gameScreenLoading);
    }

    private void AppleLoginFail()
    {
        _gamePopupLogin.SetSignInPlatformError("Apple");
        Screens.Instance.PopScreen(_gameScreenLoading);

    }

    private void GoogleOrAppleLoginSuccess()
    {
        Screens.Instance.PopScreen(_gameScreenLoading);
        stateMachine.PopState();
    }
    
    private void EmailPassSignUpSuccess()
    {
        Screens.Instance.PopScreen(_gameScreenLoading);
        stateMachine.PopState();
        stateMachine.PushState(new GameStateTwoFA());
    }

    public override void Disable()
    {
        GameEventsManager.Instance.RemoveGlobalListener(OnGameEvent);
        GameEventsManager.Instance.RemoveGlobalListener(OnMetamaskEvent);
        Screens.Instance.PopScreen(_gamePopupLogin);
        Screens.Instance.PopScreen(_darkenedBg);
    }
}
