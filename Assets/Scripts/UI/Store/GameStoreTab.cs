using UnityEngine;
using UnityEngine.UI;

public class GameStoreTab : MonoBehaviour
{
    public Image TabImage;
    public GameObject TabGlow;
    public Sprite ActiveTab;
    public Sprite InactiveTab;

    public void ActivateTab()
    {
        TabImage.sprite = ActiveTab;
        TabGlow.SetActive(true);
    }

    public void DeactivateTab()
    {
        TabImage.sprite = InactiveTab;
        TabGlow.SetActive(false);
    }

}
