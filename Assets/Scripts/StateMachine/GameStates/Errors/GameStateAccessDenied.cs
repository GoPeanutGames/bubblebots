public class GameStateAccessDenied : GameState
{
    private GamePopupAccessDenied _gamePopupAccessDenied;

    public override string GetGameStateName()
    {
        return "Game state access denied";
    }

    public override void Enter()
    {
        base.Enter();
        _gamePopupAccessDenied = Screens.Instance.PushScreen<GamePopupAccessDenied>();
        _gamePopupAccessDenied.StartOpen();
        Screens.Instance.BringToFront<GamePopupAccessDenied>();
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
            case ButtonId.AccessDeniedGoBack:
                stateMachine.PopState();
                stateMachine.PushState(new GameStateLogin());
                break;
        }
    }

    public override void Disable()
    {
        GameEventsManager.Instance.RemoveGlobalListener(OnGameEvent);
        _gamePopupAccessDenied.StartClose();
        base.Disable();
    }
}