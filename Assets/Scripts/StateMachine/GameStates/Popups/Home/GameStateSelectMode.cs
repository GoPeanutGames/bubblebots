using BubbleBots.Server.Player;

public class GameStateSelectMode : GameState
{
	private GamePopupSelectMode _gamePopupSelectMode;
	private GameScreenLoading _gameScreenLoading;
	private GameScreenNotEnoughGems _gameScreenNotEnoughGems;
	
	public override string GetGameStateName()
	{
		return "Game state select mode";
	}
	
	public override void Enter()
	{
		_gamePopupSelectMode = Screens.Instance.PushScreen<GamePopupSelectMode>();
		_gamePopupSelectMode.StartOpen();
		Screens.Instance.BringToFront<GamePopupSelectMode>();
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
			case ButtonId.ModeSelectCloseButton:
				stateMachine.PopState();
				break;
			case ButtonId.ModeSelectFreeMode:
				ClearStatesAndScreens();
				stateMachine.PushState(new GameStateFreeMode());
				break;
			case ButtonId.ModeSelectFreeModeTooltip:
				stateMachine.PushState(new GameStateFreeModeTooltip());
				break;
			case ButtonId.ModeSelectNetherModeTooltip:
				stateMachine.PushState(new GameStateNetherModeTooltip());
				break;
			case ButtonId.ModeSelectNethermode:
				_gameScreenLoading = Screens.Instance.PushScreen<GameScreenLoading>();
				UserManager.CallbackWithResources += ResourcesReceived;
				UserManager.Instance.GetPlayerResources();
				break;
			case ButtonId.NotEnoughGemsBack:
				Screens.Instance.PopScreen(_gameScreenNotEnoughGems);
				break;
		}
	}

	private void ClearStatesAndScreens()
	{
		while (Screens.GetCurrentScreen() != null)
		{
			Screens.Instance.PopScreen();
		}
		stateMachine.PopAll();
	}
	
	private void ResourcesReceived(GetPlayerWallet wallet)
	{
		Screens.Instance.PopScreen(_gameScreenLoading);
		UserManager.CallbackWithResources -= ResourcesReceived;
		if (wallet.gems <= 0)
		{
			Screens.Instance.PushScreen<GameScreenNotEnoughGems>();
			return;
		}
		ClearStatesAndScreens();
		stateMachine.PushState(new GameStateNetherMode());
	}
	
	public override void Disable()
	{
		GameEventsManager.Instance.RemoveGlobalListener(OnGameEvent);
	}

	public override void Exit()
	{
		_gamePopupSelectMode.StartClose();
	}
}