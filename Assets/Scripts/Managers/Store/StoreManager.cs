using System;
using BubbleBots.Store;
using System.Collections.Generic;
using BubbleBots.Server.Store;
using UnityEngine;

public class StoreManager : MonoSingleton<StoreManager>
{
    private Dictionary<StoreTabs, StoreTab> _storeTabItemsMap;
    private List<SpecialOffer> _specialOffers;

    private void CreateTabIfNotExists(StoreTabs tab)
    {
        _storeTabItemsMap ??= new Dictionary<StoreTabs, StoreTab>();
        if (_storeTabItemsMap.ContainsKey(tab) == false)
        {
            _storeTabItemsMap[tab] = new StoreTab()
            {
                Layout = StoreTabContentLayout.Grid,
                Items = new List<StoreItem>()
            };
        }
    }

    private void AddStoreItem(StoreTabs tab, BundleData data)
    {
        CreateTabIfNotExists(tab);
        StoreItem storeItem = new StoreItem()
        {
            Bundle = data,
            TopLine = data.gems + "GEMS",
            Image = "Store/Gems/Gem Chest Small",
            BottomLine = "USDC " + data.price
        };
        _storeTabItemsMap[tab].Items.Add(storeItem);
    }

    private void AddSpecialOffer(BundleData data)
    {
        Debug.Log("ADDING OFFERS");
        _specialOffers ??= new List<SpecialOffer>();
        SpecialOffer specialOffer = new()
        {
            Bundle = data,
            ButtonText = "USDC " + data.price,
            Image = "Store/Gems/Special Offer Save 40 on Gems"
        };
        _specialOffers.Add(specialOffer);
    }

    private void Start()
    {
        GetBundlesData((bundles) =>
        {
            foreach (BundleData bundleData in bundles)
            {
                if (bundleData.isPromotion)
                {
                    AddSpecialOffer(bundleData);
                }
                else
                {
                    AddStoreItem(StoreTabs.Gems, bundleData);
                }
            }
        });
    }

    private void GetBundlesData(Action<List<BundleData>> bundleCallback)
    {
        ServerManager.Instance.GetStoreDataFromServer(StoreAPI.Bundles, (jsonData) =>
        {
            GetBundlesData bundlesData = JsonUtility.FromJson<GetBundlesData>(jsonData);
            bundleCallback(bundlesData.bundles);
        });
    }

    public StoreTab GetStoreTabContent(StoreTabs tab)
    {
        return _storeTabItemsMap[tab];
    }

    public List<SpecialOffer> GetSpecialOffers()
    {
        return _specialOffers;
    }

    public void GetBundleFromId(int bundleId, Action<BundleData> bundleCallback)
    {
        GetBundlesData((bundles) =>
        {
            BundleData data = bundles.Find((a) => a.bundleId == bundleId);
            bundleCallback(data);
        });
    }
}