using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpecialOfferController : MonoBehaviour
{
    public Image SpecialOfferImage;
    public TextMeshProUGUI ButtonText;
    
    private void LoadImage(string path)
    {
        SpecialOfferImage.sprite = Resources.Load<Sprite>(path);
    }
    
    public void SetupSpecialOffer(string buttonLine, string image = "")
    {
        ButtonText.text = buttonLine;
        if (string.IsNullOrEmpty(image) == false)
        {
            LoadImage(image);
        }
    }
    
    public void MoveOutDone()
    {
        Destroy(this.gameObject);
    }
}
