using BubbleBots.Server.Player;
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
        SetPlayerResourceInfo();
    }

    private void SetPlayerResourceInfo()
    {
        ServerManager.Instance.GetPlayerDataFromServer(PlayerAPI.Wallet, (jsonData) =>
        {
            GetPlayerWallet playerWallet = JsonUtility.FromJson<GetPlayerWallet>(jsonData);
            gameScreenMainMenuTopHUD.SetTopInfo(GameScreenMainMenuTopHUD.PlayerResource.Bubbles, playerWallet.bubbles);
            gameScreenMainMenuTopHUD.SetTopInfo(GameScreenMainMenuTopHUD.PlayerResource.Gems, playerWallet.gems);
            gameScreenMainMenuTopHUD.SetTopInfo(GameScreenMainMenuTopHUD.PlayerResource.Energy, playerWallet.energy);
        }, UserManager.Instance.GetPlayerWalletAddress());
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
            case ButtonId.ModeSelectFreeMode:
                PlayFreeMode();
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

    private void PlayFreeMode()
    {
        stateMachine.PushState(new GameStateFreeMode());
    }

    private void ShowStore()
    {
        stateMachine.PushState(new GameStateStore());
        Screens.Instance.PopScreen(gameScreenMainMenu);
        Screens.Instance.PopScreen(gameScreenMainMenuBottomHUD);
        Screens.Instance.PopScreen(gameScreenMainMenuTopHUD);
    }

    public override void Disable()
    {
        Screens.Instance.PopScreen(gameScreenMainMenu);
        Screens.Instance.PopScreen(gameScreenMainMenuTopHUD);
        Screens.Instance.PopScreen(gameScreenMainMenuBottomHUD);
        Screens.Instance.PopScreen(gameScreenModeSelect);
        GameEventsManager.Instance.RemoveGlobalListener(OnGameEvent);
    }
}
