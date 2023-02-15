public class GameStateFreeModeTooltip : GameState
{
	private GamePopupFreeModeTooltip _gamePopupFreeModeTooltip;
	
	public override string GetGameStateName()
	{
		return "Game state free mode tooltip";
	}

	public override void Enter()
	{
		_gamePopupFreeModeTooltip = Screens.Instance.PushScreen<GamePopupFreeModeTooltip>();
		_gamePopupFreeModeTooltip.StartOpen();
		Screens.Instance.BringToFront<GamePopupFreeModeTooltip>();
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
			case ButtonId.ModeSelectFreeModeTooltipBack:
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
		_gamePopupFreeModeTooltip.StartClose();
	}
}