using BubbleBots.Server.Player;
using UnityEngine;

public class GameStateMainMenu : GameState
{
    private GameScreenMainMenuBottomHUD gameScreenMainMenuBottomHUD;
    private GameScreenMainMenuTopHUD gameScreenMainMenuTopHUD;
    private GameScreenMainMenu gameScreenMainMenu;
    private GameScreenModeSelect gameScreenModeSelect;
    private GameScreenLoading _gameScreenLoading;
    private GameScreenChangeNickname _gameScreenChangeNickname;

    public override string GetGameStateName()
    {
        return "game state main menu";
    }

    public override void Enable()
    {
        GameEventsManager.Instance.AddGlobalListener(OnGameEvent);
        gameScreenMainMenu = Screens.Instance.PushScreen<GameScreenMainMenu>(true);
        gameScreenMainMenuTopHUD = Screens.Instance.PushScreen<GameScreenMainMenuTopHUD>(true);
        gameScreenMainMenuBottomHUD = Screens.Instance.PushScreen<GameScreenMainMenuBottomHUD>(true);
        Screens.Instance.ResetBackground();
#if !UNITY_EDITOR
        if (gameScreenMainMenuTopHUD.AreResourcesSet() == false)
        {
            _gameScreenLoading = Screens.Instance.PushScreen<GameScreenLoading>();
        }
        Screens.Instance.BringToFront<GameScreenLoading>();
        UserManager.Instance.GetPlayerResources(SetUserData);
#endif
    }

    private void SetUserData(GetPlayerWallet wallet)
    {
        if (gameScreenMainMenuTopHUD.AreResourcesSet() == false)
        {
            Screens.Instance.PopScreen(_gameScreenLoading);
        }
        gameScreenMainMenuTopHUD.SetTopInfo(GameScreenMainMenuTopHUD.PlayerResource.Bubbles, wallet.bubbles);
        gameScreenMainMenuTopHUD.SetTopInfo(GameScreenMainMenuTopHUD.PlayerResource.Gems, wallet.gems);
        gameScreenMainMenuTopHUD.SetTopInfo(GameScreenMainMenuTopHUD.PlayerResource.Energy, wallet.energy);
        gameScreenMainMenuTopHUD.SetUsername(UserManager.Instance.GetPlayerUserName());
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
            case ButtonId.ModeSelectNethermode:
                PlayNetherMode();
                break;
            case ButtonId.MainMenuTopHUDChangeNickname:
                OpenChangeNickname();
                break;
            case ButtonId.ChangeNicknameClose:
                CloseChangeNickname();
                break;
            case ButtonId.ChangeNicknameOk:
                ChangeNickname();
                CloseChangeNickname();
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
        Screens.Instance.PopScreen(gameScreenMainMenu);
        Screens.Instance.PopScreen(gameScreenMainMenuBottomHUD);
        //Screens.Instance.PopScreen(gameScreenMainMenuTopHUD);
        Screens.Instance.PopScreen(gameScreenModeSelect);
        stateMachine.PushState(new GameStateFreeMode());
    }

    private void PlayNetherMode()
    {
        Screens.Instance.PopScreen(gameScreenMainMenu);
        Screens.Instance.PopScreen(gameScreenMainMenuBottomHUD);
        //Screens.Instance.PopScreen(gameScreenMainMenuTopHUD);
        Screens.Instance.PopScreen(gameScreenModeSelect);
        stateMachine.PushState(new GameStateNetherMode());
    }

    private void ShowStore()
    {
        stateMachine.PushState(new GameStateStore());
    }

    private void OpenChangeNickname()
    {
        _gameScreenChangeNickname = Screens.Instance.PushScreen<GameScreenChangeNickname>();
        _gameScreenChangeNickname.SetNicknameText(UserManager.Instance.GetPlayerUserName());
    }

    private void CloseChangeNickname()
    {
        Screens.Instance.PopScreen(_gameScreenChangeNickname);
    }

    private void ChangeNickname()
    {
        UserManager.Instance.SetPlayerUserName(_gameScreenChangeNickname.GetNicknameText(), true);
        gameScreenMainMenuTopHUD.SetUsername(_gameScreenChangeNickname.GetNicknameText());
        CloseChangeNickname();
    }
    
    public override void Disable()
    {
        Screens.Instance.PopScreen(gameScreenMainMenu);
        GameEventsManager.Instance.RemoveGlobalListener(OnGameEvent);
    }
}