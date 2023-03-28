public class GameStateSignInToBuy : GameState
{
	private GamePopupSignInToBuy _gamePopupSignInToBuy;
	
	public override string GetGameStateName()
	{
		return "game state sign in to buy";
	}
	
	public override void Enter()
	{
		_gamePopupSignInToBuy = Screens.Instance.PushScreen<GamePopupSignInToBuy>();
		_gamePopupSignInToBuy.StartOpen();
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
			case ButtonId.SignInToBuyPopupClose:
				stateMachine.PopState();
				break;
			case ButtonId.SignInToBuyPopupSignIn:
				stateMachine.PopState();
				stateMachine.PushState(new GameStateManageAccount());
				break;
		}
	}
	
	public override void Disable()
	{
		GameEventsManager.Instance.RemoveGlobalListener(OnGameEvent);
	}

	public override void Exit()
	{
		Screens.Instance.PopScreen(_gamePopupSignInToBuy);
	}
}
