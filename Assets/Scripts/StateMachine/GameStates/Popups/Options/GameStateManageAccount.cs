using UnityEngine.Device;

public class GameStateManageAccount : GameState
{
	private GamePopupManageAccount _gamePopupManageAccount;
	
	public override string GetGameStateName()
	{
		return "Game state manage account";
	}
	
	public override void Enter()
	{
		_gamePopupManageAccount = Screens.Instance.PushScreen<GamePopupManageAccount>();
	}

	public override void Enable()
	{
		GameEventsManager.Instance.AddGlobalListener(OnGameEvent);
		_gamePopupManageAccount.UpdateSignOutButtons();
	}

	private void OnGameEvent(GameEventData data)
	{
		if (data.eventName == GameEvents.ButtonTap)
		{
			OnButtonTap(data);
		}
	}
	
	private void OnButtonTap(GameEventData data)
	{
		GameEventString customButtonData = data as GameEventString;
		switch (customButtonData.stringData)
		{
			case ButtonId.ManageAccountClose:
				stateMachine.PopState();
				break;
			case ButtonId.ManageAccountDelete:
				//TODO: push delete acc state
				break;
			case ButtonId.ManageAccountSignIn:
				stateMachine.PushState(new GameStateLogin());
				break;
			case ButtonId.ManageAccountSignOut:
				SignOut();
				break;
			case ButtonId.ManageAccountAppleSignIn:
				UserManager.Instance.loginManager.AppleSignIn(AppleSignInSuccess, null);
				break;
			case ButtonId.ManageAccountGoogleSignIn:
				UserManager.Instance.loginManager.GoogleSignIn(GoogleSignInSuccess, null);
				break;
			case ButtonId.ManageAccountContact:
				Application.OpenURL("https://discord.gg/gopeanutgames");
				break;
		}
	}

	private void SignOutSuccess()
	{
		_gamePopupManageAccount.UpdateSignOutButtons();
	}

	private void SignOut()
	{
		UserManager.Instance.loginManager.SignOut(SignOutSuccess);
	}

	private void AppleSignInSuccess()
	{
		_gamePopupManageAccount.UpdateSignOutButtons();
	}
	
	private void GoogleSignInSuccess()
	{
		_gamePopupManageAccount.UpdateSignOutButtons();
	}
	
	public override void Disable()
	{
		GameEventsManager.Instance.RemoveGlobalListener(OnGameEvent);
	}

	public override void Exit()
	{
		Screens.Instance.PopScreen(_gamePopupManageAccount);
	}
}
