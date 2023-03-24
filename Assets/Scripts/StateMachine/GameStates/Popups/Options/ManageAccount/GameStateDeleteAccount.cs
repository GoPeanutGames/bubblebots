public class GameStateDeleteAccount : GameState
{
	private GamePopupDeleteAccount _gamePopupDeleteAccount;
	
	public override string GetGameStateName()
	{
		return "Game state delete account";
	}
	
	public override void Enter()
	{
		_gamePopupDeleteAccount = Screens.Instance.PushScreen<GamePopupDeleteAccount>();
	}

	public override void Enable()
	{
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
	}

	public override void Exit()
	{
		Screens.Instance.PopScreen(_gamePopupDeleteAccount);
	}
}
