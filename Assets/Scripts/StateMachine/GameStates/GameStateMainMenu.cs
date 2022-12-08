using UnityEngine;

public class GameStateMainMenu : GameState
{
    private GameScreenMainMenuBottomHUD gameScreenMainMenuBottomHUD;
    private GameScreenMainMenuTopHUD gameScreenMainMenuTopHUD;
    private GameScreenMainMenu gameScreenMainMenu;
    private GameScreenModeSelect gameScreenModeSelect;
    private GameScreenStore gameScreenStore;

    public override string GetGameStateName()
    {
        return "game state main menu";
    }

    public override void Enable()
    {
        GameEventsManager.Instance.AddGlobalListener(OnGameEvent);
        gameScreenMainMenu = Screens.Instance.PushScreen<GameScreenMainMenu>();
        gameScreenMainMenuBottomHUD = Screens.Instance.PushScreen<GameScreenMainMenuBottomHUD>();
        gameScreenMainMenuTopHUD = Screens.Instance.PushScreen<GameScreenMainMenuTopHUD>();
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
            case ButtonId.MainMenuBottomHUDPlay:
                ShowModeSelect();
                break;
            case ButtonId.ModeSelectBackButton:
                HideModeSelect();
                break;
            case ButtonId.MainMenuBottomHUDStore:
                ShowStore();
                break;
            case ButtonId.MainMenuBottomHUDHome:
                HideStore();
                break;
            case ButtonId.ModeSelectFreeModeTooltip:
                ShowFreeModeTooltip();
                break;
            case ButtonId.ModeSelectNetherModeTooltip:
                ShowNetherModeTooltip();
                break;
            case ButtonId.ModeSelectFreeModeTooltipBack:
            case ButtonId.ModeSelectNetherModeTooltipBack:
                HideFreeModeTooltip();
                break;
            default:
                break;
        }
    }

    private void ShowFreeModeTooltip()
    {
        gameScreenModeSelect.ShowToolTipFreeMode();
    }

    private void ShowNetherModeTooltip()
    {
        gameScreenModeSelect.ShowToolTipNetherMode();
    }
    private void HideFreeModeTooltip()
    {
        gameScreenModeSelect.HideToolTips();
    }


    private void HideModeSelect()
    {
        Screens.Instance.PopScreen(gameScreenModeSelect);
        gameScreenMainMenuTopHUD.ShowPlayerInfoGroup();
        gameScreenMainMenuTopHUD.ShowSettingsGroup();
    }

    private void ShowModeSelect()
    {
        gameScreenModeSelect = Screens.Instance.PushScreen<GameScreenModeSelect>();
        Screens.Instance.BringToFront<GameScreenMainMenuTopHUD>();
        gameScreenMainMenuTopHUD.HidePlayerInfoGroup();
        gameScreenMainMenuTopHUD.HideSettingsGroup();
    }

    private void HideStore()
    {
        Screens.Instance.PopScreen(gameScreenStore);
        gameScreenMainMenuTopHUD.ShowPlayerInfoGroup();
        gameScreenMainMenuTopHUD.ShowSettingsGroup();
        gameScreenMainMenuBottomHUD.HideHomeButton();
    }

    private void ShowStore()
    {
        gameScreenStore = Screens.Instance.PushScreen<GameScreenStore>();
        Screens.Instance.BringToFront<GameScreenMainMenuBottomHUD>();
        Screens.Instance.BringToFront<GameScreenMainMenuTopHUD>();
        gameScreenMainMenuTopHUD.HidePlayerInfoGroup();
        gameScreenMainMenuTopHUD.HideSettingsGroup();
        gameScreenMainMenuBottomHUD.ShowHomeButton();
    }

    public override void Disable()
    {
        GameEventsManager.Instance.RemoveGlobalListener(OnGameEvent);
    }
}
