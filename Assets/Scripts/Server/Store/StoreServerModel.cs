using System;
using System.Collections.Generic;

namespace BubbleBots.Server.Store
{
    public enum StoreAPI
    {
        Bundles
    }

    [Serializable]
    public class BundleData
    {
        public string expiredAt;
        public string _id;
        public string name;
        public string description;
        public int position;
        public bool isActive;
        public bool isPromotion;
        public int price;
        public int gems;
    }
    
    [Serializable]
    public class GetBundlesData
    {
        public List<BundleData> bundles;
    }
}