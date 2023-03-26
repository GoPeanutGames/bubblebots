using TMPro;

public class GamePopupForgotPassword : GameScreen
{
	public TextMeshProUGUI resetPassError;
	public TMP_InputField emailInputFieldResetPass;

	public bool ResetPassValidation()
	{
		if (string.IsNullOrEmpty(emailInputFieldResetPass.text))
		{
			resetPassError.text = "Fill in a valid email address";
			return false;
		}
		return true;
	}
	
	public string GetResetPassInputFieldEmail()
	{
		return emailInputFieldResetPass.text;
	}
}
