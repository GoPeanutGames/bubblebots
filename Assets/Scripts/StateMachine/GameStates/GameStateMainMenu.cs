using System.Runtime.InteropServices;
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
    private GameScreenNotEnoughGems gameScreenNotEnoughGems;
    private GameScreenPremint _gameScreenPremint;
    private GameScreenComingSoonGeneric _gameScreenComingSoonGeneric;
    private GameScreenComingSoonNether _gameScreenComingSoonNether;
    private GameScreenComingSoonItems _gameScreenComingSoonItems;

    private bool canPlayNetherMode = false;
    private bool canPlayFreeMode = false;

    [DllImport("__Internal")]
    private static extern void OpenChangeNickNameMobile(string nickname);

    public override string GetGameStateName()
    {
        return "game state main menu";
    }

    private void ResetMainMenuLook()
    {
        gameScreenMainMenuBottomHUD.DeactivateStoreButtonGlow();
        gameScreenMainMenuBottomHUD.HideHomeButton();
        gameScreenMainMenuTopHUD.ShowSettingsGroup();
        gameScreenMainMenuTopHUD.ShowPlayerInfoGroup();
    }

    public override void Enable()
    {
        canPlayNetherMode = false;
        canPlayFreeMode = false;
        GameEventsManager.Instance.AddGlobalListener(OnGameEvent);
        gameScreenMainMenu = Screens.Instance.PushScreen<GameScreenMainMenu>(true);
        gameScreenMainMenuTopHUD = Screens.Instance.PushScreen<GameScreenMainMenuTopHUD>(true);
        gameScreenMainMenuBottomHUD = Screens.Instance.PushScreen<GameScreenMainMenuBottomHUD>(true);
        Screens.Instance.ResetBackground();
        ResetMainMenuLook();
#if !UNITY_EDITOR
        if (gameScreenMainMenuTopHUD.AreResourcesSet() == false)
        {
            _gameScreenLoading = Screens.Instance.PushScreen<GameScreenLoading>();
        }
        Screens.Instance.BringToFront<GameScreenLoading>();
        GameScreenMainMenuTopHUD.ResourcesSet += ResourcesSet;
        gameScreenMainMenuTopHUD.SetUsername(UserManager.Instance.GetPlayerUserName());
#endif

        UserManager.CallbackWithResources += ResourcesReceived;
        UserManager.Instance.GetPlayerResources();
    }

    private void ResourcesSet()
    {
        Screens.Instance.PopScreen(_gameScreenLoading);
    }

    private void OnGameEvent(GameEventData data)
    {
        if (data.eventName == GameEvents.ButtonTap)
        {
            OnButtonTap(data);
        }

        if( data.eventName == GameEvents.NickNameChangedForMobile )
        {
            GameEventString nickNameData = data as GameEventString;

            ChangeNickNameWithJs(nickNameData.stringData);
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
            case ButtonId.MainMenuBottomHUDFriends:
                ShowComingSoonGeneric();
                break;
            case ButtonId.MainMenuBottomHUDNetherpass:
                ShowComingSoonNether();
                break;
            case ButtonId.ComingSoonNetherpassClose:
                HideComingSoonNether();
                break;
            case ButtonId.MainMenuBottomHUDItems:
                ShowComingSoonItems();
                break;
            case ButtonId.ComingSoonItemsClose:
                HideComingSoonItems();
                break;
            case ButtonId.ModeSelectBackButton:
                HideModeSelect();
                break;
            case ButtonId.MainMenuTopHUDGemPlus:
            case ButtonId.MainMenuBottomHUDStore:
                ShowStore();
                break;
            case ButtonId.MainMenuTopHUDPremint:
                OpenPremintPopup();
                break;
            case ButtonId.MainMenuTopHUDLeaderboard:
                ShowLeaderboard();
                break;
            case ButtonId.ComingSoonGenericClose:
                HideComingSoonGeneric();
                break;
            case ButtonId.PremintOk:
                PremintClick();
                break;
            case ButtonId.PremintClose:
                ClosePremintPopup();
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
                TryPlayNetherMode();
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
            case ButtonId.NotEnoughGemsBack:
                CloseNotEnoughGems();
                break;
            default:
                break;
        }
    }

    private void ShowLeaderboard()
    {
        stateMachine.PushState(new GameStateLeaderboard());
    }
    
    private void ShowComingSoonGeneric()
    {
        _gameScreenComingSoonGeneric = Screens.Instance.PushScreen<GameScreenComingSoonGeneric>();
    }

    private void HideComingSoonGeneric()
    {
        Screens.Instance.PopScreen(_gameScreenComingSoonGeneric);
    }

    private void ShowComingSoonNether()
    {
        _gameScreenComingSoonNether = Screens.Instance.PushScreen<GameScreenComingSoonNether>();
    }

    private void HideComingSoonNether()
    {
        Screens.Instance.PopScreen(_gameScreenComingSoonNether);
    }

    private void ShowComingSoonItems()
    {
        _gameScreenComingSoonItems = Screens.Instance.PushScreen<GameScreenComingSoonItems>();
    }

    private void HideComingSoonItems()
    {
        Screens.Instance.PopScreen(_gameScreenComingSoonItems);
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
        Screens.Instance.PopScreen(gameScreenModeSelect);
        stateMachine.PushState(new GameStateFreeMode());
    }

    private void TryPlayFreeMode()
    {

    }

    private void TryPlayNetherMode()
    {
        if (canPlayNetherMode)
        {
            PlayNetherMode();
        } 
        else
        {
            gameScreenNotEnoughGems = Screens.Instance.PushScreen<GameScreenNotEnoughGems>();
        }
        UserManager.Instance.GetPlayerResources();
    }

    private void CloseNotEnoughGems()
    {
        Screens.Instance.PopScreen(gameScreenNotEnoughGems);
        Screens.Instance.BringToFront<GameScreenMainMenuTopHUD>();
    }

    private void PlayNetherMode()
    {
        Screens.Instance.PopScreen(gameScreenMainMenu);
        Screens.Instance.PopScreen(gameScreenMainMenuBottomHUD);
        Screens.Instance.PopScreen(gameScreenModeSelect);
        stateMachine.PushState(new GameStateNetherMode());
    }

    private void ShowStore()
    {
        stateMachine.PushState(new GameStateStore());
    }

    private void OpenChangeNickname()
    {
        if (Application.isMobilePlatform == true)
        {
            OpenChangeNickNameMobile(UserManager.Instance.GetPlayerUserName());
        }
        else
        {
            _gameScreenChangeNickname = Screens.Instance.PushScreen<GameScreenChangeNickname>();
            _gameScreenChangeNickname.SetNicknameText(UserManager.Instance.GetPlayerUserName());
        }        
    }

    private void CloseChangeNickname()
    {
        Screens.Instance.PopScreen(_gameScreenChangeNickname);
    }

    private void ChangeNickNameWithJs(string nickname)
    {
        UserManager.Instance.SetPlayerUserName(nickname, true);
        gameScreenMainMenuTopHUD.SetUsername(nickname);
    
    }

    private void ChangeNickname()
    {

        UserManager.Instance.SetPlayerUserName(_gameScreenChangeNickname.GetNicknameText(), true);
        gameScreenMainMenuTopHUD.SetUsername(_gameScreenChangeNickname.GetNicknameText());
        CloseChangeNickname();
    }

    private void OpenPremintPopup()
    {
        _gameScreenPremint = Screens.Instance.PushScreen<GameScreenPremint>();
    }

    private void PremintClick()
    {
        Application.OpenURL("https://www.premint.xyz/peanutgames-bubble-bots-mini-game/");
    }
    
    private void ClosePremintPopup()
    {
        Screens.Instance.PopScreen(_gameScreenPremint);
    }
    
    public override void Disable()
    {
        Screens.Instance.PopScreen(gameScreenMainMenu);
        GameEventsManager.Instance.RemoveGlobalListener(OnGameEvent);
        UserManager.CallbackWithResources -= ResourcesReceived;
    }

    private void ResourcesReceived(GetPlayerWallet wallet)
    {
        canPlayNetherMode = wallet.gems > 0;
        canPlayFreeMode = wallet.energy > 0;
    }
}