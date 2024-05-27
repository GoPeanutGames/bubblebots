using UnityEngine.Device;

public class GameStateManageAccount : GameState
{
	private GamePopupManageAccount _gamePopupManageAccount;
	private GameScreenDarkenedBg _darkenedBg;
	private GameScreenLoading _gameScreenLoading;
	
	public override string GetGameStateName()
	{
		return "Game state manage account";
	}
	
	public override void Enable()
	{
		_darkenedBg = Screens.Instance.PushScreen<GameScreenDarkenedBg>(true);
		_gamePopupManageAccount = Screens.Instance.PushScreen<GamePopupManageAccount>(true);
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
				Screens.Instance.PopScreen(_darkenedBg);
				stateMachine.PopState();
				break;
			case ButtonId.ManageAccountDelete:
				stateMachine.PushState(new GameStateDeleteAccount());
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
			case ButtonId.LoginSignInMetamask:
				UserManager.Instance.loginManager.MetamaskSignIn(MetamaskSignInSuccess, null);
				break;
			case ButtonId.ManageAccountContact:
				Application.OpenURL(GameLinks.DiscordLink);
				break;
		}
	}

	private void SignOutSuccess()
	{
		_gamePopupManageAccount.UpdateSignOutButtons();
		Screens.Instance.PopScreen(_gameScreenLoading);
	}

	private void SignOut()
	{
		_gameScreenLoading = Screens.Instance.PushScreen<GameScreenLoading>();
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
	
	private void MetamaskSignInSuccess()
	{
		_gamePopupManageAccount.UpdateSignOutButtons();
	}
	
	public override void Disable()
	{
		Screens.Instance.PopScreen(_darkenedBg);
		Screens.Instance.PopScreen(_gamePopupManageAccount);
		GameEventsManager.Instance.RemoveGlobalListener(OnGameEvent);
	}
}
