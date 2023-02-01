using System;
using System.Collections.Generic;
using BubbleBots.Server.Store;
using BubbleBots.Store;
using UnityEngine;

public class StoreManager : MonoSingleton<StoreManager>
{
    private Dictionary<StoreTabs, StoreTab> _storeTabItemsMap;
    private List<SpecialOffer> _specialOffers;
    private bool purchasesLoggedIn = false;
    private Purchases purchases;

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

    private string GetGemText(int gems)
    {
        return gems > 1 ? gems + " GEMS" : gems + " GEM";
    }

    private void AddStoreItem(StoreTabs tab, BundleData data)
    {
        CreateTabIfNotExists(tab);
        StoreItem storeItem = new StoreItem()
        {
            Bundle = data,
            TopLine = GetGemText(data.gems),
            Image = "Store/Gems/Gem Chest Small",
            BottomLine = "USD " + data.price.ToString("##.##")
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
            ButtonText = "USD " + data.price,
            Image = "Store/Gems/Special Offer Save 40 on Gems"
        };
        _specialOffers.Add(specialOffer);
    }

    private void Start()
    {
        purchases = this.GetComponent<Purchases>();
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

    public void InitialiseStore(string address)
    {
        this.GetComponent<Purchases>().LogIn(address, PurchasesLoginCompleted);
    }

    private void PurchasesLoginCompleted(Purchases.CustomerInfo info, bool success, Purchases.Error error)
    {
        purchasesLoggedIn = success;
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

    public void BuyBundle(int bundleId, Action transactionSuccess, Action<string> transactionFail)
    {
        if (!purchasesLoggedIn)
            return;
        Debug.Log("buy");
        GetBundlesData((bundles) =>
        {
            BundleData data = bundles.Find((a) => a.bundleId == bundleId);
            purchases.GetOfferings((offerings, error) =>
            {
                foreach (Purchases.Offering offering in offerings.All.Values)
                {
                    foreach (Purchases.Package package in offering.AvailablePackages)
                    {
                        if (package.Identifier == data.googleId)
                        {
                            Debug.Log("found package");
                            purchases.PurchasePackage(package,
                                (productIdentifier, customerInfo, userCancelled, error) =>
                                {
                                    if (!userCancelled)
                                    {
                                        if (error != null)
                                        {
                                            // show error
                                            transactionFail?.Invoke(error.ReadableErrorCode + ":" +
                                                                    error.UnderlyingErrorMessage);
                                            
                                            Debug.Log("fail");
                                        }
                                        else
                                        {
                                            // show updated Customer Info
                                            transactionSuccess?.Invoke();
                                            
                                            Debug.Log("success");
                                        }
                                    }
                                    else
                                    {
                                        // user cancelled, don't show an error
                                    }
                                });
                        }
                    }
                }
            });
        });
    }
}