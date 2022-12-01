using UnityEngine;

public class HomeScreenController : MonoBehaviour
{
    public PlayButtonController PlayButtonController;
    public GameObject UpperInfoContainer;
    public GameObject BottomMenuContainer;
    public GameObject HomeScreen;
    public GameObject ModeSelectScreen;
    public GameObject StoreScreen;

    private void SetInfoContainers(bool upperActive, bool bottomActive)
    {
        UpperInfoContainer.SetActive(upperActive);
        BottomMenuContainer.SetActive(bottomActive);
    }

    private void ResetAllScreens()
    {
        HomeScreen.SetActive(false);
        ModeSelectScreen.SetActive(false);
    }

    public void OpenModeSelectScreen()
    {
        ResetAllScreens();
        SetInfoContainers(true, false);
        ModeSelectScreen.SetActive(true);
    }

    public void OpenStoreScreen()
    {
        ResetAllScreens();
        SetInfoContainers(true, true);
        PlayButtonController.SetPlayButtonState(false);
        StoreScreen.SetActive(true);
    }

    public void BackToHome()
    {
        ResetAllScreens();
        SetInfoContainers(true, true);
        PlayButtonController.SetPlayButtonState(true);
        HomeScreen.SetActive(true);
    }
}
