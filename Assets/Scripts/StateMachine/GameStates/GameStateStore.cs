using BubbleBots.Store;

public class GameStateStore : GameState
{
    private GameScreenStore _gameScreenStore;
    private GameScreenMainMenuTopHUD _gameScreenMainMenuTopHUD;
    private GameScreenMainMenuBottomHUD _gameScreenMainMenuBottomHUD;
    private StoreTabs _activeTab = StoreTabs.Gems;
    
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
        ActivateTab(_activeTab);
    }

    private void ActivateTab(StoreTabs tab)
    {
        _activeTab = tab;
        _gameScreenStore.ActivateTab(_activeTab);
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
            case ButtonId.StoreClose:
            case ButtonId.MainMenuBottomHUDHome:
                HideStore();
                break;
            case ButtonId.StoreTabGems:
                ActivateTab(StoreTabs.Gems);
                break;
            case ButtonId.StoreTabSkins:
                ActivateTab(StoreTabs.Skins);
                break;
            case ButtonId.StoreTabOffers:
                ActivateTab(StoreTabs.Offers);
                break;
            case ButtonId.StoreTabNuts:
                ActivateTab(StoreTabs.Nuts);
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