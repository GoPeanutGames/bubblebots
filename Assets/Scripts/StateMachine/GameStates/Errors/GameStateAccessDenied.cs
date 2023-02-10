public class GameStateAccessDenied : GameState
{
    private GameScreenAccessDenied _gameScreenAccessDenied;

    public override string GetGameStateName()
    {
        return "Game state access denied";
    }

    public override void Enter()
    {
        base.Enter();
        _gameScreenAccessDenied = Screens.Instance.PushScreen<GameScreenAccessDenied>();
        _gameScreenAccessDenied.StartOpen();
        Screens.Instance.BringToFront<GameScreenAccessDenied>();
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
        _gameScreenAccessDenied.StartClose(ExitDone);
        base.Disable();
    }

    private void ExitDone()
    {
        Screens.Instance.PopScreen(_gameScreenAccessDenied);
    }
}