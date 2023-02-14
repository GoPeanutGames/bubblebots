using System.Collections.Generic;
using BubbleBots.Store;

public class GameStateStore : GameState
{
    private GameScreenStore _gameScreenStore;
    private GameScreenHomeHeader _gameScreenHomeHeader;
    private GameScreenHomeFooter _gameScreenHomeFooter;
    private StoreTabs _activeTab = StoreTabs.Gems;
    private int currentlyShowingOfferIndex = 0;

    public override string GetGameStateName()
    {
        return "game state store";
    }

    public override void Enable()
    {
        GameEventsManager.Instance.AddGlobalListener(OnGameEvent);
        _gameScreenStore = Screens.Instance.PushScreen<GameScreenStore>(true);
        _gameScreenHomeHeader = Screens.Instance.PushScreen<GameScreenHomeHeader>(true);
        _gameScreenHomeFooter = Screens.Instance.PushScreen<GameScreenHomeFooter>(true);
        Screens.Instance.BringToFront<GameScreenHomeHeader>();
        Screens.Instance.BringToFront<GameScreenHomeFooter>();
        _gameScreenHomeFooter.ShowHomeButton();
        _gameScreenHomeHeader.HidePlayerInfoGroup();
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
        // _gameScreenStore.MoveOutLeftSpecialOffer();
        // currentlyShowingOfferIndex++;
        // if (currentlyShowingOfferIndex >= StoreManager.Instance.GetSpecialOffers().Count)
        // {
        //     currentlyShowingOfferIndex = 0;
        // }
        //
        // GenerateSpecialOffer();
        // _gameScreenStore.MoveInRightSpecialOffer();
    }

    private void ClickLeft()
    {
        // _gameScreenStore.MoveOutRightSpecialOffer();
        // currentlyShowingOfferIndex--;
        // if (currentlyShowingOfferIndex < 0)
        // {
        //     currentlyShowingOfferIndex = StoreManager.Instance.GetSpecialOffers().Count - 1;
        // }
        //
        // GenerateSpecialOffer();
        // _gameScreenStore.MoveInLeftSpecialOffer();
    }

    public void GenerateSpecialOffer()
    {
        // List<SpecialOffer> specialOffers = StoreManager.Instance.GetSpecialOffers();
        // SpecialOffer offer = specialOffers[currentlyShowingOfferIndex];
        // _gameScreenStore.GenerateSpecialOffer(offer);
    }

    private void ActivateTab(StoreTabs tab)
    {
        _activeTab = tab;
        _gameScreenStore.ActivateTab(_activeTab);
        SetupTabContent();
        // GenerateSpecialOffer();
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
            case ButtonId.StoreBuy:
                BuyButtonClick(data);
                break;
            default:
                break;
        }
    }

    
    private void BuyButtonClick(GameEventData data)
    {
        GameEventStore gameEventStore = data as GameEventStore;
        stateMachine.PushState(new GameStateConfirmTransaction(gameEventStore.bundleId));
    }
    
    private void HideStore()
    {
        stateMachine.PushState(new GameStateHome());
        Screens.Instance.PopScreen(_gameScreenStore);
        _gameScreenHomeFooter.HideHomeButton();
        _gameScreenHomeHeader.ShowPlayerInfoGroup();
    }

    public override void Disable()
    {
        GameEventsManager.Instance.RemoveGlobalListener(OnGameEvent);
    }
}