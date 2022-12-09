using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StoreItemController : MonoBehaviour
{
    public TextMeshProUGUI TopText;
    public TextMeshProUGUI BottomText;
    public Image ItemImage;

    private void LoadImage(string path)
    {
        ItemImage.sprite = Resources.Load<Sprite>(path);
    }
    
    public void SetupItem(string top, string bottom, string image = "")
    {
        TopText.text = top;
        BottomText.text = bottom;
        if (string.IsNullOrEmpty(image) == false)
        {
            LoadImage(image);
        }
    }
}
