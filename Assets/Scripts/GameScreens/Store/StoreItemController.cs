using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StoreItemController : MonoBehaviour
{
    public TextMeshProUGUI TopText;
    public TextMeshProUGUI BottomText;
    public Image ItemImage;

    public void SetupItem(string top, string bottom, string image = "")
    {
        TopText.text = top;
        BottomText.text = bottom;
    }
}
