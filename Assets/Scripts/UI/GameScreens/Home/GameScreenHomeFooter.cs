using UnityEngine;

public class GameScreenHomeFooter : GameScreenAnimatedShowHide
{
    public GameObject PlayButton;
    public GameObject HomeButton;

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
}
