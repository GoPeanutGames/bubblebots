using UnityEngine;

public class GameScreenLogin : GameScreen
{
    public GameObject LoginScreen;
    public GameObject GuestScreen;
    public GameObject LoadingScreen;

    public void OnPlayAsGuestPressed()
    {
        GuestScreen.SetActive(true);
        LoginScreen.SetActive(false);
    }

    public void ShowLoadingScreen()
    {
        LoadingScreen.SetActive(true);
    }

    public void HideLoginScreen()
    {
        LoginScreen.SetActive(false);
    }
}
