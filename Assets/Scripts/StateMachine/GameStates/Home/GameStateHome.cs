using BubbleBots.Server.Player;

public class GameStateHome : GameState
{
	private GameScreenHomeFooter _gameScreenHomeFooter;
	private GameScreenHomeHeader _gameScreenHomeHeader;
	private GameScreenHomeSideBar _gameScreenHomeSideBar;
	private GameScreenHome _gameScreenHome;
	private GameScreenModeSelect _gameScreenModeSelect;
	private GameScreenLoading _gameScreenLoading;
	private GameScreenChangeNickname _gameScreenChangeNickname;
	private GameScreenNotEnoughGems _gameScreenNotEnoughGems;
	private GameScreenComingSoonGeneric _gameScreenComingSoonGeneric;
	private GameScreenComingSoonNether _gameScreenComingSoonNether;
	private GameScreenComingSoonItems _gameScreenComingSoonItems;

	private bool canPlayNetherMode = false;
	private bool canPlayFreeMode = false;

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
		canPlayNetherMode = false;
		canPlayFreeMode = false;
		//todo: hide these with animation instead of pop, so on enable won't need to have these
		// _gameScreenHome = Screens.Instance.PushScreen<GameScreenHome>(true);
		// _gameScreenHomeHeader = Screens.Instance.PushScreen<GameScreenHomeHeader>(true);
		// _gameScreenHomeFooter = Screens.Instance.PushScreen<GameScreenHomeFooter>(true);
		// _gameScreenHomeSideBar = Screens.Instance.PushScreen<GameScreenHomeSideBar>(true);
		_gameScreenHomeFooter.Show();
		_gameScreenHomeSideBar.Show();
		_gameScreenHomeHeader.Show();
		GameEventsManager.Instance.AddGlobalListener(OnGameEvent);
		ResetMainMenuLook();
		if (_gameScreenHomeHeader.AreResourcesSet() == false)
		{
			_gameScreenLoading = Screens.Instance.PushScreen<GameScreenLoading>();
		}

