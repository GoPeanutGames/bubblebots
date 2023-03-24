public class GameStateConfirmDeleteAccount : GameState
{
	private GamePopupConfirmDeleteAccount _gamePopupConfirmDeleteAccount;
	
	public override string GetGameStateName()
	{
		return "Game state confirm delete account";
	}
	
	public override void Enter()
	{
		_gamePopupConfirmDeleteAccount = Screens.Instance.PushScreen<GamePopupConfirmDeleteAccount>();
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
			case ButtonId.DeleteAccountConfirmClose:
				stateMachine.PopState();
				break;
			case ButtonId.DeleteAccountProceed:
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
	}

	public override void Exit()
	{
		Screens.Instance.PopScreen(_gamePopupConfirmDeleteAccount);
	}
}
