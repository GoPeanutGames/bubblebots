using TMPro;

public class GamePopupRegister : GameScreen
{
	public TextMeshProUGUI signUpErrorText;
	public TMP_InputField emailInputFieldSignUp;
	public TMP_InputField passInputFieldSignUp;
	
	public string GetSignUpInputFieldEmail()
	{
		return emailInputFieldSignUp.text;
	}

	public string GetSignUpInputFieldPass()
	{
		return passInputFieldSignUp.text;
	}
	
	public bool SignUpValidation()
	{
		signUpErrorText.text = "";
		if (string.IsNullOrEmpty(emailInputFieldSignUp.text) || string.IsNullOrEmpty(passInputFieldSignUp.text))
		{
			signUpErrorText.text = "Email address or password cannot be empty";
			return false;
		}

		if (passInputFieldSignUp.text.Length < 8)
		{
			signUpErrorText.text = "Password doesn't have enough characters.";
			return false;
		}

		return true;
	}

	public void SetSignUpWrongError()
	{
		signUpErrorText.text = "Account already exists! Please use a different email address or sign in instead.";
	}
}