		Screens.Instance.BringToFront<GameScreenLoading>();
		GameScreenHomeHeader.ResourcesSet += ResourcesSet;
		_gameScreenHomeHeader.SetUsername(UserManager.Instance.GetPlayerUserName());
		UserManager.CallbackWithResources += ResourcesReceived;
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
				ShowModeSelect();
				break;
			case ButtonId.MainMenuBottomHUDFriends:
				ShowComingSoonGeneric();
				break;
			case ButtonId.MainMenuBottomHUDNetherpass:
				ShowComingSoonNether();
				break;
			case ButtonId.ComingSoonNetherpassClose:
				HideComingSoonNether();
				break;
			case ButtonId.MainMenuBottomHUDItems:
				ShowComingSoonItems();
				break;
			case ButtonId.ComingSoonItemsClose:
				HideComingSoonItems();
				break;
			case ButtonId.ModeSelectBackButton:
				HideModeSelect();
				break;
			case ButtonId.MainMenuTopHUDGemPlus:
			case ButtonId.MainMenuBottomHUDStore:
				ShowStore();
				break;
			case ButtonId.MainMenuSideBarLeaderboard:
				ShowLeaderboard();
				break;
			case ButtonId.ComingSoonGenericClose:
				HideComingSoonGeneric();
				break;
			case ButtonId.ModeSelectFreeModeTooltip:
				ShowFreeModeTooltip();
				break;
			case ButtonId.ModeSelectNetherModeTooltip:
				ShowNetherModeTooltip();
				break;
			case ButtonId.ModeSelectFreeModeTooltipBack:
			case ButtonId.ModeSelectNetherModeTooltipBack:
				HideFreeModeTooltip();
				break;
			case ButtonId.ModeSelectFreeMode:
				PlayFreeMode();
				break;
			case ButtonId.ModeSelectNethermode:
				TryPlayNetherMode();
				break;
			case ButtonId.MainMenuSideBarSettings:
				stateMachine.PushState(new GameStateOptions());
				break;
			case ButtonId.ChangeNicknameClose:
				CloseChangeNickname();
				break;
			case ButtonId.ChangeNicknameOk:
				ChangeNickname();
				CloseChangeNickname();
				break;
			case ButtonId.NotEnoughGemsBack:
				CloseNotEnoughGems();
				break;
			default:
				break;
		}
	}

	private void ShowLeaderboard()
	{
		Screens.Instance.PopScreen(_gameScreenHomeHeader);
		Screens.Instance.PopScreen(_gameScreenHomeFooter);
		stateMachine.PushState(new GameStateLeaderboard());
	}

	private void ShowComingSoonGeneric()
	{
		_gameScreenComingSoonGeneric = Screens.Instance.PushScreen<GameScreenComingSoonGeneric>();
	}

	private void HideComingSoonGeneric()
	{
		Screens.Instance.PopScreen(_gameScreenComingSoonGeneric);
	}

	private void ShowComingSoonNether()
	{
		_gameScreenComingSoonNether = Screens.Instance.PushScreen<GameScreenComingSoonNether>();
	}

	private void HideComingSoonNether()
	{
		Screens.Instance.PopScreen(_gameScreenComingSoonNether);
	}

	private void ShowComingSoonItems()
	{
		_gameScreenComingSoonItems = Screens.Instance.PushScreen<GameScreenComingSoonItems>();
	}

	private void HideComingSoonItems()
	{
		Screens.Instance.PopScreen(_gameScreenComingSoonItems);
	}

	private void ShowFreeModeTooltip()
	{
		_gameScreenModeSelect.ShowToolTipFreeMode();
	}

	private void ShowNetherModeTooltip()
	{
		_gameScreenModeSelect.ShowToolTipNetherMode();
	}

	private void HideFreeModeTooltip()
	{
		_gameScreenModeSelect.HideToolTips();
	}

	private void HideModeSelect()
	{
		Screens.Instance.PopScreen(_gameScreenModeSelect);
		_gameScreenHomeHeader.ShowPlayerInfoGroup();
	}

	private void ShowModeSelect()
	{
		_gameScreenModeSelect = Screens.Instance.PushScreen<GameScreenModeSelect>();
		Screens.Instance.BringToFront<GameScreenHomeHeader>();
		_gameScreenHomeHeader.HidePlayerInfoGroup();
	}

	private void PlayFreeMode()
	{
		Screens.Instance.PopScreen(_gameScreenHome);
		Screens.Instance.PopScreen(_gameScreenHomeFooter);
		Screens.Instance.PopScreen(_gameScreenModeSelect);
		stateMachine.PushState(new GameStateFreeMode());
	}

	private void TryPlayNetherMode()
	{
		if (canPlayNetherMode)
		{
			PlayNetherMode();
		}
		else
		{
			_gameScreenNotEnoughGems = Screens.Instance.PushScreen<GameScreenNotEnoughGems>();
		}

		UserManager.Instance.GetPlayerResources();
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
		Screens.Instance.PopScreen(_gameScreenModeSelect);
		Screens.Instance.PopScreen(_gameScreenHomeSideBar);
		stateMachine.PushState(new GameStateNetherMode());
	}

	private void ShowStore()
	{
		stateMachine.PushState(new GameStateStore());
	}

	private void OpenChangeNickname()
	{
		_gameScreenChangeNickname = Screens.Instance.PushScreen<GameScreenChangeNickname>();
		_gameScreenChangeNickname.SetNicknameText(UserManager.Instance.GetPlayerUserName());
	}

	private void CloseChangeNickname()
	{
		Screens.Instance.PopScreen(_gameScreenChangeNickname);
	}

	private void ChangeNickname()
	{
		UserManager.Instance.SetPlayerUserName(_gameScreenChangeNickname.GetNicknameText(), true);
		_gameScreenHomeHeader.SetUsername(_gameScreenChangeNickname.GetNicknameText());
		CloseChangeNickname();
	}

	public override void Disable()
	{
		GameEventsManager.Instance.RemoveGlobalListener(OnGameEvent);
		UserManager.CallbackWithResources -= ResourcesReceived;
	}

	private void ResourcesReceived(GetPlayerWallet wallet)
	{
		canPlayNetherMode = wallet.gems > 0;
		canPlayFreeMode = wallet.energy > 0;
	}

	public override void Exit()
	{
		Screens.Instance.PopScreen(_gameScreenHome);
		Screens.Instance.PopScreen(_gameScreenHomeFooter);
		Screens.Instance.PopScreen(_gameScreenHomeHeader);
		Screens.Instance.PopScreen(_gameScreenHomeSideBar);
	}
}