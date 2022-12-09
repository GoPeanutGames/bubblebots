using System;
using System.Collections.Generic;

namespace BubbleBots.Store
{
    public enum StoreTabs { Gems, Skins, Offers, Nuts }
    public enum StoreTabContentLayout { Grid, Vertical }

    [Serializable]
    public class StoreItem
    {
        public string TopLine;
        public string Image;
        public string BottomLine;
    }

    [Serializable]
    public class StoreTab
    {
        public StoreTabContentLayout Layout;
        public List<StoreItem> Items;
    }

    public class SpecialOffer
    {
        public string Name;
    }
}