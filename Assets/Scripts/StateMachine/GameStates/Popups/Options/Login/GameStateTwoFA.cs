public class GameStateTwoFA : GameState
{
	private GamePopupTwoFA _gamePopupTwoFa;
	private GameScreenLoading _gameScreenLoading;
	private GameScreenDarkenedBg _darkenedBg;
	
	public override string GetGameStateName()
	{
		return "game state 2fa";
	}

	public override void Enable()
	{
		_darkenedBg = Screens.Instance.PushScreen<GameScreenDarkenedBg>();
		_gamePopupTwoFa = Screens.Instance.PushScreen<GamePopupTwoFA>();
		GameEventsManager.Instance.AddGlobalListener(OnGameEvent);
	}
    
	private void OnGameEvent(GameEventData gameEvent)
	{
		if (gameEvent.eventName == GameEvents.ButtonTap)
		{
			OnButtonTap(gameEvent);
		}
	}

	private void OnButtonTap(GameEventData data)
	{
		GameEventString buttonTapData = data as GameEventString;
		switch (buttonTapData.stringData)
		{
			case ButtonId.LoginCodeSubmit:
				if (_gamePopupTwoFa.CodeValidation())
				{
					_gameScreenLoading = Screens.Instance.PushScreen<GameScreenLoading>();
					UserManager.Instance.loginManager.Submit2FaCode(_gamePopupTwoFa.GetLoginInputFieldCode(), TwoFaCodeSuccess, TwoFaCodeFail);
				}
				break;
			case ButtonId.LoginCodeDidntReceive:
				UserManager.Instance.loginManager.SignIn("", "", true, null, null);
				break;
		}
	}
	
	private void TwoFaCodeSuccess()
	{
		Screens.Instance.PopScreen(_gameScreenLoading);
		stateMachine.PopState();
	}

	private void TwoFaCodeFail()
	{
		Screens.Instance.PopScreen(_gameScreenLoading);
		_gamePopupTwoFa.Set2FaError();
	}
	
	public override void Disable()
	{
		GameEventsManager.Instance.RemoveGlobalListener(OnGameEvent);
		Screens.Instance.PopScreen(_darkenedBg);
		Screens.Instance.PopScreen(_gamePopupTwoFa);
	}
}
