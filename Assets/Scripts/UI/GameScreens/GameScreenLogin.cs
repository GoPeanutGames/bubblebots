using UnityEngine;

public class GameScreenLogin : GameScreen
{
    public GameObject LoginScreen;
    public GameObject GuestScreen;
    public GameObject LoadingScreen;

    public GameObject metamaskButton1;
    public GameObject metamaskButton2;


    public void HideMetamaskButtons()
    {
        metamaskButton1.SetActive(false);
        metamaskButton2.SetActive(false);
    }

    public void OnPlayAsGuestPressed()
    {
        GuestScreen.SetActive(true);
        LoginScreen.SetActive(false);
    }

    public void ShowLoadingScreen()
    {
        LoadingScreen.SetActive(true);
    }

    public void HideLoadingScreen()
    {
        LoadingScreen.SetActive(false);
    }

    public void HideLoginScreen()
    {
        LoginScreen.SetActive(false);
    }
}

