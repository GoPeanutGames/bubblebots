public class GameStateConfirmDeleteAccount : GameState
{
	private GameScreenDarkenedBg _darkenedBg;
	private GamePopupConfirmDeleteAccount _gamePopupConfirmDeleteAccount;
	
	public override string GetGameStateName()
	{
		return "Game state confirm delete account";
	}

	public override void Enable()
	{
		_darkenedBg = Screens.Instance.PushScreen<GameScreenDarkenedBg>();
		_gamePopupConfirmDeleteAccount = Screens.Instance.PushScreen<GamePopupConfirmDeleteAccount>();
		GameEventsManager.Instance.AddGlobalListener(OnGameEvent);
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
			case ButtonId.DeleteAccountConfirmClose:
				stateMachine.PopState();
				break;
			case ButtonId.DeleteAccountConfirmProceed:
				UserManager.Instance.loginManager.DeleteAccount(_gamePopupConfirmDeleteAccount.GetCodeConfirmDelete(), DeleteAccountSuccess);
				break;
			case ButtonId.DeleteAccountConfirmNoCode:
				UserManager.Instance.loginManager.AskDeleteAccount(null);
				break;
		}
	}

	private void DeleteAccountSuccess()
	{
		stateMachine.PopState();
	}
	
	public override void Disable()
	{
		GameEventsManager.Instance.RemoveGlobalListener(OnGameEvent);
		Screens.Instance.PopScreen(_gamePopupConfirmDeleteAccount);
		Screens.Instance.PopScreen(_darkenedBg);
	}
}
