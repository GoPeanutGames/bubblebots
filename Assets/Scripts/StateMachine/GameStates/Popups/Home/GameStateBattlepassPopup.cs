using UnityEngine.Device;

public class GameStateBattlepassPopup : GameState
{
	private GamePopupBattlepass _gamePopupBattlepass;
	
	public override string GetGameStateName()
	{
		return "Game state explanator popup";
	}
	
	public override void Enter()
	{
		_gamePopupBattlepass = Screens.Instance.PushScreen<GamePopupBattlepass>();
		_gamePopupBattlepass.StartOpen();
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
			case ButtonId.BattlepassPopupClose:
				stateMachine.PopState();
				break;
			case ButtonId.BattlepassPopupBuy:
				Application.OpenURL("https://peanutgames.com/mint");
				break;
		}
	}
	
	public override void Disable()
	{
		GameEventsManager.Instance.RemoveGlobalListener(OnGameEvent);
	}

	public override void Exit()
	{
		Screens.Instance.PopScreen(_gamePopupBattlepass);
	}
}