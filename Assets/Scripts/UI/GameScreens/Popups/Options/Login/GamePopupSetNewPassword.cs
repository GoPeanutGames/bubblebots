using TMPro;

public class GamePopupSetNewPassword : GameScreen
{
	public TextMeshProUGUI setNewPassDesc;
	public TextMeshProUGUI setNewPassError;
	public TMP_InputField newPassInputFieldSetNewPass;
	public TMP_InputField authCodeInputFieldSetNewPass;

	public string GetSetNewPassInputFieldPass()
	{
		return newPassInputFieldSetNewPass.text;
	}

	public string GetSetNewPassInputFieldAuthCode()
	{
		return authCodeInputFieldSetNewPass.text;
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
