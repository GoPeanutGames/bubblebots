public class GameStateQuitPopup : GameState
{
	private GameQuitPopup _gameQuitPopup;
	private readonly string _warningText;

	public GameStateQuitPopup(string warningText)
	{
		this._warningText = warningText;
	}
	
	public override string GetGameStateName()
	{
		return "Game state robot select quit";
	}
	
	public override void Enter()
	{
		_gameQuitPopup = Screens.Instance.PushScreen<GameQuitPopup>();
		_gameQuitPopup.SetWarningText(_warningText);
		_gameQuitPopup.StartOpen();
		Screens.Instance.BringToFront<GameQuitPopup>();
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
			case ButtonId.QuitGameMenuPlay:
				stateMachine.PopState();
				break;
			case ButtonId.QuitGameMenuQuit:
				stateMachine.PopAll();
				break;
		}
	}
	
	public override void Disable()
	{
		GameEventsManager.Instance.RemoveGlobalListener(OnGameEvent);
	}

	public override void Exit()
	{
		_gameQuitPopup.StartClose();
	}
}