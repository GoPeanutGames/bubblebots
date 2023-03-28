public class GameStateSaveYourProgress : GameState
{
	private GamePopupSaveYourProgress _gamePopupSaveYourProgress;
	
	public override string GetGameStateName()
	{
		return "game state save your progress";
	}
	
	public override void Enter()
	{
		_gamePopupSaveYourProgress = Screens.Instance.PushScreen<GamePopupSaveYourProgress>();
		_gamePopupSaveYourProgress.StartOpen();
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
			case ButtonId.SaveYourProgressPopupClose:
				stateMachine.PopState();
				break;
			case ButtonId.SaveYourProgressPopupSave:
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
		Screens.Instance.PopScreen(_gamePopupSaveYourProgress);
	}
}
