public class GameStateChangeNickname : GameState
{
	private GamePopupChangeNickname _gamePopupChangeNickname;
	private GameScreenLoading _gameScreenLoading;

	public override string GetGameStateName()
	{
		return "Game state change nickname";
	}

	public override void Enter()
	{
		_gamePopupChangeNickname = Screens.Instance.PushScreen<GamePopupChangeNickname>();
		_gamePopupChangeNickname.StartOpen();
		_gamePopupChangeNickname.SetNicknameText(UserManager.Instance.GetPlayerUserName());
		Screens.Instance.BringToFront<GamePopupChangeNickname>();
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
			case ButtonId.ChangeNicknameClose:
				stateMachine.PopState();
				break;
			case ButtonId.ChangeNicknameOk:
				ChangeNickname();
				break;
		}
	}

	private void ChangeNickname()
	{
		_gameScreenLoading = Screens.Instance.PushScreen<GameScreenLoading>();
		Screens.Instance.BringToFront<GameScreenLoading>();
		UserManager.Instance.SetPlayerUserName(_gamePopupChangeNickname.GetNicknameText(), true,ChangeNicknameSuccess, ChangeNicknameFail);
	}

	private void ChangeNicknameSuccess(string result)
	{
		Screens.Instance.PopScreen<GameScreenLoading>();
		stateMachine.PopState();
	}

	private void ChangeNicknameFail(string reason)
	{
		Screens.Instance.PopScreen<GameScreenLoading>();
		_gamePopupChangeNickname.SetNickDuplicatedError();
	}

	public override void Disable()
	{
		GameEventsManager.Instance.RemoveGlobalListener(OnGameEvent);
	}

	public override void Exit()
	{
		_gamePopupChangeNickname.StartClose();
	}
}