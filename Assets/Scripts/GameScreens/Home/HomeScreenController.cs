using UnityEngine;

public class HomeScreenController : MonoBehaviour
{
    public GameObject HomeScreen;
    public GameObject ModeSelectScreen;

    private void ResetAllScreens()
    {
        HomeScreen.SetActive(false);
        ModeSelectScreen.SetActive(false);
    }

    public void PlayButtonClick()
    {
        ResetAllScreens();
        ModeSelectScreen.SetActive(true);
    }

    public void BackToHome()
    {
        ResetAllScreens();
        HomeScreen.SetActive(true);
    }
}
