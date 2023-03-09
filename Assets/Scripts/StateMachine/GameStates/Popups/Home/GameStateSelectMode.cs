using BubbleBots.Server.Player;
using UnityEngine;

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
				SoundManager.Instance.PlayModeSelectedSfx();
				break;
			case ButtonId.ModeSelectFreeModeTooltip:
				stateMachine.PushState(new GameStateFreeModeTooltip());
				SoundManager.Instance.PlayModeSelectedSfx();
				break;
			case ButtonId.ModeSelectNetherModeTooltip:
				stateMachine.PushState(new GameStateNetherModeTooltip());
				break;
			case ButtonId.ModeSelectNethermode:
				_gameScreenLoading = Screens.Instance.PushScreen<GameScreenLoading>();
				CheckForBattlePass();
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

	private void CheckForBattlePass()
	{
		ServerManager.Instance.GetPlayerDataFromServer(PlayerAPI.Battlepass, BattlePassSuccess, UserManager.Instance.GetPlayerWalletAddress(), BattlePassFail);
	}

	private void BattlePassSuccess(string data)
	{
		GetBattlePassResponse response = JsonUtility.FromJson<GetBattlePassResponse>(data);
		if (response.exists)
		{
			PlayNetherMode();
		}
		else
		{
			BattlePassFail("no battlepass");
		}
	}

	private void BattlePassFail(string data)
	{
		UserManager.CallbackWithResources += ResourcesReceived;
		UserManager.Instance.GetPlayerResources();
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
		PlayNetherMode();
	}

	private void PlayNetherMode()
	{
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