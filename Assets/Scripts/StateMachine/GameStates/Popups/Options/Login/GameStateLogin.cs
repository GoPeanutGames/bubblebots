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
    }

    public override void Enable()
    {
        _darkenedBg = Screens.Instance.PushScreen<GameScreenDarkenedBg>();
        _gamePopupLogin = Screens.Instance.PushScreen<GamePopupLogin>();
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
                UserManager.Instance.loginManager.GoogleSignIn(LoginSuccess, GoogleLoginFail);
                break;
            case ButtonId.LoginSignInApple:
                _gameScreenLoading = Screens.Instance.PushScreen<GameScreenLoading>();
                UserManager.Instance.loginManager.AppleSignIn(LoginSuccess, AppleLoginFail);
                break;
            case ButtonId.LoginSignInMetamask:
                _gameScreenLoading = Screens.Instance.PushScreen<GameScreenLoading>();
                UserManager.Instance.loginManager.MetamaskSignIn(LoginSuccess, MetamaskSignInFail);
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

    private void MetamaskSignInFail()
    {
        Screens.Instance.PopScreen(_gameScreenLoading);
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

    private void LoginSuccess()
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
        Screens.Instance.PopScreen(_gamePopupLogin);
        Screens.Instance.PopScreen(_darkenedBg);
    }
}
