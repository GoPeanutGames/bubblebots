using TMPro;

public class GameWonPopup : GameScreenAnimatedEntryExit
{
	public TextMeshProUGUI descriptionText;
	public TextMeshProUGUI actionButtonText;
	public CustomButton actionButton;

	public void SetDescriptionText(string text)
	{
		descriptionText.text = text;
	}

	public void SetActionButtonId(string buttonId)
	{
		actionButton.buttonId = buttonId;
	}
	
	public void SetActionButtonText(string buttonText)
	{
		actionButtonText.text = buttonText;
	}
}