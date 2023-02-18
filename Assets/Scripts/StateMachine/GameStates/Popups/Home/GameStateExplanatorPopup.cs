public class GameStateExplanatorPopup : GameState
{
	private GamePopupExplanator _gamePopupExplanator;
	
	public override string GetGameStateName()
	{
		return "Game state explanator popup";
	}
	
	public override void Enter()
	{
		_gamePopupExplanator = Screens.Instance.PushScreen<GamePopupExplanator>();
		_gamePopupExplanator.StartOpen();
		Screens.Instance.BringToFront<GamePopupExplanator>();
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
			case ButtonId.ExplanatorPopupClose:
				stateMachine.PopState();
				break;
		}
	}
	
	public override void Disable()
	{
		GameEventsManager.Instance.RemoveGlobalListener(OnGameEvent);
	}

	public override void Exit()
	{
		Screens.Instance.PopScreen(_gamePopupExplanator);
	}
}
