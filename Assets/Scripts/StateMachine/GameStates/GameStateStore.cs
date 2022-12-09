using System.Collections.Generic;
using BubbleBots.Store;

public class GameStateStore : GameState
{
    private GameScreenStore _gameScreenStore;
    private GameScreenMainMenuTopHUD _gameScreenMainMenuTopHUD;
    private GameScreenMainMenuBottomHUD _gameScreenMainMenuBottomHUD;
    private StoreTabs _activeTab = StoreTabs.Gems;
    private int currentlyShowingOfferIndex = 0;

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

    private void SetupTabContent()
    {
        StoreTab tabDefinition = StoreManager.Instance.GetStoreTabContent(_activeTab);
        _gameScreenStore.GenerateLayout(tabDefinition.Layout);
        _gameScreenStore.GenerateStoreItems(tabDefinition.Items);
    }

    private void ClickRight()
    {
        _gameScreenStore.MoveOutLeftSpecialOffer();
        currentlyShowingOfferIndex++;
        if (currentlyShowingOfferIndex >= StoreManager.Instance.GetSpecialOffers().Count)
        {
            currentlyShowingOfferIndex = 0;
        }

        GenerateSpecialOffer();
        _gameScreenStore.MoveInRightSpecialOffer();
    }

    private void ClickLeft()
    {
        _gameScreenStore.MoveOutRightSpecialOffer();
        currentlyShowingOfferIndex--;
        if (currentlyShowingOfferIndex < 0)
        {
            currentlyShowingOfferIndex = StoreManager.Instance.GetSpecialOffers().Count - 1;
        }

        GenerateSpecialOffer();
        _gameScreenStore.MoveInLeftSpecialOffer();
    }

    public void GenerateSpecialOffer()
    {
        List<SpecialOffer> specialOffers = StoreManager.Instance.GetSpecialOffers();
        SpecialOffer offer = specialOffers[currentlyShowingOfferIndex];
        _gameScreenStore.GenerateSpecialOffer(offer);
    }

    private void ActivateTab(StoreTabs tab)
    {
        _activeTab = tab;
        _gameScreenStore.ActivateTab(_activeTab);
        SetupTabContent();
        GenerateSpecialOffer();
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
            case ButtonId.StoreSpecialOfferLeft:
                ClickLeft();
                break;
            case ButtonId.StoreSpecialOfferRight:
                ClickRight();
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