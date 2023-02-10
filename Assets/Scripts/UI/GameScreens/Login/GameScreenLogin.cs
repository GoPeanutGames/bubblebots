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

    public TextMeshProUGUI signInErrorText;
    public TextMeshProUGUI signUpErrorText;
    public TextMeshProUGUI signUpDescText;
    public TextMeshProUGUI twoFaCodeText;
    public TextMeshProUGUI resetPassError;
    public TextMeshProUGUI setNewPassDesc;
    public TextMeshProUGUI setNewPassError;
    public TextMeshProUGUI codeTitle;
    public TextMeshProUGUI codeError;
    public TMP_InputField emailInputFieldSignIn;
    public TMP_InputField passInputFieldSignIn;
    public TMP_InputField emailInputFieldSignUp;
    public TMP_InputField passInputFieldSignUp;
    public TMP_InputField codeInputField;
    public TMP_InputField emailInputFieldResetPass;
    public TMP_InputField newPassInputFieldSetNewPass;
    public TMP_InputField authCodeInputFieldSetNewPass;

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

    public void ShowSetNewPassword()
    {
        setNewPassDesc.gameObject.SetActive(true);
        setNewPassError.text = "";
        resetPassPopup.SetActive(false);
        setNewPassPopup.SetActive(true);
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

    public string GetLoginInputFieldEmail()
    {
        return emailInputFieldSignIn.text;
    }

    public string GetLoginInputFieldPass()
    {
        return passInputFieldSignIn.text;
    }

    public string GetResetPassInputFieldEmail()
    {
        return emailInputFieldResetPass.text;
    }

    public string GetSetNewPassInputFieldPass()
    {
        return newPassInputFieldSetNewPass.text;
    }

    public string GetSetNewPassInputFieldAuthCode()
    {
        return authCodeInputFieldSetNewPass.text;
    }

    public bool SignInValidation()
    {
        signInErrorText.text = "";
        if (string.IsNullOrEmpty(emailInputFieldSignIn.text) || string.IsNullOrEmpty(passInputFieldSignIn.text))
        {
            signInErrorText.text = "Please fill in all necessary information.";
            return false;
        }

        return true;
    }

    public void SetSignInWrongError()
    {
        signInErrorText.text = "You've entered the wrong password or email address!";
    }

    public void SetSignInPlatformError(string platform)
    {
        signInErrorText.text = platform + " sign in failed!";
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

    public void ResetPassResetScreen()
    {
        resetPassError.text = "";
    }
    
    public bool ResetPassValidation()
    {
        if (string.IsNullOrEmpty(emailInputFieldResetPass.text))
        {
            resetPassError.text = "Fill in a valid email address";
            return false;
        }
        return true;
    }

    public bool SetNewPassValidation()
    {
        if (string.IsNullOrEmpty(authCodeInputFieldSetNewPass.text) || string.IsNullOrEmpty(newPassInputFieldSetNewPass.text))
        {
            setNewPassDesc.gameObject.SetActive(false);
            setNewPassError.text = "Please fill in all necessary information.";
            return false;
        }
        if (newPassInputFieldSetNewPass.text.Length < 8)
        {
            setNewPassDesc.gameObject.SetActive(false);
            setNewPassError.text = "Password needs to have at least 8 characters.";
            return false;
        }
        return true;
    }

    public void SetNewPassError()
    {
        setNewPassDesc.gameObject.SetActive(false);
        setNewPassError.text = "Failed to set new password. Please check if your authentication code is correct.";
    }
}