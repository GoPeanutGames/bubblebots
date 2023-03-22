public class GameStateLogin : GameState
{
    private GamePopupLogin _gamePopupLogin;
    private GameScreenLoading _gameScreenLoading;
    private GameScreenDarkenedBg _darkenedBg;

    public override string GetGameStateName()
    {
        return "game state login";
    }

    public override void Enter()
    {
        _darkenedBg = Screens.Instance.PushScreen<GameScreenDarkenedBg>(true);
        _gamePopupLogin = Screens.Instance.PushScreen<GamePopupLogin>();
    }

    public override void Enable()
    {
        GameEventsManager.Instance.AddGlobalListener(OnGameEvent);
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
                    UserManager.Instance.loginManager.SignIn(_gamePopupLogin.GetLoginInputFieldEmail(), _gamePopupLogin.GetLoginInputFieldPass(), EmailPassSignUpSuccess, SignInFail);
                }
                break;
            case ButtonId.LoginSignInClose:
                Screens.Instance.PopScreen(_darkenedBg);
                stateMachine.PopState();
                break;
            case ButtonId.LoginForgotPassword:
                //todo:
                break;
            case ButtonId.LoginGoToSignUp:
                //todo:
                break;
        }
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

    }
    
    private void EmailPassSignUpSuccess()
    {
        Screens.Instance.PopScreen(_gameScreenLoading);
        //TODO: pop this state
        //TODO: push 2fa state
        // _gameScreenLogin.Show2FAuth();
    }
    
    
    public override void Disable()
    {
        GameEventsManager.Instance.RemoveGlobalListener(OnGameEvent);
    }
    
    public override void Exit()
    {
        Screens.Instance.PopScreen(_gamePopupLogin);
    }
}