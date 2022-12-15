using BubbleBots.Server.Store;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameScreenConfirmTransaction : GameScreen
{
    public GameObject loadingParent;
    public GameObject contentParent;
    public TextMeshProUGUI bundleGems;
    public Image bundleImage;

    public void SetLoading()
    {
        loadingParent.SetActive(true);
        contentParent.SetActive(false);
    }

    public void RemoveLoading()
    {
        loadingParent.SetActive(false);
        contentParent.SetActive(true);
    }

    public void SetBundleData(BundleData bundleData)
    {
        bundleGems.text = bundleData.gems + " GEM";
        bundleImage.sprite = Resources.Load<Sprite>("Store/Gems/Gem Chest Small");
    }
}