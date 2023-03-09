using BubbleBots.Server.Signature;
using GooglePlayGames;

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
        _finalAvatar = UserManager.Instance.GetPlayerAvatar();
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
        _finalAvatar++;
        if (_finalAvatar == 3) _finalAvatar = 0;
        _gamePopupOptions.SetPlayerAvatar(_finalAvatar);
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
        ServerManager.Instance.GetLoginSignatureDataFromServer(SignatureLoginAPI.Logout, LogoutSuccess,"", LogoutSuccess);
        PlayGamesPlatform.Instance.SignOut();
    }

    private void LogoutSuccess(string result)
    {
        for (int i = 0; i < 50; i++)
        {
            Screens.Instance.PopScreen();
        }
        
        stateMachine.PopAll();
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