public class GameStateOptions : GameState
{
    private GamePopupOptions _gamePopupOptions;
    
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
        }
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