using BubbleBots.Store;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpecialOfferController : MonoBehaviour
{
    public Image SpecialOfferImage;
    public TextMeshProUGUI ButtonText;
    public CustomStoreButton buyButton;
    
    private void LoadImage(string path)
    {
        SpecialOfferImage.sprite = Resources.Load<Sprite>(path);
    }
    
    public void SetupSpecialOffer(SpecialOffer offer)
    {
        ButtonText.text = offer.ButtonText;
        if (string.IsNullOrEmpty(offer.Image) == false)
        {
            LoadImage(offer.Image);
        }
        buyButton.SetBundleId(offer.Bundle.bundleId);
    }
    
    public void MoveOutDone()
    {
        Destroy(this.gameObject);
    }
}
