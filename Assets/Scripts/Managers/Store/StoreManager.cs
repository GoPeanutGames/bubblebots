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
                    new StoreItem(){TopLine = "Item 1",Image = "Placeholder",BottomLine = "Price 1"},
                    new StoreItem(){TopLine = "Item 2",Image = "Placeholder",BottomLine = "Price 2"},
                    new StoreItem(){TopLine = "Item 3",Image = "Placeholder",BottomLine = "Price 3"},
                    new StoreItem(){TopLine = "Item 4",Image = "Placeholder",BottomLine = "Price 4"},
                    new StoreItem(){TopLine = "Item 5",Image = "Placeholder",BottomLine = "Price 5"},
                    new StoreItem(){TopLine = "Item 6",Image = "Placeholder",BottomLine = "Price 6"},
                }
            }
        },
        {StoreTabs.Bubbles, new StoreTab()
            {
                Layout = StoreTabContentLayout.Grid,
                Items = new List<StoreItem>()
                {
                    new StoreItem(){TopLine = "Item 1",Image = "Placeholder",BottomLine = "Price 1"},
                    new StoreItem(){TopLine = "Item 2",Image = "Placeholder",BottomLine = "Price 2"},
                    new StoreItem(){TopLine = "Item 3",Image = "Placeholder",BottomLine = "Price 3"},
                }
            }
        },
        {StoreTabs.Skins, new StoreTab()
            {
                Layout = StoreTabContentLayout.Grid,
                Items = new List<StoreItem>()
                {
                    new StoreItem(){TopLine = "Item 1",Image = "Placeholder",BottomLine = "Price 1"},
                    new StoreItem(){TopLine = "Item 2",Image = "Placeholder",BottomLine = "Price 2"},
                    new StoreItem(){TopLine = "Item 3",Image = "Placeholder",BottomLine = "Price 3"},
                }
            }
        },
        {StoreTabs.Offers, new StoreTab()
            {
                Layout = StoreTabContentLayout.Vertical,
                Items = new List<StoreItem>()
                {
                    new StoreItem(){TopLine = "Item 1",Image = "Placeholder",BottomLine = "Price 1"},
                    new StoreItem(){TopLine = "Item 2",Image = "Placeholder",BottomLine = "Price 2"},
                    new StoreItem(){TopLine = "Item 3",Image = "Placeholder",BottomLine = "Price 3"},
                }
            }
        },
    };

    private List<SpecialOffer> specialOffers = new()
    {
        new SpecialOffer() {Name = "Special offer 1"},
        new SpecialOffer() {Name = "Special offer 2"},
        new SpecialOffer() {Name = "Special offer 3"}
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
