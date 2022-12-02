using BubbleBots.Store;
using UnityEngine;

public class StoreScreenController : MonoBehaviour
{
    public GameObject GridLayoutPrefab;
    public GameObject VerticalLayoutPrefab;
    public GameObject GridItemPrefab;
    public GameObject VerticalItemPrefab;

    public GameObject ContentParent;

    private StoreTabs selectedTab = StoreTabs.Gems;
    private GameObject itemsContainer;

    private void Start()
    {
        GenerateTabContent();
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

    public void TabClick(int index)
    {
        selectedTab = (StoreTabs)index;
        GenerateTabContent();
    }
}
