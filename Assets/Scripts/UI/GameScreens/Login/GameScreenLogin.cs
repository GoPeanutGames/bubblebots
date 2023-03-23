using TMPro;
using UnityEngine;

public class GameScreenLogin : GameScreen
{
    public GameObject signInPopup;
    public GameObject signUpPopup;
    public GameObject twoFaPopup;
    public GameObject resetPassPopup;
    public GameObject setNewPassPopup;
    public GameObject loadingOverlay;

    public TextMeshProUGUI signUpErrorText;
    public TextMeshProUGUI signUpDescText;
    public TextMeshProUGUI twoFaCodeText;
    public TextMeshProUGUI codeTitle;
    public TextMeshProUGUI codeError;
    public TMP_InputField emailInputFieldSignUp;
    public TMP_InputField passInputFieldSignUp;
    public TMP_InputField codeInputField;

    public void ShowLoading()
    {
        loadingOverlay.SetActive(true);
    }

    public void HideLoading()
    {
        loadingOverlay.SetActive(false);
    }

    public void ShowSignIn()
    {
        signInPopup.SetActive(true);
        setNewPassPopup.SetActive(false);
    }
    
    public void Show2FAuth()
    {
        codeError.gameObject.SetActive(false);
        codeTitle.gameObject.SetActive(true);
        signInPopup.SetActive(false);
        signUpPopup.SetActive(false);
        twoFaPopup.SetActive(true);
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
        return codeInputField.text;
    }

    public bool SignUpValidation()
    {
        signUpDescText.gameObject.SetActive(true);
        signUpErrorText.text = "";
        if (string.IsNullOrEmpty(emailInputFieldSignUp.text) || string.IsNullOrEmpty(passInputFieldSignUp.text))
        {
            signUpDescText.gameObject.SetActive(false);
            signUpErrorText.text = "Email address or password cannot be empty";
            return false;
        }

        if (passInputFieldSignUp.text.Length < 8)
        {
            signUpDescText.gameObject.SetActive(false);
            signUpErrorText.text = "Password doesn't have enough characters.";
            return false;
        }

        return true;
    }

    public void SetSignUpWrongError()
    {
        signUpDescText.gameObject.SetActive(false);
        signUpErrorText.text = "Account already exists! Please use a different email address or sign in instead.";
    }

    public bool CodeValidation()
    {
        if (string.IsNullOrEmpty(codeInputField.text))
        {
            codeTitle.gameObject.SetActive(false);
            codeError.gameObject.SetActive(true);
            twoFaCodeText.text = "Enter again the authorization code that we sent to your email address.";
            return false;
        }

        return true;
    }

    public void Set2FAError()
    {
        twoFaCodeText.text = "Enter again the authorization code that we sent to your email address.";
        codeTitle.gameObject.SetActive(false);
        codeError.gameObject.SetActive(true);
    }
}