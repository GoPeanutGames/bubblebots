using UnityEngine;

public class GamePopupManageAccount : GameScreen
{
	public GameObject SignedInSection;
	public GameObject SignedOutSection;
	public CustomButton DeleteButton;

	private void Start()
	{
		UpdateSignOutButtons();
	}

	public void UpdateSignOutButtons()
	{
		SignedInSection.SetActive(UserManager.PlayerType == PlayerType.LoggedInUser);
		SignedOutSection.SetActive(UserManager.PlayerType == PlayerType.Guest);
		DeleteButton.interactable = UserManager.PlayerType == PlayerType.LoggedInUser;
	}
}