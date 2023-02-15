public class GameStateHome : GameState
{
	private GameScreenHomeFooter _gameScreenHomeFooter;
	private GameScreenHomeHeader _gameScreenHomeHeader;
	private GameScreenHomeSideBar _gameScreenHomeSideBar;
	private GameScreenHome _gameScreenHome;
	private GameScreenLoading _gameScreenLoading;
	private GameScreenNotEnoughGems _gameScreenNotEnoughGems;

	public override string GetGameStateName()
	{
		return "game state main menu";
	}

	private void ResetMainMenuLook()
	{
		_gameScreenHomeFooter.HideHomeButton();
		_gameScreenHomeHeader.ShowPlayerInfoGroup();
	}

	public override void Enter()
	{
		base.Enter();
		_gameScreenHome = Screens.Instance.PushScreen<GameScreenHome>(true);
		_gameScreenHomeHeader = Screens.Instance.PushScreen<GameScreenHomeHeader>(true);
		_gameScreenHomeFooter = Screens.Instance.PushScreen<GameScreenHomeFooter>(true);
		_gameScreenHomeSideBar = Screens.Instance.PushScreen<GameScreenHomeSideBar>(true);
	}

	public override void Enable()
	{
		_gameScreenHomeFooter.Show();
		_gameScreenHomeSideBar.Show();
		_gameScreenHomeHeader.Show();
		_gameScreenHomeHeader.RefreshData();
		GameEventsManager.Instance.AddGlobalListener(OnGameEvent);
		ResetMainMenuLook();
		if (_gameScreenHomeHeader.AreResourcesSet() == false)
		{
			_gameScreenLoading = Screens.Instance.PushScreen<GameScreenLoading>();
		}

		Screens.Instance.BringToFront<GameScreenLoading>();
		GameScreenHomeHeader.ResourcesSet += ResourcesSet;
		UserManager.Instance.GetPlayerResources();
	}

	private void ResourcesSet()
	{
		Screens.Instance.PopScreen(_gameScreenLoading);
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
			case ButtonId.MainMenuBottomHUDPlay:
				stateMachine.PushState(new GameStateSelectMode());
				break;
			case ButtonId.MainMenuTopHUDGemPlus:
			case ButtonId.MainMenuBottomHUDStore:
				ShowStore();
				break;
			case ButtonId.MainMenuSideBarLeaderboard:
				ShowLeaderboard();
				break;
			case ButtonId.ModeSelectFreeMode:
				PlayFreeMode();
				break;
			case ButtonId.HomeHeaderExplanator:
				stateMachine.PushState(new GameStateExplanatorPopup());
				break;
			case ButtonId.MainMenuSideBarSettings:
				stateMachine.PushState(new GameStateOptions());
				break;
			case ButtonId.NotEnoughGemsBack:
				CloseNotEnoughGems();
				break;
		}
	}

	private void ShowLeaderboard()
	{
		Screens.Instance.PopScreen(_gameScreenHomeHeader);
		Screens.Instance.PopScreen(_gameScreenHomeFooter);
		stateMachine.PushState(new GameStateLeaderboard());
	}

	private void PlayFreeMode()
	{
		Screens.Instance.PopScreen(_gameScreenHome);
		Screens.Instance.PopScreen(_gameScreenHomeFooter);
		stateMachine.PushState(new GameStateFreeMode());
	}

	private void CloseNotEnoughGems()
	{
		Screens.Instance.PopScreen(_gameScreenNotEnoughGems);
		Screens.Instance.BringToFront<GameScreenHomeHeader>();
	}

	private void PlayNetherMode()
	{
		Screens.Instance.PopScreen(_gameScreenHome);
		Screens.Instance.PopScreen(_gameScreenHomeFooter);
		Screens.Instance.PopScreen(_gameScreenHomeSideBar);
		stateMachine.PushState(new GameStateNetherMode());
	}

	private void ShowStore()
	{
		stateMachine.PushState(new GameStateStore());
	}

	public override void Disable()
	{
		GameEventsManager.Instance.RemoveGlobalListener(OnGameEvent);
	}

	public override void Exit()
	{
		Screens.Instance.PopScreen(_gameScreenHome);
		Screens.Instance.PopScreen(_gameScreenHomeFooter);
		Screens.Instance.PopScreen(_gameScreenHomeHeader);
		Screens.Instance.PopScreen(_gameScreenHomeSideBar);
	}
}