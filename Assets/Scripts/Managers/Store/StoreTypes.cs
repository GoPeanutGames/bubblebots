using System;
using System.Collections.Generic;
using BubbleBots.Server.Store;

namespace BubbleBots.Store
{
    public enum StoreTabs
    {
        Gems,
        Skins,
        Offers,
        Nuts
    }

    public enum StoreTabContentLayout
    {
        Grid,
        Vertical
    }

    public class BaseStoreItem
    {
        public BundleData Bundle;
    }

    public class StoreItem : BaseStoreItem
    {
        public string TopLine;
        public string Image;
        public string BottomLine;
    }

    public class StoreTab
    {
        public StoreTabContentLayout Layout;
        public List<StoreItem> Items;
    }

    public class SpecialOffer : BaseStoreItem
    {
        public string ButtonText;
        public string Image;
    }
}