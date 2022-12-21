using UnityEngine;

public class GameScreenMainMenuBottomHUD : GameScreen
{
    public GameObject PlayButton;
    public GameObject HomeButton;
    public GameObject StoreButtonGlow;

    public void ShowHomeButton()
    {
        PlayButton.SetActive(false);
        HomeButton.SetActive(true);
    }

    public void HideHomeButton()
    {
        PlayButton.SetActive(true);
        HomeButton.SetActive(false);
    }

    public void ActivateStoreButtonGlow()
    {
        StoreButtonGlow.SetActive(true);
    }

    public void DeactivateStoreButtonGlow()
    {
        StoreButtonGlow.SetActive(false);
    }
}
