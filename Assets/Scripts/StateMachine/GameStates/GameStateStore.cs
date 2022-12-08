using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateStore : GameState
{
    private GameScreenStore _gameScreenStore;
    private GameScreenMainMenuTopHUD _gameScreenMainMenuTopHUD;
    private GameScreenMainMenuBottomHUD _gameScreenMainMenuBottomHUD;

    public override string GetGameStateName()
    {
        return "game state store";
    }

    public override void Enable()
    {
        GameEventsManager.Instance.AddGlobalListener(OnGameEvent);
        _gameScreenStore = Screens.Instance.PushScreen<GameScreenStore>();
        _gameScreenMainMenuTopHUD = Screens.Instance.PushScreen<GameScreenMainMenuTopHUD>();
        _gameScreenMainMenuBottomHUD = Screens.Instance.PushScreen<GameScreenMainMenuBottomHUD>();
        Screens.Instance.BringToFront<GameScreenMainMenuTopHUD>();
        Screens.Instance.BringToFront<GameScreenMainMenuBottomHUD>();
        _gameScreenMainMenuBottomHUD.ActivateStoreButtonGlow();
        _gameScreenMainMenuBottomHUD.ShowHomeButton();
        _gameScreenMainMenuTopHUD.HideSettingsGroup();
        _gameScreenMainMenuTopHUD.HidePlayerInfoGroup();
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
            case ButtonId.MainMenuBottomHUDHome:
                HideStore();
                break;
            default:
                break;
        }
    }

    private void HideStore()
    {
        stateMachine.PopState();
        Screens.Instance.PopScreen(_gameScreenStore);
        Screens.Instance.PopScreen(_gameScreenMainMenuBottomHUD);
        Screens.Instance.PopScreen(_gameScreenMainMenuTopHUD);
    }

    public override void Disable()
    {
        _gameScreenMainMenuBottomHUD.DeactivateStoreButtonGlow();
        _gameScreenMainMenuBottomHUD.ShowHomeButton();
        _gameScreenMainMenuTopHUD.ShowSettingsGroup();
        _gameScreenMainMenuTopHUD.ShowPlayerInfoGroup();
        GameEventsManager.Instance.RemoveGlobalListener(OnGameEvent);
    }
}