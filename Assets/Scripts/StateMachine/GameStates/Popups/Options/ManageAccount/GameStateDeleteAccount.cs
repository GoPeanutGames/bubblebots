public class GameStateDeleteAccount : GameState
{
	private GameScreenDarkenedBg _darkenedBg;
	private GamePopupDeleteAccount _gamePopupDeleteAccount;
	
	public override string GetGameStateName()
	{
		return "Game state delete account";
	}

	public override void Enable()
	{
		_darkenedBg = Screens.Instance.PushScreen<GameScreenDarkenedBg>();
		_gamePopupDeleteAccount = Screens.Instance.PushScreen<GamePopupDeleteAccount>();
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
			case ButtonId.DeleteAccountClose:
				stateMachine.PopState();
				break;
			case ButtonId.DeleteAccountProceed:
				UserManager.Instance.loginManager.AskDeleteAccount(AskDeleteSuccess);
				break;
		}
	}

	private void AskDeleteSuccess()
	{
		stateMachine.PopState();
		stateMachine.PushState(new GameStateConfirmDeleteAccount());
	}

	public override void Disable()
	{
		GameEventsManager.Instance.RemoveGlobalListener(OnGameEvent);
		Screens.Instance.PopScreen(_darkenedBg);
		Screens.Instance.PopScreen(_gamePopupDeleteAccount);
	}
}
