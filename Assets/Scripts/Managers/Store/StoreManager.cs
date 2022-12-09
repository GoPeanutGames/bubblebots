using BubbleBots.Store;
using System.Collections.Generic;

public class StoreManager : MonoSingleton<StoreManager>
{
    private Dictionary<StoreTabs, StoreTab> storeTabItemsMap = new()
    {
        //TODO: placeholder, this will be taken from backend
        {StoreTabs.Gems, new StoreTab()
            {
                Layout = StoreTabContentLayout.Grid,
                Items = new List<StoreItem>()
                {
                    new StoreItem(){TopLine = "Item 1",Image = "Store/Gems/Gem Chest Small",BottomLine = "Price 1"},
                    new StoreItem(){TopLine = "Item 2",Image = "Store/Gems/Gem Chest Small",BottomLine = "Price 2"},
                    new StoreItem(){TopLine = "Item 3",Image = "Store/Gems/Gem Chest Small",BottomLine = "Price 3"},
                    new StoreItem(){TopLine = "Item 4",Image = "Store/Gems/Gem Chest Small",BottomLine = "Price 4"},
                    new StoreItem(){TopLine = "Item 5",Image = "Store/Gems/Gem Chest Small",BottomLine = "Price 5"},
                    new StoreItem(){TopLine = "Item 6",Image = "Store/Gems/Gem Chest Small",BottomLine = "Price 6"},
                }
            }
        }
    };

    private List<SpecialOffer> specialOffers = new()
    {
        new SpecialOffer() {ButtonText = "Temporary", Image = "Store/Gems/Special Offer Save 40 on Gems"},
        new SpecialOffer() {ButtonText = "Temporary 2", Image = "Store/Gems/Special Offer Save 40 on Gems"},
        new SpecialOffer() {ButtonText = "Temporary 3", Image = "Store/Gems/Special Offer Save 40 on Gems"}
    };

    public StoreTab GetStoreTabContent(StoreTabs tab)
    {
        return storeTabItemsMap[tab];
    }

    public List<SpecialOffer> GetSpecialOffers()
    {
        return specialOffers;
    }
}
