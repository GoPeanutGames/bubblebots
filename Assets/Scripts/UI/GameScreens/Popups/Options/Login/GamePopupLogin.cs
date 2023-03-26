using TMPro;

public class GamePopupLogin : GameScreen
{
	public TextMeshProUGUI signInErrorText;
	public TMP_InputField emailInputFieldSignIn;
	public TMP_InputField passInputFieldSignIn;

	public string GetLoginInputFieldEmail()
	{
		return emailInputFieldSignIn.text;
	}

	public string GetLoginInputFieldPass()
	{
		return passInputFieldSignIn.text;
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
}
