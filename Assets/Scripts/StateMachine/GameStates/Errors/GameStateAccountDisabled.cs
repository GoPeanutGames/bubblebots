using UnityEngine;

public class GameStateAccountDisabled : GameState
{
    private GameScreenAccountDisabled _gameScreenAccountDisabled;
    
    public override string GetGameStateName()
    {
        return "Game state account disabled";
    }

    public override void Enter()
    {
        base.Enter();
        _gameScreenAccountDisabled = Screens.Instance.PushScreen<GameScreenAccountDisabled>();
        Screens.Instance.BringToFront<GameScreenAccountDisabled>();
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
            case ButtonId.AccountDisabledClose:
                stateMachine.PopState();
                break;
            case ButtonId.AccountDisabledContactSupport:
                Application.OpenURL("mailto:info@peanutgames.com");
                break;
        }
    }

    public override void Disable()
    {
        GameEventsManager.Instance.RemoveGlobalListener(OnGameEvent);
        base.Disable();
    }

    public override void Exit()
    {
        Screens.Instance.PopScreen(_gameScreenAccountDisabled);
        base.Exit();
    }
}