using System.Collections.Generic;
using BubbleBots.Store;
using TMPro;
using UnityEngine;

public class GameScreenStore : GameScreen
{
    [Header("Prefabs")] public GameObject GridLayoutPrefab;
    public GameObject VerticalLayoutPrefab;
    public GameObject GridItemPrefab;
    public GameObject VerticalItemPrefab;
    public GameObject SpecialOfferPrefab;

    [Header("References")] public List<GameStoreTab> StoreTabs;
    public GameObject ContentContainer;
    public GameObject SpecialOffersContentParent;

    private GameObject _spawnedItemContainer;
    private StoreTabContentLayout _currentLayout;
    private GameObject _currentSpecialOfferGO;

    private void DeactivateAllTabs()
    {
        foreach (GameStoreTab storeTab in StoreTabs)
        {
            storeTab.DeactivateTab();
        }
    }

    private void ClearContentIfExists()
    {
        if (_spawnedItemContainer != null)
        {
            Destroy(_spawnedItemContainer);
        }
    }

    public void GenerateLayout(StoreTabContentLayout layout)
    {
        _currentLayout = layout;
        ClearContentIfExists();
        if (layout == StoreTabContentLayout.Grid)
        {
            _spawnedItemContainer = Instantiate(GridLayoutPrefab, ContentContainer.transform);
        }
        else if (layout == StoreTabContentLayout.Vertical)
        {
            _spawnedItemContainer = Instantiate(VerticalLayoutPrefab, ContentContainer.transform);
        }
    }

    public void GenerateStoreItems(List<StoreItem> items)
    {
        foreach (StoreItem storeItem in items)
        {
            GameObject itemGO = Instantiate(_currentLayout == StoreTabContentLayout.Grid ? GridItemPrefab : VerticalItemPrefab, _spawnedItemContainer.transform);
            itemGO.GetComponent<StoreItemController>().SetupItem(storeItem.TopLine, storeItem.BottomLine, storeItem.Image);
        }
    }

    public void GenerateSpecialOffer(SpecialOffer offer)
    {
        GameObject offerGO = Instantiate(SpecialOfferPrefab, SpecialOffersContentParent.transform);
        offerGO.GetComponent<SpecialOfferController>().SetupSpecialOffer(offer.ButtonText, offer.Image);
        _currentSpecialOfferGO = offerGO;
    }

    public void MoveInLeftSpecialOffer()
    {
        _currentSpecialOfferGO.GetComponent<Animator>().SetTrigger("MoveInLeft");
    }

    public void MoveOutLeftSpecialOffer()
    {
        _currentSpecialOfferGO.GetComponent<Animator>().SetTrigger("MoveOutLeft");
    }

    public void MoveInRightSpecialOffer()
    {
        _currentSpecialOfferGO.GetComponent<Animator>().SetTrigger("MoveInRight");
    }

    public void MoveOutRightSpecialOffer()
    {
        _currentSpecialOfferGO.GetComponent<Animator>().SetTrigger("MoveOutRight");
    }

    public void ActivateTab(StoreTabs tab)
    {
        DeactivateAllTabs();
        StoreTabs[(int)tab].ActivateTab();
    }
}