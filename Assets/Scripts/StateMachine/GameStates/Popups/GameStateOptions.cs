using BubbleBots.Server.Signature;

public class GameStateOptions : GameState
{
    private GamePopupOptions _gamePopupOptions;

    private int _finalAvatar;
    
    public override string GetGameStateName()
    {
        return "Game state options";
    }

    public override void Enter()
    {
        _gamePopupOptions = Screens.Instance.PushScreen<GamePopupOptions>();
        _gamePopupOptions.StartOpen();
        Screens.Instance.BringToFront<GamePopupOptions>();
    }

    public override void Enable()
    {
        _gamePopupOptions.RefreshPlayerUsername();
        GameEventsManager.Instance.AddGlobalListener(OnGameEvent);
    }

    private void OnGameEvent(GameEventData data)
    {
        if (data.eventName == GameEvents.ButtonTap)
        {
            OnButtonTap(data);
        }
    }

    private void OnButtonTap(GameEventData data)
    {
        GameEventString customButtonData = data as GameEventString;
        switch (customButtonData.stringData)
        {
            case ButtonId.OptionsClose:
                stateMachine.PopState();
                break;
            case ButtonId.OptionsSignOut:
                Logout();
                break;
            case ButtonId.OptionsChangePicture:
                ChangePicture();
                break;
            case ButtonId.OptionsChangeName:
                stateMachine.PushState(new GameStateChangeNickname());
                break;
            case ButtonId.OptionsSave:
                SaveSettings();
                stateMachine.PopState();
                break;
        }
    }

    private void ChangePicture()
    {
        int avatar = UserManager.Instance.GetPlayerAvatar() + 1;
        if (avatar == 3) avatar = 0;
        _finalAvatar = avatar;
        _gamePopupOptions.SetPlayerAvatar(avatar);
    }

    private void SaveSettings()
    {
        UserManager.Instance.ChangePlayerAvatar(_finalAvatar);
        bool musicOn = _gamePopupOptions.GetFinalMusicValue();
        bool hintsOn = _gamePopupOptions.GetFinalHintsValue();
        if (musicOn)
        {
            SoundManager.Instance.UnMute();
        }
        else
        {
            SoundManager.Instance.Mute();
        }
        UserManager.Instance.SetPlayerHints(hintsOn);
    }
    
    private void Logout()
    {
        UserManager.ClearPrefs();
        Screens.Instance.PushScreen<GameScreenLogin>();
        ServerManager.Instance.GetLoginSignatureDataFromServer(SignatureLoginAPI.Logout, LogoutSuccess);
    }

    private void LogoutSuccess(string result)
    {
        for (int i = 0; i < 50; i++)
        {
            Screens.Instance.PopScreen();
        }
        
        stateMachine.ForceClean();
        stateMachine.PushState(new GameStateLogin());
    }

    public override void Disable()
    {
        GameEventsManager.Instance.RemoveGlobalListener(OnGameEvent);
    }

    public override void Exit()
    {
        _gamePopupOptions.StartClose();
    }
}