using BubbleBots.Store;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StoreScreenController : MonoBehaviour
{
    public GameObject GridLayoutPrefab;
    public GameObject VerticalLayoutPrefab;
    public GameObject GridItemPrefab;
    public GameObject VerticalItemPrefab;
    public GameObject SpecialOfferPrefab;

    public GameObject ContentParent;
    public GameObject SpecialOffersContentParent;

    private StoreTabs selectedTab = StoreTabs.Gems;
    private GameObject itemsContainer;
    private int currentlyShowingOfferIndex = 0;
    private GameObject currentSpecialOfferGO;

    private void Start()
    {
        GenerateTabContent();
        GenerateSpecialOffer();
    }

    private void GenerateTabContent()
    {
        if(itemsContainer != null)
        {
            Destroy(itemsContainer);
        }
        StoreTab tabData = StoreManager.Instance.GetStoreTabContent(selectedTab);
        GameObject layoutPrefab = tabData.Layout == StoreTabContentLayout.Grid ? GridLayoutPrefab : VerticalLayoutPrefab;
        GameObject itemPrefab = tabData.Layout == StoreTabContentLayout.Grid ? GridItemPrefab : VerticalItemPrefab;
        itemsContainer = GameObject.Instantiate(layoutPrefab, ContentParent.transform);
        foreach(StoreItem itemData in tabData.Items)
        {
            GameObject itemGO = GameObject.Instantiate(itemPrefab, itemsContainer.transform);
            itemGO.GetComponent<StoreItemController>().SetupItem(itemData.TopLine, itemData.BottomLine, itemData.Image);
        }
    }

    private void GenerateSpecialOffer()
    {
        List<SpecialOffer> specialOffers = StoreManager.Instance.GetSpecialOffers();
        SpecialOffer offer = specialOffers[currentlyShowingOfferIndex];
        currentSpecialOfferGO = GameObject.Instantiate(SpecialOfferPrefab, SpecialOffersContentParent.transform);
        currentSpecialOfferGO.GetComponentInChildren<TextMeshProUGUI>().text = offer.Name;
    }

    public void TabClick(int index)
    {
        selectedTab = (StoreTabs)index;
        GenerateTabContent();
    }

    public void ClickRight()
    {
        currentSpecialOfferGO.GetComponent<Image>().color = Color.red;
        currentSpecialOfferGO.GetComponent<Animator>().SetTrigger("MoveOutLeft");
        currentlyShowingOfferIndex++;
        if(currentlyShowingOfferIndex >= StoreManager.Instance.GetSpecialOffers().Count)
        {
            currentlyShowingOfferIndex = 0;
        }
        GenerateSpecialOffer();
        currentSpecialOfferGO.GetComponent<Animator>().SetTrigger("MoveInRight");
    }

    public void ClickLeft()
    {
        currentSpecialOfferGO.GetComponent<Image>().color = Color.red;
        currentSpecialOfferGO.GetComponent<Animator>().SetTrigger("MoveOutRight");
        currentlyShowingOfferIndex--;
        if (currentlyShowingOfferIndex < 0)
        {
            currentlyShowingOfferIndex = StoreManager.Instance.GetSpecialOffers().Count - 1;
        }
        GenerateSpecialOffer();
        currentSpecialOfferGO.GetComponent<Animator>().SetTrigger("MoveInLeft");
    }
}
