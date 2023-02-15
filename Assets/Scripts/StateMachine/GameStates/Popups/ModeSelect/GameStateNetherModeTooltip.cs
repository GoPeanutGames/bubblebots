public class GameStatenetherModeTooltip : GameState
{
	private GamePopupNetherModeTooltip _gamePopupNetherModeTooltip;
	
	public override string GetGameStateName()
	{
		return "Game state nether mode tooltip";
	}

	public override void Enter()
	{
		_gamePopupNetherModeTooltip = Screens.Instance.PushScreen<GamePopupNetherModeTooltip>();
		_gamePopupNetherModeTooltip.StartOpen();
		Screens.Instance.BringToFront<GamePopupNetherModeTooltip>();
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
			case ButtonId.ModeSelectNetherModeTooltipBack:
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
		_gamePopupNetherModeTooltip.StartClose();
	}
}