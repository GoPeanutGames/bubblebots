using BubbleBots.Store;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StoreItemController : MonoBehaviour
{
    public TextMeshProUGUI TopText;
    public TextMeshProUGUI BottomText;
    public Image ItemImage;
    public CustomStoreButton buyButton;

    private void LoadImage(string path)
    {
        ItemImage.sprite = Resources.Load<Sprite>(path);
    }
    
    public void SetupItem(StoreItem item)
    {
        TopText.text = item.TopLine;
        BottomText.text = item.BottomLine;
        if (string.IsNullOrEmpty(item.Image) == false)
        {
            LoadImage(item.Image);
        }
        buyButton.SetBundleId(item.Bundle.bundleId);
    }
}
