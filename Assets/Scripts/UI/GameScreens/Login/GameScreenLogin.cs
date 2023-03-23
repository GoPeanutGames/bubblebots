using TMPro;
using UnityEngine;

public class GameScreenLogin : GameScreen
{
    public GameObject loadingOverlay;

    public TextMeshProUGUI twoFaCodeText;
    public TextMeshProUGUI codeTitle;
    public TextMeshProUGUI codeError;
    public TMP_InputField codeInputField;

    public void ShowLoading()
    {
        loadingOverlay.SetActive(true);
    }

    public void HideLoading()
    {
        loadingOverlay.SetActive(false);
    }

    public string GetLoginInputFieldCode()
    {
        return codeInputField.text;
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