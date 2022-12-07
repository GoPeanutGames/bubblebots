using UnityEngine;

public class PlayButtonController : MonoBehaviour
{
    public HomeScreenController HomeScreenController;
    public GameObject PlayText;
    public GameObject HomeText;

    private bool play = true;

    public void ButtonClick()
    {
        if (play)
        {
            HomeScreenController.OpenModeSelectScreen();
        }
        else
        {
            HomeScreenController.BackToHome();
        }
    }

    public void SetPlayButtonState(bool play)
    {
        this.play = play;
        PlayText.SetActive(play);
        HomeText.SetActive(!play);
    }
}
