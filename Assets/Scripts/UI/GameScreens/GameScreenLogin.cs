using TMPro;
using UnityEngine;

public class GameScreenLogin : GameScreen
{
    public GameObject LoginScreen;
    public GameObject EmailPassScreen;
    public GameObject EmailPassLoginScreen;
    public GameObject EmailPassSignUpScreen;
    public GameObject EmailPassSignUpLogin2ndScreen;
    public GameObject GuestScreen;
    public GameObject LoadingScreen;

    public TMP_InputField emailInputFieldLogin;
    public TMP_InputField passInputFieldLogin;
    public TMP_InputField emailInputFieldSignUp;
    public TMP_InputField passInputFieldSignUp;
    public TMP_InputField codeInputFieldSignUp;

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

    public void ShowLoginScreen1stStep()
    {
        EmailPassScreen.SetActive(false);
        EmailPassLoginScreen.SetActive(true);
    }

    public void ShowSignUpLogin2ndStep()
    {
        EmailPassLoginScreen.SetActive(false);
        EmailPassSignUpScreen.SetActive(false);
        EmailPassSignUpLogin2ndScreen.SetActive(true);
    }

    public string GetSignUpInputFieldEmail()
    {
        return emailInputFieldSignUp.text;
    }
    
    public string GetSignUpInputFieldPass()
    {
        return passInputFieldSignUp.text;
    }

    public string GetLoginInputFieldCode()
    {
        return codeInputFieldSignUp.text;
    }

    public string GetLoginInputFieldEmail()
    {
        return emailInputFieldLogin.text;
    }

    public string GetLoginInputFieldPass()
    {
        return passInputFieldLogin.text;
    }
}

