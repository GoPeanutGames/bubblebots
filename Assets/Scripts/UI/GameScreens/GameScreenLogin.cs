using TMPro;
using UnityEngine;

public class GameScreenLogin : GameScreen
{
    public GameObject LoginScreen;
    public GameObject EmailPassScreen;
    public GameObject EmailPassSignUpScreen;
    public GameObject GuestScreen;
    public GameObject LoadingScreen;

    public TMP_InputField emailInputFieldSignUp;
    public TMP_InputField passInputFieldSignUp;

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

    public void ShowEmailPassLoginSignupScreen()
    {
        LoginScreen.SetActive(false);
        EmailPassScreen.SetActive(true);
    }

    public void ShowEmailPassSignupScreen()
    {
        EmailPassScreen.SetActive(false);
        EmailPassSignUpScreen.SetActive(true);
    }

    public string GetSignUpInputFieldEmail()
    {
        return emailInputFieldSignUp.text;
    }
    
    public string GetSignUpInputFieldPass()
    {
        return passInputFieldSignUp.text;
    }
}

